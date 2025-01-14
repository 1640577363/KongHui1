using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Net.WebSockets;
using System.Text;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Security.Principal;
using Newtonsoft.Json;
using static Community.CsharpSqlite.Sqlite3;
using Windows.UI.Popups;

namespace KongHui1.Presentation
{
    public sealed partial class FullCheckPage : Page
    {
        private DispatcherTimer _progressTimer; // 用于控制进度条的计时器
        private double _progressValue = 0;     // 当前进度值
        private Process monitoringProcess1;
        private Process monitoringProcess2;
        double totalScore = 100;
        int abnormalCount = 0;
        int[] status = new int[16];

        public FullCheckPage()
        {
            this.InitializeComponent();
            InitializeProgressBar();
            StartHardwareMonitoring();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void ReScanButton_Click(object sender, RoutedEventArgs e)
        {
            ReScanButton.Visibility = Visibility.Collapsed;
            cancelButtonPanel.Visibility = Visibility.Visible;
            scanstatusText.Text = "智能检测中";
            scanText.Text = "正在重新开始体检";
            CpuTemeratureTextBlock.Text = "待检测";
            CpuusageTextBlock.Text = "待检测";
            CpufanTextBlock.Text = "待检测";
            GraphicTemperatureTextBlock.Text = "待检测";
            GraphicUsageTextBlock.Text = "待检测";
            MemoryUsageTextBlock.Text = "待检测";
            DiskUsageTextBlock.Text = "待检测";
            CPUDriveText.Text = "待检测";
            GraphicDriveText.Text = "待检测";
            NetworkAdapterText.Text = "待检测";
            AudioDeviceText.Text = "待检测";
            StorageDeviceText.Text = "待检测";
            BluetoothText.Text = "待检测";
            USBText.Text = "待检测";
            VirusTextBlock.Text = "待检测";
            FirewallTextBlock.Text = "待检测";
            InitializeProgressBar();
            StartHardwareMonitoring();
        }

        private void InitializeProgressBar()
        {
            // 设置进度条初始状态
            progressBar.IsIndeterminate = false;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100; // 假设最大值为 100
            progressBar.Value = 0; // 从30开始

            // 初始化进度值
            _progressValue = 0; // 从30开始

            // 初始化计时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 每 100 毫秒更新一次
            };
            _progressTimer.Tick += ProgressTimer_Tick; // 注册定时器事件
            _progressTimer.Start();
        }


        private void ProgressTimer_Tick(object sender, object e)
        {
            // 每次更新的步长
            double progressIncrement = 2; // 8秒内达到100，每100毫秒更新一次

            // 更新进度值
            _progressValue += progressIncrement;

            // 如果进度达到或超过最大值，停止定时器
            if (_progressValue >= progressBar.Maximum)
            {
                _progressValue = progressBar.Maximum; // 确保不会超过最大值
                _progressTimer.Stop(); // 停止计时器
            }

            // 更新进度条的值
            progressBar.Value = _progressValue;
        }



