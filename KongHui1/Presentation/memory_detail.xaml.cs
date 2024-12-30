using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class memory_detail : Page
{
    private DispatcherTimer _timer;
    private Process _memoryPythonProcess;
    private Process _MemorydetailpythonProcess;

    public memory_detail()
    {
        this.InitializeComponent();
        StartMemoryMonitoring();
    }

    
    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        StopPythonScript();
        if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
        {
            Frame.GoBack();   // 返回到前一个页面
        }
    }
    private async void StartMemoryMonitoring()
    {
        await RunMemoryPythonScript();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)  // 每秒更新
        };
        _timer.Tick += UpdateMemoryChartImage; // Use separate handler for memory
        _timer.Start();
    }

    private async Task RunMemoryPythonScript()
    {

        string currentDirectory = AppContext.BaseDirectory;

        // 构建 Python 脚本的相对路径
        string pythonScriptPath1 = Path.Combine(currentDirectory, "Python", "memory_usage_plot.py");
        string pythonScriptPath2 = Path.Combine(currentDirectory, "Python", "Memory_detail.py");

        var startInfo1 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath1, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _memoryPythonProcess = Process.Start(startInfo1);

        var startInfo2 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath2, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _MemorydetailpythonProcess = Process.Start(startInfo2);
        await Task.Delay(1000); // 等待 Python 脚本初始化
    }

    private async void UpdateMemoryChartImage(object sender, object e)
    {
        var imagePath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "memory_usage_chart.png");

        if (File.Exists(imagePath))
        {
            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            bitmap.UriSource = new Uri(imagePath);
            MemoryUsageChartImage.Source = bitmap;
        }
        await UpdateMemoryInfoText();
    }
    private async Task UpdateMemoryInfoText()
    {
        var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "Memory_info.json");
        if (File.Exists(infoPath))
        {
            var jsonData = await File.ReadAllTextAsync(infoPath);
            var MemoryInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);

            if (MemoryInfo != null)
            {
                speed.Text = $"速度: {MemoryInfo["speed"]}";
                memory_percent.Text = $"内存使用率: {MemoryInfo["memory_percent"]}";
                capacity.Text = $"容量: {MemoryInfo["capacity"]}";
                part_number.Text = $"部件编号: {MemoryInfo["part_number"]}";
                manufacturer.Text = $"制造商: {MemoryInfo["manufacturer"]}";
                memory_type.Text = $"内存类型: {MemoryInfo["memory_type"]}";
                form_factor.Text = $"外形规格: {MemoryInfo["form_factor"]}";
                memory_total.Text = $"总内存: {MemoryInfo["memory_total"]}";

            }
        }
    }


    // 处理页面卸载事件
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
        if (_memoryPythonProcess != null && !_memoryPythonProcess.HasExited)
        {
            _memoryPythonProcess.Kill();
            _memoryPythonProcess.Dispose();
            _memoryPythonProcess = null;
        }
        if (_MemorydetailpythonProcess != null && !_MemorydetailpythonProcess.HasExited)
        {
            _MemorydetailpythonProcess.Kill();
            _MemorydetailpythonProcess.Dispose();
            _MemorydetailpythonProcess = null;
        }
    }
    // 按钮点击事件处理器
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

    private void FullCheckButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(FullCheckPage));  // 跳转到全面扫描页
    }
}
