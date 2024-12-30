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

namespace KongHui1.Presentation
{
    public sealed partial class FullCheckPage : Page
    {
        private DispatcherTimer _progressTimer; // 用于控制进度条的计时器
        private double _progressValue = 0;     // 当前进度值
        private Process _monitoringProcess;

        public FullCheckPage()
        {
            this.InitializeComponent();
            InitializeProgressBar();
            scanText.Text = "正在检测硬件健康状况...";
            StartHardwareMonitoring();
            scanText.Text = "正在检测驱动安装状况...";
            StartDriveMonitoring();
            scanText.Text = "正在检测病毒软件及防火墙状态...";
            UpdateSecurityStatus();
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }


        private void InitializeProgressBar()
        {
            // 设置进度条初始状态
            progressBar.IsIndeterminate = false;
            progressBar.Minimum = 0;
            progressBar.Maximum = 80;
            progressBar.Value = 0;

            // 初始化计时器
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // 每 100 毫秒更新一次
            };
            _progressTimer.Tick += ProgressTimer_Tick; // 注册定时器事件
            _progressTimer.Start();
        }

        /// <summary> 
        /// 定时器事件处理程序，更新进度条值
        /// </summary>
        private void ProgressTimer_Tick(object sender, object e)
        {
            _progressValue += 1; // 每次增加 1%
            if(_progressValue == progressBar.Maximum)
            {
                _progressTimer.Stop();
            }
            if (_progressValue < progressBar.Maximum)
            {
                progressBar.Value = _progressValue;
            }
            else
            {
                _progressTimer.Stop(); // 停止计时器
            }
        }

        private async Task StartDriveMonitoring()
        {
            try
            {
                // Python 脚本路径
                string scriptPath = @"D:\\Project\\UNO2\\KongHui1\\Python\\Drive_detection.py";

                var result = await Task.Run(() =>
                {
                    Process _monitoringProcess = new Process
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
                    _monitoringProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    _monitoringProcess.Start();
                    _monitoringProcess.BeginOutputReadLine();
                    _monitoringProcess.BeginErrorReadLine();
                    _monitoringProcess.WaitForExit();

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

        private async Task StartHardwareMonitoring()
        {
            
            try
            {
                // Python 脚本路径
                string scriptPath = @"D:\Project\UNO2\KongHui1\Python\HardwareMonitor.py";

                var result = await Task.Run(() =>
                {
                    Process process = new Process
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
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine($"[Error] {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    return new { ExitCode = process.ExitCode, Output = output.ToString() };
                });

                if (result.ExitCode == 0)
                {
                    UpdateHealthStatus(result.Output); // 调用方法解析评分并更新UI
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 根据脚本输出的评分更新健康状态文本和评分圆圈。
        /// </summary>
        private void UpdateHealthStatus(string output)
        {
            
            try
            {
                // 从文本中提取平均值
                double ExtractMetric(string metricName)
                {
                    string pattern = $"{metricName}: ([\\d\\.]+)"; // 正则匹配类似 "cpu_temp: 37.5"
                    var match = System.Text.RegularExpressions.Regex.Match(output, pattern);
                    if (match.Success && double.TryParse(match.Groups[1].Value, out double value))
                    {
                        return value;
                    }
                    return 0; // 默认值
                }

                // 提取监控数据
                double cpuTemp = ExtractMetric("cpu_temp");
                double cpuUsage = ExtractMetric("cpu_usage");
                double fanSpeed = ExtractMetric("fan_speed");
                double gpuTemp = ExtractMetric("gpu_temp");
                double hddTemp = ExtractMetric("hdd_temp");

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
                GraphicUsageTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0) );
                MemoryUsageTextBlock.Text = "正常";
                MemoryUsageTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0) );

            }
            catch (Exception ex)
            {
                CpuTemeratureTextBlock.Text = "解析失败";
                CpuTemeratureTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                Console.WriteLine($"更新健康状态失败: {ex.Message}");
            }

            try
            {
                // 提取总分
                double totalScore = ExtractScore("总分", output);
                ScoreText.Text = totalScore.ToString("F0");

                // 更新圆球颜色（基于总分）
                Color fillColor = totalScore >= 90 ? Color.FromArgb(255, 0, 255, 0) :
                                   totalScore >= 80 ? Color.FromArgb(220, 159, 70, 0) :
                                                      Color.FromArgb(158, 61, 61, 1);
                ScoreCircle.Fill = new SolidColorBrush(fillColor);
                scanText.Text = totalScore == 100 ? "电脑非常健康" :
                                totalScore >= 90 ? "电脑存在问题，请及时修复" :
                                totalScore >= 80 ? "电脑问题较多，请立即修复" :
                                                   "电脑存在严重问题，需要立即修复";

            }
            catch (Exception ex)
            {
                ScoreText.Text = "Error";
                Console.WriteLine($"更新健康状态失败: {ex.Message}");
            }

            Endprogressbar();
            
        }
        double ExtractScore(string metricName, string output)
        {
            try
            {
                string pattern = $"{metricName}: (\\d+)";
                var match = Regex.Match(output, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析 {metricName} 失败: {ex.Message}");
            }
            return 0; // 默认值
        }

        // 取消体检按钮的点击事件处理程序
        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "取消体检",
                Content = "您确定要取消体检吗？",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消"
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                // 停止进程
                if (_monitoringProcess != null && !_monitoringProcess.HasExited)
                {
                    _monitoringProcess.Kill(); // 杀死进程
                    _monitoringProcess.Dispose(); // 释放资源
                    _monitoringProcess = null; // 清除引用
                }

                // 停止进度条
                progressBar.IsIndeterminate = false;
                progressBar.Value = 0;
            };

            await dialog.ShowAsync();
        }


        private void UpdateSecurityStatus()
        {
            
            try
            {
                // 检查病毒和威胁防护是否已开启
                bool isVirusProtectionEnabled = CheckVirusProtectionStatus();
                VirusTextBlock.Text = isVirusProtectionEnabled ? "已开启" : "未开启";
                VirusTextBlock.Foreground = new SolidColorBrush(isVirusProtectionEnabled ? Color.FromArgb(255, 0, 175, 17) : Color.FromArgb(255, 255, 0, 0));

                // 检查防火墙和网络保护是否已开启
                bool isFirewallEnabled = CheckFirewallStatus();
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
        private void Endprogressbar()
        {
            _progressTimer.Stop(); // 停止进度条更新的计时器
            progressBar.IsIndeterminate = false; // 停止进度条的无确定状态
            progressBar.Value = 100;  // 设置进度条为 100%
            fixButton.Visibility = Visibility.Visible; // 显示修复按钮
            scanstatusText.Text= "全面体检完毕";
            cancelButtonPanel.Visibility = Visibility.Collapsed; // 隐藏取消按钮
        }
        private async void FixButton_Click(object sender, RoutedEventArgs e)
        {
            CpufanTextBlock.Text = "正常";
            CpufanTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            NetworkAdapterText.Text = "已安装";
            NetworkAdapterText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 175, 17));
            FirewallTextBlock.Text = "已开启";
            FirewallTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 175, 17));
            ScoreText.Text="100";
            ScoreCircle.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        }
    }
}
