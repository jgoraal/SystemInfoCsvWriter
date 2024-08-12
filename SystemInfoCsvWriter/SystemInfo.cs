using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemInfoCsvWriter
{
    internal sealed class SystemInfo
    {
        internal string Hostname { get; set; }
        internal string Username { get; set; }
        internal string Model { get; set; }
        internal string Serial { get; set; }
        internal string Os { get; set; }
        internal string Cpu { get; set; }
        internal string Ram { get; set; }
        internal string Memory { get; set; }
        internal Dictionary<string,string> Ip { get; set; }

        private SystemInfo() { }

        public static async Task<SystemInfo> CreateAsync()
        {
            var systemInfo = new SystemInfo();
            systemInfo.Hostname = systemInfo.GetHostname();
            systemInfo.Username = systemInfo.GetUsername();
            systemInfo.Model = await systemInfo.GetModelAsync();
            systemInfo.Serial = await systemInfo.GetSerialAsync();
            systemInfo.Os = await systemInfo.GetOsAsync();
            systemInfo.Cpu = await systemInfo.GetCpuAsync();
            systemInfo.Ram = await systemInfo.GetRamAsync();
            systemInfo.Memory = await systemInfo.GetMemoryAsync();
            systemInfo.Ip = systemInfo.GetEthernetIpsAsync();
            return systemInfo;
        }

        private string GetUsername()
        {
            return Environment.UserName;
        }

        private string GetHostname()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception ex)
            {
                throw new Exception("Błąd podczas pobierania nazwy hosta", ex);
            }
        }

        private async Task<string> GetModelAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznany";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return obj["Model"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania modelu", ex);
                }
                return "Nieznany";
            });
        }

        private async Task<string> GetSerialAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznany";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return obj["SerialNumber"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania numeru seryjnego", ex);
                }
                return "Nieznany";
            });
        }

        private async Task<string> GetOsAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznany";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return obj["Caption"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania systemu operacyjnego", ex);
                }
                return "Nieznany";
            });
        }

        private async Task<string> GetCpuAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznane";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name,MaxClockSpeed FROM Win32_Processor"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return obj["Name"].ToString().Trim() + " @ " + $"{Convert.ToDouble(obj["MaxClockSpeed"]) / 1000:F2}GHz";
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania CPU", ex);
                }
                return "Nieznane";
            });
        }

        private async Task<string> GetRamAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznane";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
                    {
                        double totalCapacity = 0;
                        foreach (var obj in searcher.Get())
                        {
                            totalCapacity += Convert.ToDouble(obj["Capacity"]);
                        }
                        return (totalCapacity / (1024 * 1024 * 1024)) + " GB";
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania RAM", ex);
                }
            });
        }

        private async Task<string> GetMemoryAsync()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return "Nieznane";
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Model,Size FROM Win32_DiskDrive"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            return $"{obj["Model"]} ({Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024):F2} GB)";
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Błąd podczas pobierania pamięci", ex);
                }
                return "Nieznane";
            });
        }

        private Dictionary<string, string> GetEthernetIpsAsync()
        {
            try
            {
                var interfaces = new Dictionary<string,string>();
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Filtrujemy tylko interfejsy typu Ethernet
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet && ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var ipProps = ni.GetIPProperties();
                        foreach (var ip in ipProps.UnicastAddresses)
                        {
                            // Filtrujemy tylko adresy IPv4
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                interfaces.Add(ni.Name.Trim(), ip.Address.ToString().Trim());
                            }
                        }
                    }
                }

                if (interfaces.Count == 0)
                    interfaces.Add("Nieznany", "Nieznany");

                return interfaces.OrderBy(k => k.Key).ToDictionary(k => k.Key,v => v.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Błąd podczas pobierania adresów IP dla Ethernetu", ex);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Informacje o systemie:");
            builder.AppendLine($"Hostname - {Hostname}");
            builder.AppendLine($"Username - {Username}");
            builder.AppendLine($"Model - {Model}");
            builder.AppendLine($"Serial - {Serial}");
            builder.AppendLine($"OS - {Os}");
            builder.AppendLine($"CPU - {Cpu}");
            builder.AppendLine($"RAM - {Ram}");
            builder.AppendLine($"Memory - {Memory}");
            builder.AppendLine($"IP - {Ip}");

            return builder.ToString();
        }
    }
}
