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
    }
    private async Task StartHardwareMonitoring()
    {
        scanText.Text = "正在进行CPU压力测试...";
        try
        {
            // Python 脚本路径
            string scriptPath1 = Path.Combine(scriptPath, "Hardware_stress_test.py");

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

        string ExtractPerformanceValue(string text)
        {
            string pattern = @"(\w+)\s+Performance:\s*(\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
            return match.Success ? match.Groups[2].Value : string.Empty;
        }

        string cpuTempPerformance = ExtractPerformanceValue(output);
        string cpuUsagePerformance = ExtractPerformanceValue(output);
        string memoryPerformance = ExtractPerformanceValue(output);
        string gpuTempPerformance = ExtractPerformanceValue(output);
        string gpuUsagePerformance = ExtractPerformanceValue(output);

        string GetPerformanceText(string performance)
        {
            switch (performance)
            {
                case "Excellent":
                    return "优秀";
                case "Fair":
                    return "合格";
                case "Poor":
                    return "欠佳";
                default:
                    return "待检测";
            }
        }

        CpuTempText.Text = GetPerformanceText(cpuTempPerformance);
        CpuUsageText.Text = GetPerformanceText(cpuUsagePerformance);
        MemoryUsageText.Text = GetPerformanceText(memoryPerformance);
        GpuTempText.Text = GetPerformanceText(gpuTempPerformance);
        GpuUsageText.Text = GetPerformanceText(gpuUsagePerformance);

        int GetScore(string performance)
        {
            switch (performance)
            {
                case "Excellent":
                    return 20;
                case "Fair":
                    return 10;
                case "Poor":
                    return 5;
                default:
                    return 0;
            }
        }

        totalScore = GetScore(cpuTempPerformance) +
                         GetScore(cpuUsagePerformance) +
                         GetScore(memoryPerformance) +
                         GetScore(gpuTempPerformance) +
                         GetScore(gpuUsagePerformance);
        ScoreText.Text = totalScore.ToString("F0");

        Color fillColor = totalScore >= 90 ? Color.FromArgb(255, 0, 100, 0) :
                            totalScore >= 80 ? Color.FromArgb(220, 159, 70, 0) :
                                                Color.FromArgb(158, 61, 61, 1);
        ScoreCircle.Fill = new SolidColorBrush(fillColor);
        scanText.Text = totalScore == 100 ? "电脑非常健康" :
                        totalScore >= 90 ? "电脑存在问题，请及时修复" :
                        totalScore >= 80 ? "电脑问题较多，请立即修复" :
                                            "电脑存在严重问题，需要立即修复";



        CpuTempRing.IsActive = false;
        CpuUsageRing.IsActive = false;
        MemoryUsageRing.IsActive = false;
        GpuTempRing.IsActive = false;
        GpuUsageRing.IsActive = false;

        Endprogressbar();

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
        MemoryUsageRing.IsActive = true;
        GpuTempRing.IsActive = true;
        GpuUsageRing.IsActive = true;
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
        scanstatusText.Text = "全面体检完毕";
        cancelButtonPanel.Visibility = Visibility.Collapsed; // 隐藏取消按钮
    }

    
}
