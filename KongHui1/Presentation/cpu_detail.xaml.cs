using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Linq;
using System.Text.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class cpu_detail : Page
{
    private DispatcherTimer _timer;
    private Process _CPUpythonProcess;
    private Process _CpudetailpythonProcess;
    public cpu_detail()
    {
        this.InitializeComponent();
        StartCpuMonitoring();
    }


    private async void StartCpuMonitoring()
    {
        await RunPythonScript();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)  // 每秒更新
        };
        _timer.Tick += UpdateChartImage;
        _timer.Start();
    }

    private async Task RunPythonScript()
    {
        // 获取当前执行文件的目录
        string currentDirectory = AppContext.BaseDirectory;

        // 构建 Python 脚本的相对路径
        string pythonScriptPath1 = Path.Combine(currentDirectory, "Python", "cpu_usage_plot.py");
        string pythonScriptPath2 = Path.Combine(currentDirectory, "Python", "cpu_detail.py");

        // 启动 cpu_usage_plot.py 脚本
        var startInfo1 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath1, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _CPUpythonProcess = Process.Start(startInfo1);

        // 启动 cpu_detail.py 脚本
        var startInfo2 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath2, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _CpudetailpythonProcess = Process.Start(startInfo2);

        await Task.Delay(1000); // 等待 Python 脚本初始化
    }


    private async void UpdateChartImage(object sender, object e)
    {
        var imagePath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "cpu_usage_chart.png");

        if (File.Exists(imagePath))
        {
            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            bitmap.UriSource = new Uri(imagePath);
            CpuUsageChartImage.Source = bitmap;
        }
        await UpdateCpuInfoText();
    }
    private async Task UpdateCpuInfoText()
    {
        var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "cpu_usage_info.json");
        if (File.Exists(infoPath))
        {
            var jsonData = await File.ReadAllTextAsync(infoPath);
            var cpuInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);

            if (cpuInfo != null)
            {
                CpuUsageText.Text = $"利用率: {cpuInfo["cpu_percent"]}";
                CpuSpeedText.Text = $"速度: {cpuInfo["cpu_speed"]}";
                TotalProcessesText.Text = $"进程: {cpuInfo["total_processes"]}";
                TotalThreadsText.Text = $"线程: {cpuInfo["total_threads"]}";
                HandleCountText.Text = $"句柄: {cpuInfo["total_handles"]}";
                UptimeText.Text = $"正常运行时间: {cpuInfo["uptime"]}";
                L1CacheText.Text = $"L1 缓存: {cpuInfo["cpu_l1_cache"]}";
                L2CacheText.Text = $"L2 缓存: {cpuInfo["cpu_l2_cache"]}";
                L3CacheText.Text = $"L3 缓存: {cpuInfo["cpu_l3_cache"]}";
            }
        }
    }

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        StopPythonScript();  // 程序退出时终止 Python 脚本
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
        base.OnNavigatedFrom(e);
    }
    private void StopPythonScript()
    {
        if (_CPUpythonProcess != null && !_CpudetailpythonProcess.HasExited)
        {
            _CPUpythonProcess.Kill();
            _CPUpythonProcess.Dispose();
            _CPUpythonProcess = null;
        }
        if (_CpudetailpythonProcess != null && !_CpudetailpythonProcess.HasExited)
        {
            _CpudetailpythonProcess.Kill();
            _CpudetailpythonProcess.Dispose();
            _CpudetailpythonProcess = null;
        }
    }
    private void FullCheckButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(FullCheckPage));
    }
    private void CpuButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(cpu_detail));  // 跳转到 CPU 详情页
    }

    private void MemoryButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(memory_detail));  // 跳转到内存详情页
    }

    private void GpuButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(gpu_detail));  // 跳转到显卡详情页
    }

    private void DiskButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(disk_detail));  // 跳转到硬盘详情页
    }

}

