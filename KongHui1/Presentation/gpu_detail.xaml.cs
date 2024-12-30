using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware;
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
public sealed partial class gpu_detail : Page
{
    private DispatcherTimer _timer;
    private Process _GPUusagePythonProcess;
    private Process _GpudetailpythonProcess;

    public gpu_detail()
    {
        this.InitializeComponent();
        StartGpuMonitoring();
        // 额外调用以加载主板信息
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

    private async void StartGpuMonitoring()
    {
        await RunGPUPythonScript();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)  // 每秒更新
        };
        _timer.Tick += UpdateGPUChartImage;
        _timer.Start();
    }

    private async Task RunGPUPythonScript()
    {

        string currentDirectory = AppContext.BaseDirectory;

        // 构建 Python 脚本的相对路径
        string pythonScriptPath1 = Path.Combine(currentDirectory, "Python", "Graphics_usage.py");
        string pythonScriptPath2 = Path.Combine(currentDirectory, "Python", "Gpu_detail.py");

        var startInfo1 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath1, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _GPUusagePythonProcess = Process.Start(startInfo1);

        var startInfo2 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath2, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _GpudetailpythonProcess = Process.Start(startInfo2);

        await Task.Delay(1000); // 等待 Python 脚本初始化
    }

    private async void UpdateGPUChartImage(object sender, object e)
    {
        var imagePath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "gpu_usage_chart.png");

        if (File.Exists(imagePath))
        {
            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            bitmap.UriSource = new Uri(imagePath);
            GpuUsageChartImage.Source = bitmap;
        }
        await UpdateGpuInfoText();
    }

    private async Task UpdateGpuInfoText()
    {
        var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "gpuinfo.json");
        if (File.Exists(infoPath))
        {
            var jsonData = await File.ReadAllTextAsync(infoPath);
            var GpuInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);

            if (GpuInfo != null)
            {
                name.Text = $"速度: {GpuInfo["name"]}";
                utilization_ratio.Text = $"内存使用率: {GpuInfo["utilization_ratio"]}";
                memoryTotal.Text = $"容量: {GpuInfo["memoryTotal"]}";
                Used.Text = $"部件编号: {GpuInfo["Used"]}";
                Free.Text = $"制造商: {GpuInfo["Free"]}";
                driver_version.Text = $"内存类型: {GpuInfo["driver_version"]}";
                driver_date.Text = $"外形规格: {GpuInfo["driver_date"]}";
                location.Text = $"总内存: {GpuInfo["location"]}";
                DirectX_version.Text = $"总内存: {GpuInfo["DirectX_version"]}";

            }
        }
    }
    private void StopPythonScript()
    {
        if (_GPUusagePythonProcess != null && !_GPUusagePythonProcess.HasExited)
        {
            _GPUusagePythonProcess.Kill();
            _GPUusagePythonProcess.Dispose();
            _GPUusagePythonProcess = null;
        }
        if (_GpudetailpythonProcess != null && !_GpudetailpythonProcess.HasExited)
        {
            _GpudetailpythonProcess.Kill();
            _GpudetailpythonProcess.Dispose();
            _GpudetailpythonProcess = null;
        }
    }
    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        StopPythonScript();
        if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
        {
            Frame.GoBack();   // 返回到前一个页面
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
