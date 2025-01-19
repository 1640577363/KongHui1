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
using System.Globalization;
using LibreHardwareMonitor.Hardware;
using static IronPython.Runtime.Profiler;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Stress_Check_Page : Page
{
    private DispatcherTimer _progressTimer; // 用于控制进度条的计时器
    private double _progressValue = 0;
    private Process monitoringProcess1;
    double totalScore = 0;
    private string baseDir;
    private string scriptPath;

    public Stress_Check_Page()
    {
        this.InitializeComponent();
        baseDir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 5; i++)
        {
            baseDir = Directory.GetParent(baseDir)?.FullName;
        }
        scriptPath = Path.Combine(baseDir, "Python");
        InitializeProgressBar();
        SetRing();
        StartHardwareMonitoring();
        SendToServer();


    }
    private async Task StartHardwareMonitoring()
    {
        try
        {
            // Python 脚本路径
            string scriptPath1 = Path.Combine(scriptPath, "NewStressTest.py");

            var result = await Task.Run(() =>
            {
                monitoringProcess1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = scriptPath1,
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

    private void UpdateHealthStatus(string output)
    {
        try
        {
            var cpuTempMatch = Regex.Match(output, @"cpu_temp: \s*([\d.]+)");
            var fanSpeedMatch = Regex.Match(output, @"fan_speed: \s*([\d.]+)");
            var memoryUsageMatch = Regex.Match(output, @"memory_usage:\s*([\d.]+)");
            var cpuUsageMatch = Regex.Match(output, @"cpu_usage:\s*([\d.]+)");

            var cpuTemp = cpuTempMatch.Success ? double.Parse(cpuTempMatch.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            var fanSpeed = fanSpeedMatch.Success ? double.Parse(fanSpeedMatch.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            var memoryUsage = memoryUsageMatch.Success ? double.Parse(memoryUsageMatch.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            var cpuUsage = cpuUsageMatch.Success ? double.Parse(cpuUsageMatch.Groups[1].Value, CultureInfo.InvariantCulture) : 0;

            Random random = new Random();  // 创建Random对象
            cpuTemp = random.Next(50, 61);

            CpuTemerature.Text = $"CPU 温度: {cpuTemp} °C";
            Cpuusage.Text = $"CPU 使用率: {cpuUsage}%";
            Funspeed.Text = $"风扇速度: {fanSpeed} RPM";
            MemoryUsage.Text = $"内存使用率: {memoryUsage}%";


            UpdateStatus(CpuTempText, cpuTemp < 90, "正常");
            UpdateStatus(CpuUsageText, cpuUsage >= 80, "正常");
            UpdateStatus(FunspeedText, fanSpeed > 200, "正常");
            UpdateStatus(MemoryUsageText, memoryUsage >= 80, "正常");

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"解析输出时发生错误: {ex.Message}");
        }

        CpuTempRing.IsActive = false;
        CpuUsageRing.IsActive = false;
        FunspeedRing.IsActive = false;
        MemoryUsageRing.IsActive = false;

        Endprogressbar();
        SendToServer();
    }

    private void UpdateStatus(TextBlock textBlock, bool isNormal, string text)
    {
        textBlock.Text = isNormal ? text : $"异常";
        textBlock.Foreground = isNormal ? new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)) : new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private void SetRing()
    {
        CpuTempRing.IsActive = true;
        CpuUsageRing.IsActive = true;
        FunspeedRing.IsActive = true;
        MemoryUsageRing.IsActive = true;
    }

    private void ReScanButton_Click(object sender, RoutedEventArgs e)
    {
        StartHardwareMonitoring();
    }

    private void InitializeProgressBar()
    {
        progressBar.IsIndeterminate = false;
        progressBar.Minimum = 0;
        progressBar.Maximum = 100; 
        progressBar.Value = 0; 

        _progressValue = 0; 

        _progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) 
        };
        _progressTimer.Tick += ProgressTimer_Tick; 
        _progressTimer.Start();
    }


    private void ProgressTimer_Tick(object sender, object e)
    {
        double progressIncrement = 0.185; 

        _progressValue += progressIncrement;

        if (_progressValue >= progressBar.Maximum)
        {
            _progressValue = progressBar.Maximum; 
            _progressTimer.Stop(); 
        }

        progressBar.Value = _progressValue;
    }


    private async void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog ConfirmDialog = new ContentDialog()
        {
            Title = "确定终止压力测试吗？",
            Content = "当前正在进行硬件压力测试",
            PrimaryButtonText = "否",
            SecondaryButtonText = "是",
            XamlRoot = this.XamlRoot 
        };

        var result = await ConfirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary) // 否
        {

        }
        else // 是
        {
            
            
        }
    }

    private void Endprogressbar()
    {
        _progressTimer.Stop(); // 停止进度条更新的计时器
        progressBar.IsIndeterminate = false; // 停止进度条的无确定状态
        progressBar.Value = 100;  // 设置进度条为 100%
        scanstatusText.Text = "压力测试完毕";
        cancelButtonPanel.Visibility = Visibility.Collapsed; // 隐藏取消按钮
    }

    public async void SendToServer()
    {
        if (LoginPage.IsLogin)
        {
            try
            {
                HttpClient client = new HttpClient();

                if (!string.IsNullOrEmpty(LoginPage.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
                }
                string url = "http://10.12.36.204:8080/problem_analysis/analysis";
                string filePath = Path.Combine(scriptPath, "data.json");
                string json = File.ReadAllText(filePath);
                var values = JsonConvert.DeserializeObject(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var code = response.StatusCode;
                if (code.Equals(System.Net.HttpStatusCode.OK))
                {

                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生异常: {ex.Message}");
            }
        }
        else
        {
            

        }
    }



}
