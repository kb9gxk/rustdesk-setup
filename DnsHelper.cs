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
                    string trimmedRecord = record.Trim();
                    InstallationSettings.log?.WriteLine($"Found DNS TXT record: {trimmedRecord}");

                    if (trimmedRecord.StartsWith(ConfigRecordName))
                    {
                        rustdeskCfg = trimmedRecord.Substring(ConfigRecordName.Length).TrimStart('=');
                    }
                    else if (trimmedRecord.StartsWith(PasswordRecordName))
                    {
                        string encryptedPw = trimmedRecord.Substring(PasswordRecordName.Length).TrimStart('=');
                        rustdeskPw = EncryptionHelper.Decrypt(encryptedPw);
                    }
                    else if (trimmedRecord.StartsWith(KeyRecordName))
                    {
                        encryptionKey = trimmedRecord.Substring(KeyRecordName.Length).TrimStart('=');
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
            udpClient.Connect(dnsServer, 53);
            udpClient.Send(query, query.Length);

            IPEndPoint remoteEP = new(IPAddress.Any, 0);
            return udpClient.Receive(ref remoteEP);
        }

        private static List<string> ParseTxtRecords(byte[] response)
        {
            List<string> txtRecords = new();
            int index = 12; // Skip header

            // Skip the query section
            while (response[index] != 0)
                index++;
            index += 5; // Move past NULL terminator, QTYPE, and QCLASS

            // Skip Answer Section's Name (compression or domain name)
            if (response[index] >= 192) index += 2; // Name is compressed
            else while (response[index] != 0) index++; // Regular domain
            index++;

            // Read Type, Class, TTL (Skip these)
            index += 8;

            // Read Data Length
            int txtLength = response[index + 1];
            index += 2;

            // Read TXT Record Data
            txtRecords.Add(Encoding.ASCII.GetString(response, index + 1, txtLength - 1));

            return txtRecords;
        }
    }
}
