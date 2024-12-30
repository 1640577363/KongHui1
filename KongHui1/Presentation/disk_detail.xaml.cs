using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;  // 添加命名空间以使用 WMI
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
public sealed partial class disk_detail : Page
{
    private Process _diskSpacePythonProcess;
    private Process _diskdetailPythonProcess;
    private DispatcherTimer _timer;

    public disk_detail()
    {
        this.InitializeComponent();
        StartDiskSpaceMonitoring();
    }
    

    private async void StartDiskSpaceMonitoring()
    {
        await RunDiskSpacePythonScript();
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)  // 每秒更新
        };
        _timer.Tick += UpdateDiskSpaceChartImage; // 新增磁盘空间图表更新
        _timer.Start();
    }

    private async Task RunDiskSpacePythonScript()
    {

        string currentDirectory = AppContext.BaseDirectory;

        // 构建 Python 脚本的相对路径
        string pythonScriptPath1 = Path.Combine(currentDirectory, "Python", "disk_space.py");
        string pythonScriptPath2 = Path.Combine(currentDirectory, "Python", "disk_detail.py");

        var startInfo1 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath1, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _diskSpacePythonProcess = Process.Start(startInfo1);

        var startInfo2 = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = pythonScriptPath2, // 使用相对路径
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(currentDirectory, "Python") // 使用相对路径作为工作目录
        };

        _diskdetailPythonProcess = Process.Start(startInfo2);

        await Task.Delay(1000); // 等待 Python 脚本初始化
    }

    private void UpdateDiskSpaceChartImage(object sender, object e)
    {
        var imagePath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "disk_usage_chart.png");

        if (File.Exists(imagePath))
        {
            var bitmap = new BitmapImage
            {
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            bitmap.UriSource = new Uri(imagePath);
            DiskSpaceChartImage.Source = bitmap; // 更新磁盘空间图表
        }
        UpdateDiskSpaceText();
    }
    private async Task UpdateDiskSpaceText()
    {
        var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "disk_usage_info.json");
        if (File.Exists(infoPath))
        {
            var jsonData = await File.ReadAllTextAsync(infoPath);
            var diskInfoList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonData);

            if (diskInfoList != null && diskInfoList.Count > 0)
            {
                // 获取第一个磁盘信息并更新UI
                var diskInfo = diskInfoList[0];

                DiskUsageText.Text = $"利用率: {diskInfo["read_speed"]}";
                DeviceNameText.Text = $"设备名称: {diskInfo["partition"]}";
                MountPointText.Text = $"挂载点: {diskInfo["mountpoint"]}";
                TotalSizeText.Text = $"总容量: {diskInfo["total_size"]}";
                ReadSpeedText.Text = $"读取速度: {diskInfo["read_speed"]}";
                WriteSpeedText.Text = $"写入速度: {diskInfo["write_speed"]}";
                ModelText.Text = $"型号: {diskInfo["model"]}";
                DiskTypeText.Text = $"类型: {diskInfo["disk_type"]}";


            }
        }
    }
    private void StopPythonScript()
    {
        if (_diskSpacePythonProcess != null && !_diskSpacePythonProcess.HasExited)
        {
            _diskSpacePythonProcess.Kill();
            _diskSpacePythonProcess.Dispose();
            _diskSpacePythonProcess = null;
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
}
