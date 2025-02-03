using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class DnsHelper
    {
        private const string ConfigRecordName = "_rdcfg";
        private const string PasswordRecordName = "_rdpw";
        private const string KeyRecordName = "_rdkey";

        internal static async Task<(string? rustdeskCfg, string? rustdeskPw, string? encryptionKey)> GetRustdeskConfigFromDnsAsync()
        {
            string? rustdeskCfg = null;
            string? rustdeskPw = null;
            string? encryptionKey = null;

            try
            {
                InstallationSettings.log?.WriteLine("Starting DNS TXT record lookup for kb9gxk.net...");
                var txtRecords = await LookupTxtRecordsAsync("kb9gxk.net");
                InstallationSettings.log?.WriteLine("Finished DNS TXT record lookup.");

                foreach (var record in txtRecords)
                {
                    string trimmedRecord = record.Trim().TrimStart('\uFEFF'); // Trim BOM and whitespace
                    InstallationSettings.log?.WriteLine($"Found DNS TXT record: {trimmedRecord}");

                    if (trimmedRecord.StartsWith(ConfigRecordName + "="))
                    {
                        rustdeskCfg = trimmedRecord.Substring(ConfigRecordName.Length + 1).Trim();
                    }
                    else if (trimmedRecord.StartsWith(PasswordRecordName + "="))
                    {
                        string encryptedPw = trimmedRecord.Substring(PasswordRecordName.Length + 1).Trim();
                        if (encryptedPw.StartsWith("="))
                        {
                            encryptedPw = encryptedPw.Substring(1);
                        }
                        rustdeskPw = EncryptionHelper.Decrypt(encryptedPw);
                    }
                    else if (trimmedRecord.StartsWith(KeyRecordName + "="))
                    {
                        encryptionKey = trimmedRecord.Substring(KeyRecordName.Length + 1).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching DNS TXT records: {ex.Message}");
            }

            return (rustdeskCfg, rustdeskPw, encryptionKey);
        }

        private static async Task<List<string>> LookupTxtRecordsAsync(string domain)
        {
            return await Task.Run(() =>
            {
                List<string> txtRecords = new();
                var dnsServers = GetSystemDnsServers();

                if (dnsServers.Count == 0)
                {
                    InstallationSettings.log?.WriteLine("❌ No system DNS servers found.");
                    return txtRecords;
                }

                foreach (var dnsServer in dnsServers)
                {
                    InstallationSettings.log?.WriteLine($"🔍 Querying DNS server: {dnsServer}");
                    byte[] query = BuildDnsQuery(domain);
                    byte[] response = SendDnsQuery(query, dnsServer);

                    if (response != null)
                    {
                        txtRecords.AddRange(ParseTxtRecords(response));
                        if (txtRecords.Count > 0) break; // Stop once we get a valid response
                    }
                }

                return txtRecords;
            });
        }

        private static List<string> GetSystemDnsServers()
        {
            List<string> dnsServers = new();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var dns in adapter.GetIPProperties().DnsAddresses)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                        {
                            dnsServers.Add(dns.ToString());
                        }
                    }
                }
            }

            return dnsServers;
        }

        private static byte[] BuildDnsQuery(string domain)
        {
            List<byte> packet = new();

            // Transaction ID (random)
            packet.AddRange(new byte[] { 0x12, 0x34 });

            // Flags: Standard Query (0x0100)
            packet.AddRange(new byte[] { 0x01, 0x00 });

            // Questions: 1
            packet.AddRange(new byte[] { 0x00, 0x01 });

            // Answer, Authority, and Additional RRs: 0
            packet.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            // Encode domain name
            foreach (var part in domain.Split('.'))
            {
                packet.Add((byte)part.Length);
                packet.AddRange(Encoding.ASCII.GetBytes(part));
            }
            packet.Add(0); // End of domain name

            // Query Type: TXT (0x0010)
            packet.AddRange(new byte[] { 0x00, 0x10 });

            // Query Class: IN (0x0001)
            packet.AddRange(new byte[] { 0x00, 0x01 });

            return packet.ToArray();
        }

        private static byte[] SendDnsQuery(byte[] query, string dnsServer)
        {
            using UdpClient udpClient = new();
            try
            {
                udpClient.Connect(dnsServer, 53);
                udpClient.Send(query, query.Length);

                IPEndPoint remoteEP = new(IPAddress.Any, 0);
                return udpClient.Receive(ref remoteEP);
            }
            catch (SocketException ex)
            {
                InstallationSettings.log?.WriteLine($"Error sending/receiving DNS query to {dnsServer}: {ex.Message}");
                return null;
            }
        }

        private static List<string> ParseTxtRecords(byte[] response)
        {
            List<string> txtRecords = new();
            int index = 12; // Skip header

            // Skip the query section
            while (response[index] != 0)
                index++;
            index += 5; // Move past NULL terminator, QTYPE, and QCLASS

            while (index < response.Length)
            {
                // Skip Answer Section's Name (compression or domain name)
                if (response[index] >= 192)
                {
                    index += 2; // Name is compressed
                }
                else
                {
                    while (response[index] != 0) index++; // Regular domain
                    index++;
                }

                if (index + 8 >= response.Length) break; // Check if enough bytes left for Type, Class, TTL

                // Read Type, Class, TTL (Skip these)
                index += 8;

                if (index + 2 >= response.Length) break; // Check if enough bytes left for Data Length
                // Read Data Length
                int txtLength = response[index + 1];
                index += 2;

                if (index + txtLength > response.Length) break; // Check if enough bytes left for TXT data

                // Read TXT Record Data
                if (txtLength > 0)
                {
                    txtRecords.Add(Encoding.UTF8.GetString(response, index, txtLength));
                }
                index += txtLength;
            }

            return txtRecords;
        }
    }
}