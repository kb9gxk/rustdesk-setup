using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RustdeskSetup
{
    internal static class DnsHelper
    {
        private const string ConfigRecordName = "_rdcfg";
        private const string PasswordRecordName = "_rdpw";

        internal static async Task<(string? rustdeskCfg, string? rustdeskPw)> GetRustdeskConfigFromDnsAsync()
        {
            string? rustdeskCfg = null;
            string? rustdeskPw = null;
            try
            {
                var dnsRecords = await LookupTxtRecordsAsync("kb9gxk.net");
                foreach (var record in dnsRecords)
                {
                    string trimmedRecord = record.Trim();
                    if (trimmedRecord.StartsWith(ConfigRecordName))
                    {
                        string encryptedCfg = trimmedRecord.Substring(ConfigRecordName.Length).TrimStart('=');
                        rustdeskCfg = EncryptionHelper.Decrypt(encryptedCfg);
                    }
                    else if (trimmedRecord.StartsWith(PasswordRecordName))
                    {
                        string encryptedPw = trimmedRecord.Substring(PasswordRecordName.Length).TrimStart('=');
                        rustdeskPw = EncryptionHelper.Decrypt(encryptedPw);
                    }
                }
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Error fetching DNS TXT records: {ex.Message}");
            }
            return (rustdeskCfg, rustdeskPw);
        }

        private static async Task<List<string>> LookupTxtRecordsAsync(string domain)
        {
            List<string> txtRecords = new List<string>();
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(domain);
                if (addresses.Length > 0)
                {
                    using (var client = new UdpClient())
                    {
                        var serverEndpoint = new IPEndPoint(addresses[0], 53);
                        byte[] query = BuildDnsQuery(domain, DnsQueryType.TXT);

                        await client.SendAsync(query, query.Length, serverEndpoint);
                        var result = await client.ReceiveAsync();
                        byte[] response = result.Buffer;
                        txtRecords = ParseDnsResponse(response);
                    }
                }
                else
                {
                    InstallationSettings.log?.WriteLine($"No IP addresses found for domain: {domain}");
                }
            }
            catch (SocketException ex)
            {
                InstallationSettings.log?.WriteLine($"Socket Exception while resolving DNS TXT records: {ex.Message}");
            }
            catch (Exception ex)
            {
                InstallationSettings.log?.WriteLine($"Exception while resolving DNS TXT records: {ex.Message}");
            }
            return txtRecords;
        }

        private static byte[] BuildDnsQuery(string domain, DnsQueryType queryType)
        {
            var domainParts = domain.Split('.');
            var query = new List<byte>
        {
            0x01, 0x00, // Transaction ID
            0x01, 0x00, // Flags: Standard query
            0x00, 0x01, // Questions: 1
            0x00, 0x00, // Answer RRs: 0
            0x00, 0x00, // Authority RRs: 0
            0x00, 0x00  // Additional RRs: 0
        };

            foreach (var part in domainParts)
            {
                query.Add((byte)part.Length);
                query.AddRange(Encoding.ASCII.GetBytes(part));
            }

            query.Add(0x00); // End of domain
            query.AddRange(new byte[] { 0x00, (byte)queryType, 0x00, 0x01 }); // Query type (TXT) and class (IN)

            return query.ToArray();
        }

        private enum DnsQueryType : ushort
        {
            A = 0x0001,
            NS = 0x0002,
            CNAME = 0x0005,
            MX = 0x000F,
            TXT = 0x0010,
            AAAA = 0x001C,
            SRV = 0x0021,
        }

        private static List<string> ParseDnsResponse(byte[] response)
        {
            List<string> txtRecords = new List<string>();
            int index = 12; // Skip header
            // Skip Questions
            while (response[index] != 0)
            {
                index += response[index] + 1;
            }
            index += 5; // Skip QTYPE, QCLASS
            ushort answerCount = (ushort)((response[6] << 8) | response[7]);
            for (int i = 0; i < answerCount; i++)
            {
                // Skip Name
                while (response[index] != 0)
                {
                    index += response[index] + 1;
                }
                index++;
                ushort type = (ushort)((response[index] << 8) | response[index + 1]);
                index += 8; // Skip TYPE, CLASS, TTL
                ushort rdLength = (ushort)((response[index] << 8) | response[index + 1]);
                index += 2;
                if (type == (ushort)DnsQueryType.TXT)
                {
                    txtRecords.Add(Encoding.UTF8.GetString(response, index + 1, rdLength - 1));
                }
                index += rdLength;
            }
            return txtRecords;
        }
    }
}