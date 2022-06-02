//using System.Diagnostics;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Mallenom.WebApi.ResourcesUsage.Services
{
    public class MetricsService
    {
        private static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            return isUnix;
        }

        public List<string?> GetCpuMetrics()
        {
            var result = new List<string?>();

            if (!IsUnix())
            {
                //var man = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");

                //foreach (var o in man.Get())
                //{
                //    var obj = (ManagementObject)o;
                //    result.Add(obj["LoadPercentage"].ToString());
                //}
                var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName);
                cpuCounter.NextValue();
                System.Threading.Thread.Sleep(300); //This avoid that answer always 0
                result.Add(((int)cpuCounter.NextValue()).ToString());
            }
            else
            {
                var output = BashCommands.GetCpuMetrics();
                var cpuLoadArr = output.Split(" ");
                cpuLoadArr[0] = cpuLoadArr[0][2..];
                result.Add(cpuLoadArr[0]);
            }

            return result;
        }

        public MemoryMetrics GetMemoryMetrics()
        {
            if (!IsUnix())
            {
                string? output;

                var info = new ProcessStartInfo();
                info.FileName = "wmic";
                info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
                info.RedirectStandardOutput = true;

                using (var process = Process.Start(info))
                {
                    output = process?.StandardOutput.ReadToEnd();
                }

                var lines = output?.Trim().Split("\n");
                var freeMemoryParts = lines?[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
                var totalMemoryParts = lines?[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
                    Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
                };
                metrics.Used = metrics.Total - metrics.Free;

                return metrics;
            }
            else
            {
                string? output;

                var info = new ProcessStartInfo("free -m")
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"free -m\"",
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process?.StandardOutput.ReadToEnd();
                }

                var lines = output?.Split("\n");
                var memory = lines?[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = double.Parse(memory[1]),
                    Used = double.Parse(memory[2]),
                    Free = double.Parse(memory[3])
                };
                return metrics;
            }
        }
        public DisksMetrics GetDisksMetrics()
        {
            DisksMetrics dm1 = new DisksMetrics();
            if (!IsUnix())
            {
                ConnectionOptions options = new ConnectionOptions();
                ManagementScope scope = new ManagementScope("\\\\localhost\\root\\cimv2", options);
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                SelectQuery query1 = new SelectQuery("Select * from Win32_LogicalDisk");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection queryCollection = searcher.Get();
                ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(scope, query1);
                ManagementObjectCollection queryCollection1 = searcher1.Get();

                foreach (ManagementObject mo in queryCollection1)
                {
                    // Display Logical Disks information
                    string? name = mo["Name"].ToString();
                    string? freespace = (Convert.ToInt64(mo["FreeSpace"]) / 1000000000).ToString();
                    string? fullsize = (Convert.ToInt64(mo["Size"]) / 1000000000).ToString();
                    string? usedsize = (Convert.ToDouble(fullsize) - Convert.ToDouble(freespace)).ToString();
                    string? usedsizeonbar = "width: " + Convert.ToInt64((Convert.ToDouble(fullsize) - Convert.ToDouble(freespace)) / Convert.ToDouble(fullsize) * 100).ToString() + "%";
                    dm1.Drives.Add(new DisksMetrics.Drive(name, freespace, fullsize, usedsize));
                }
            }
            else
            {
                string dfresult = BashCommands.GetDiskSpace();
                string[] dfresultar = dfresult.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < dfresultar.Length; i++)
                {
                    if (dfresultar[i].Contains("sda"))
                    {
                        string name = dfresultar[i];
                        /*name = name.Insert(name.Length - 9, "*");*/
                        name = name.Substring(dfresultar[i].Length - 9);
                        string freespace = Math.Round(Convert.ToDouble(dfresultar[i + 3]) / Math.Pow(10, 6), 2).ToString();
                        string fullsize = Math.Round(Convert.ToDouble(dfresultar[i + 1]) / Math.Pow(10, 6), 2).ToString();
                        string usedsize = Math.Round(Convert.ToDouble(dfresultar[i + 2]) / Math.Pow(10, 6), 2).ToString();
                        string usedsizeonbar = "width: " + Convert.ToInt64((Convert.ToDouble(fullsize) - Convert.ToDouble(freespace)) / Convert.ToDouble(fullsize) * 100).ToString() + "%";
                        dm1.Drives.Add(new DisksMetrics.Drive(name, freespace, fullsize, usedsize));
                    }
                }
            }
            return dm1;
        }
    }
}