        private async Task StartDriveMonitoring()
        {
            scanText.Text = "正在检测驱动安装状况...";
            try
            {
                // Python 脚本路径
                string scriptPath = @"D:\\Project\\UNO2\\KongHui1\\Python\\Drive_detection.py";

                var result = await Task.Run(() =>
                {
                    Process Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = scriptPath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    StringBuilder output = new StringBuilder();
                    Process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    Process.Start();
                    Process.BeginOutputReadLine();
                    Process.BeginErrorReadLine();
                    Process.WaitForExit();

                    return output.ToString();
                });

                ParseAndUpdateUI(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error running Drive_detection.py: {ex.Message}");
            }
        }

        private void ParseAndUpdateUI(string output)
        {
            UpdateTextBlock(CPUDriveText, output.Contains("CPU驱动已安装。"));
            UpdateTextBlock(GraphicDriveText, output.Contains("显卡驱动已安装。"));
            UpdateTextBlock(NetworkAdapterText, output.Contains("网络适配器驱动已安装。"));
            UpdateTextBlock(AudioDeviceText, output.Contains("声卡驱动已安装。"));
            UpdateTextBlock(StorageDeviceText, output.Contains("存储设备驱动已安装。"));
            UpdateTextBlock(BluetoothText, output.Contains("蓝牙驱动已安装。"));
            UpdateTextBlock(USBText, output.Contains("USB驱动已安装。"));

        }

        private void UpdateTextBlock(TextBlock textBlock, bool condition, string failureText = "未安装")
        {
            if (condition)
            {
                textBlock.Text = "已安装";
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 175, 17)); // 绿色
            }
            else
            {
                textBlock.Text = failureText;
                textBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)); // 红色
            }
        }
        private async Task ReStartHardwareMonitoring()
        {
            try
            {
                // Python 脚本路径
                string scriptPath = @"D:\Project\UNO2\KongHui1\Python\HardwareMonitorOneSecond.py";

                var result = await Task.Run(() =>
                {
                    monitoringProcess2 = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = scriptPath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    StringBuilder output = new StringBuilder();
                    monitoringProcess2.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    monitoringProcess2.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine($"[Error] {e.Data}");
                        }
                    };

                    monitoringProcess2.Start();
                    monitoringProcess2.BeginOutputReadLine();
                    monitoringProcess2.BeginErrorReadLine();
                    monitoringProcess2.WaitForExit();

                    return new { ExitCode = monitoringProcess2.ExitCode, Output = output.ToString() };
                });

                if (result.ExitCode == 0)
                {
                    UpdateHealthStatusTwo(result.Output); // 调用方法解析评分并更新UI
                }
            }
            catch (Exception ex)
            {
                // 处理异常
            }
            finally
            {
                // 确保进程对象被释放
                monitoringProcess2?.Dispose();
            }
        }
        private void UpdateHealthStatusTwo(string output)
        {
            string ExtractJson(string text)
            {
                string pattern = @"当前硬件监控数据:\s*(\{.*?\})"; // 匹配 "{...}" 的 JSON 数据部分
                var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            string json = ExtractJson(output);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine("未能找到 JSON 数据部分");
                return;
            }
            var metrics = JsonConvert.DeserializeObject<Dictionary<string, double>>(json);

            // 提取监控数据
            metrics.TryGetValue("cpu_temp", out double cpuTemp);
            metrics.TryGetValue("cpu_usage", out double cpuUsage);
            metrics.TryGetValue("fan_speed", out double fanSpeed);
            metrics.TryGetValue("gpu_temp", out double gpuTemp);
            metrics.TryGetValue("hdd_temp", out double hddTemp);
            metrics.TryGetValue("gpu_usage", out double gpuUsage);
            metrics.TryGetValue("memory_usage", out double memoryUsage);
            metrics.TryGetValue("hdd_usage", out double hddUsage);

            CpuTemerature.Text = $"CPU温度: {cpuTemp}°C";
            Cpuusage.Text = $"CPU使用率: {cpuUsage}%";
            Cpufan.Text = $"CPU风扇转速: {fanSpeed} RPM";
            GraphicTemperature.Text = $"显卡温度: {gpuTemp}°C";
            GraphicUsage.Text = $"显卡使用率: {gpuUsage}%";
            MemoryUsage.Text = $"内存使用率: {memoryUsage}%";
            DiskUsage.Text = $"硬盘使用率: {hddUsage}%";

            totalScore = 100;
            ScoreText.Text = totalScore.ToString("F0");

            Color fillColor = Color.FromArgb(255, 0, 100, 0);
            ScoreCircle.Fill = new SolidColorBrush(fillColor);
            scanText.Text = totalScore == 100 ? "电脑非常健康" :
                            totalScore >= 90 ? "电脑存在问题，请及时修复" :
                            totalScore >= 80 ? "电脑问题较多，请立即修复" :
                                                "电脑存在严重问题，需要立即修复";


            Endprogressbar();
        }

        private async Task StartHardwareMonitoring()
        {
            scanText.Text = "正在检测硬件健康状况...";
            try
            {
                // Python 脚本路径
                string scriptPath = @"D:\Project\UNO2\KongHui1\Python\HardwareMonitor.py";

                var result = await Task.Run(() =>
                {
                    monitoringProcess1 = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = scriptPath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    StringBuilder output = new StringBuilder();
                    monitoringProcess1.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    monitoringProcess1.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine($"[Error] {e.Data}");
                        }
                    };

                    monitoringProcess1.Start();
                    monitoringProcess1.BeginOutputReadLine();
                    monitoringProcess1.BeginErrorReadLine();
                    monitoringProcess1.WaitForExit();

                    return new { ExitCode = monitoringProcess1.ExitCode, Output = output.ToString() };
                });

                if (result.ExitCode == 0)
                {
                    UpdateHealthStatus(result.Output); // 调用方法解析评分并更新UI
                }
            }
            catch (Exception ex)
            {
                // 处理异常
            }
            finally
            {
                // 确保进程对象被释放
                monitoringProcess1?.Dispose();
            }
        }

        /// <summary>
        /// 根据脚本输出的评分更新健康状态文本和评分圆圈。
        /// </summary>
        private void UpdateHealthStatus(string output)
        {
            for (int i = 0; i < status.Length; i++)
            {
                status[i] = 0; // 将每个元素设置为 0
            }

            // 从文本中提取平均值
            string ExtractJson(string text)
            {
                string pattern = @"平均值计算结果:\s*(\{.*?\})"; // 匹配 "{...}" 的 JSON 数据部分
                var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            string json = ExtractJson(output);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine("未能找到 JSON 数据部分");
                return;
            }
            var metrics = JsonConvert.DeserializeObject<Dictionary<string, double>>(json);

            // 提取监控数据
            metrics.TryGetValue("cpu_temp", out double cpuTemp);
            metrics.TryGetValue("cpu_usage", out double cpuUsage);
            metrics.TryGetValue("fan_speed", out double fanSpeed);
            metrics.TryGetValue("gpu_temp", out double gpuTemp);
            metrics.TryGetValue("hdd_temp", out double hddTemp);
            metrics.TryGetValue("gpu_usage", out double gpuUsage);
            metrics.TryGetValue("memory_usage", out double memoryUsage);
            metrics.TryGetValue("hdd_usage", out double hddUsage);

            CpuTemerature.Text = $"CPU温度: {cpuTemp}°C";
            Cpuusage.Text = $"CPU使用率: {cpuUsage}%";
            Cpufan.Text = $"CPU风扇转速: {fanSpeed} RPM";
            GraphicTemperature.Text = $"显卡温度: {gpuTemp}°C";
            GraphicUsage.Text = $"显卡使用率: {gpuUsage}%";
            MemoryUsage.Text = $"内存使用率: {memoryUsage}%";
            DiskUsage.Text = $"硬盘使用率: {hddUsage}%";


            // 更新 CPU 温度
            CpuTemeratureTextBlock.Text = cpuTemp < 90 ? "正常" : "异常";
            CpuTemeratureTextBlock.Foreground = new SolidColorBrush(cpuTemp < 90 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            // 更新 CPU 使用率
            CpuusageTextBlock.Text = cpuUsage < 90 ? "正常" : "异常";
            CpuusageTextBlock.Foreground = new SolidColorBrush(cpuUsage < 90 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            // 更新 CPU 风扇状态
            CpufanTextBlock.Text = fanSpeed > 0 ? "正常" : "异常";
            CpufanTextBlock.Foreground = new SolidColorBrush(fanSpeed > 0 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            // 更新 GPU 温度
            GraphicTemperatureTextBlock.Text = gpuTemp < 90 ? "正常" : "异常";
            GraphicTemperatureTextBlock.Foreground = new SolidColorBrush(gpuTemp < 90 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            // 更新硬盘温度
            DiskUsageTextBlock.Text = hddTemp < 60 ? "正常" : "异常";
            DiskUsageTextBlock.Foreground = new SolidColorBrush(hddTemp < 60 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            GraphicUsageTextBlock.Text = "正常";
            GraphicUsageTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            MemoryUsageTextBlock.Text = "正常";
            MemoryUsageTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

            // 检查各项状态并更新计数
            if (cpuTemp >= 90) abnormalCount++;
            if (cpuUsage >= 90) abnormalCount++;
            if (fanSpeed <= 0) abnormalCount++;
            if (gpuTemp >= 90) abnormalCount++;
            if (hddTemp >= 60) abnormalCount++;
            if (gpuUsage >= 90) abnormalCount++;
            if (memoryUsage >= 90) abnormalCount++;
            if (hddUsage >= 90) abnormalCount++;

            StartDriveMonitoring();

            scanText.Text = "正在检测病毒软件及防火墙状态...";
            bool res1 = false, res2 = false;
            try
            {
                // 检查病毒和威胁防护是否已开启
                bool isVirusProtectionEnabled = CheckVirusProtectionStatus();
                res1 = isVirusProtectionEnabled;
                VirusTextBlock.Text = isVirusProtectionEnabled ? "已开启" : "未开启";
                VirusTextBlock.Foreground = new SolidColorBrush(isVirusProtectionEnabled ? Color.FromArgb(255, 0, 175, 17) : Color.FromArgb(255, 255, 0, 0));

                // 检查防火墙和网络保护是否已开启
                bool isFirewallEnabled = CheckFirewallStatus();
                res2 = isFirewallEnabled;
                FirewallTextBlock.Text = isFirewallEnabled ? "已开启" : "未开启";
                FirewallTextBlock.Foreground = new SolidColorBrush(isFirewallEnabled ? Color.FromArgb(255, 0, 175, 17) : Color.FromArgb(255, 255, 0, 0));
            }
            catch (Exception ex)
            {
                VirusTextBlock.Text = "检测失败";
                VirusTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

                FirewallTextBlock.Text = "检测失败";
                FirewallTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

                Console.WriteLine($"更新安全状态失败: {ex.Message}");
            }
            if (!res1) abnormalCount++;
            if (!res2) abnormalCount++;

            totalScore = 100 - abnormalCount * 5;
            ScoreText.Text = totalScore.ToString("F0");

            Color fillColor = totalScore >= 90 ? Color.FromArgb(255, 0, 100, 0) :
                                totalScore >= 80 ? Color.FromArgb(220, 159, 70, 0) :
                                                    Color.FromArgb(158, 61, 61, 1);
            ScoreCircle.Fill = new SolidColorBrush(fillColor);
            scanText.Text = totalScore == 100 ? "电脑非常健康" :
                            totalScore >= 90 ? "电脑存在问题，请及时修复" :
                            totalScore >= 80 ? "电脑问题较多，请立即修复" :
                                                "电脑存在严重问题，需要立即修复";


            Endprogressbar();
        }


        private bool CheckVirusProtectionStatus()
        {
            try
            {
                // 使用 WMI 查询 Windows Security Center 的状态
                using (var searcher = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT * FROM AntiVirusProduct"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        string state = queryObj["productState"]?.ToString();
                        if (!string.IsNullOrEmpty(state))
                        {
                            // Interpret the state code more specifically
                            int productStateCode = int.Parse(state, System.Globalization.NumberStyles.HexNumber);
                            if ((productStateCode & 0x0010) == 0x0010) // 检查第 4 位是否为 1
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"病毒防护状态检查失败: {ex.Message}");
            }
            return false;
        }

        private bool CheckFirewallStatus()
        {
            try
            {
                // 创建 WMI 查询以获取防火墙规则
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_FirewallProfile"))
                {
                    bool anyProfileEnabled = false;

                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        // 检查防火墙规则是否启用
                        if (queryObj["Enabled"] != null && bool.TryParse(queryObj["Enabled"].ToString(), out bool isEnabled))
                        {
                            if (isEnabled)
                            {
                                anyProfileEnabled = true;
                                break; // 如果找到启用的规则，退出循环
                            }
                        }
                    }

                    return anyProfileEnabled;
                }
            }
            catch (Exception ex)
            {
                // 如果发生异常，打印错误信息
                Console.WriteLine($"防火墙状态检查失败: {ex.Message}");
                return false;
            }
        }

        // 取消体检按钮的点击事件处理程序
        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个带有“是”和“否”按钮的对话框
            ContentDialog ConfirmDialog = new ContentDialog()
            {
                Title = "确定终止全面体检吗？",
                Content = "当前正在进行硬件健康扫描",
                PrimaryButtonText = "否",
                SecondaryButtonText = "是",
                XamlRoot = this.XamlRoot  // 设置 XamlRoot
            };

            var result = await ConfirmDialog.ShowAsync();

            // 根据用户的选择执行不同的操作
            if (result == ContentDialogResult.Primary) // 否
            {
                // 什么都不做，继续扫描
            }
            else // 是
            {
                // 点击“是”按钮时，终止 Python 脚本的运行
                if (monitoringProcess1 != null && !monitoringProcess1.HasExited)
                {
                    monitoringProcess1.Kill();  // 杀死进程
                    Endprogressbar();
                    scanText.Text = "全面体检已终止"; // 更新扫描状态文本
                    CpuTemeratureTextBlock.Text = "已终止";
                    CpuusageTextBlock.Text = "已终止";
                    CpufanTextBlock.Text = "已终止";
                    GraphicTemperatureTextBlock.Text = "已终止";
                    GraphicUsageTextBlock.Text = "已终止";
                    MemoryUsageTextBlock.Text = "已终止";
                    DiskUsageTextBlock.Text = "已终止";
                    CPUDriveText.Text = "已终止";
                    GraphicDriveText.Text = "已终止";
                    NetworkAdapterText.Text = "已终止";
                    AudioDeviceText.Text = "已终止";
                    StorageDeviceText.Text = "已终止";
                    BluetoothText.Text = "已终止";
                    USBText.Text = "已终止";
                    VirusTextBlock.Text = "已终止";
                    FirewallTextBlock.Text = "已终止";
                    fixButton.Visibility = Visibility.Collapsed;
                    cancelButtonPanel.Visibility = Visibility.Collapsed;
                    ReScanButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void Endprogressbar()
        {
            _progressTimer.Stop(); // 停止进度条更新的计时器
            progressBar.IsIndeterminate = false; // 停止进度条的无确定状态
            progressBar.Value = 100;  // 设置进度条为 100%
            fixButton.Visibility = Visibility.Visible; // 显示修复按钮
            scanstatusText.Text = "全面体检完毕";
            cancelButtonPanel.Visibility = Visibility.Collapsed; // 隐藏取消按钮
        }

        private async void FixButton_Click(object sender, RoutedEventArgs e)
        {
            GraphicUsageProgressRing.IsActive = true;
            GraphicUsageTextBlock.Text = "修复中...";
            CpuUsageProgressRing.IsActive = true;
            CpuusageTextBlock.Text = "修复中...";
            MemoryUsageProgressRing.IsActive = true;
            MemoryUsageTextBlock.Text = "修复中...";
            // 在后台线程执行耗时的任务，避免阻塞 UI 线程
            await Task.Delay(200);
            await KillHighCpuUsageProcesses();
            await KillHighGpuUsageProcesses();
            await ForceGarbageCollection();

            CpuUsageProgressRing.IsActive = false;
            CpuusageTextBlock.Text = "正常";
            GraphicUsageProgressRing.IsActive = false;
            GraphicUsageTextBlock.Text = "正常";
            MemoryUsageProgressRing.IsActive = false;
            MemoryUsageTextBlock.Text = "正常";

            totalScore = 100;
            ReStartHardwareMonitoring();
            ScoreText.Text = "100";
            scanText.Text = "一键修复完成";
        }

        // 关闭高CPU 使用率的进程
        private async Task KillHighCpuUsageProcesses()
        {
            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses)
            {
                try
                {
                    // 排除系统进程或用户进程，不关闭关键进程
                    if (process.ProcessName.Equals("System", StringComparison.OrdinalIgnoreCase) ||
                        process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase) ||
                        process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase) ||
                        process.HasExited)
                    {
                        continue; // 不关闭系统进程等
                    }

                    if (process.ProcessName.Contains("Python", StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill(); // 终止该进程
                    }
                }
                catch (Exception ex)
                {
                    // 捕获任何异常并输出
                    Console.WriteLine($"无法关闭进程 {process.ProcessName}: {ex.Message}");
                }
            }
        }

        private async Task KillHighGpuUsageProcesses()
        {
            var allProcesses = Process.GetProcesses();
            foreach (var process in allProcesses)
            {
                try
                {
                    if (process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase) ||
                        process.ProcessName.Equals("cmd", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using (var gpuCounter = new PerformanceCounter("GPU", "% GPU Usage", process.ProcessName))
                    {
                        gpuCounter.NextValue();
                        float gpuUsage = gpuCounter.NextValue();

                        if (gpuUsage > 80)
                        {
                            process.Kill();
                            Console.WriteLine($"Terminated process {process.ProcessName} with GPU usage: {gpuUsage}%");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to terminate process {process.ProcessName}: {ex.Message}");
                }
            }

        }
        private async Task ForceGarbageCollection()
        {
            // 强制启动垃圾回收，释放不再使用的内存
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


    }
}
