using System;
using System.Diagnostics;
using System.Management;
using System.IO;
using System.Text.Json;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using static IronPython.Modules._ast;
using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware;

namespace KongHui1.Presentation
{

    public sealed partial class HomeCheckPage : Page
    {
        private DispatcherTimer _timer;
        private Process _CPUpythonProcess;
        private Process _CpudetailpythonProcess;
        private Process _GPUusagePythonProcess;
        private Process _GpudetailpythonProcess;
        private Process _diskSpacePythonProcess;
        private Process _diskdetailPythonProcess;
        private Process _memoryPythonProcess;
        private DateTime lastCheckTime;
        private DispatcherTimer _updateTimer;
        private Process _MemorydetailpythonProcess;
        string currentDirectory = AppContext.BaseDirectory;
        public bool isload = false;

        public HomeCheckPage()
        {
            this.InitializeComponent();

            // 现有的初始化代码
            LoadMotherboardInfo();
            StartDiskSpaceMonitoring();
            StartCpuMonitoring();
            StartGpuMonitoring();
            StartMemoryMonitoring();

            // 初始化距离上次体检的定时器
            InitializeLastCheckTimer();
        }

        private void InitializeLastCheckTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // 每分钟检查一次
            };
            _timer.Tick += (sender, e) => UpdateLastCheckTime();
            _timer.Start();

            // 初始化 lastCheckTime 的值
            lastCheckTime = DateTime.Now; // 初次加载时可设定为当前时间或从存储中加载
            UpdateLastCheckTime(); // 立即更新 UI
        }

        // 查询主板型号
        private void LoadMotherboardInfo()
        {
            string motherboardModel = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                motherboardModel = queryObj["Product"].ToString();
            }
            MotherboardInfoTextBlock.Text = $"{motherboardModel}";
        }
        private void UpdateLastCheckTime()
        {
            // 计算时间差
            var timeElapsed = DateTime.Now - lastCheckTime;

            // 获取小时数（取整）
            int elapsedHours = (int)timeElapsed.TotalHours;

            // 将小时数显示在 UI 上
            LastCheckTextBlock.Text = $"距离上次体检：{elapsedHours} 小时";
        }



        private void FullCheckButton_Click(object sender, RoutedEventArgs e)
        {
            StopPythonScript();
            // 记录上次体检时间
            lastCheckTime = DateTime.Now;

            // 更新显示为刚开始的体检
            LastCheckTextBlock.Text = "距离上次体检：0.00 小时";

            // 导航到体检页面
            this.Frame.Navigate(typeof(FullCheckPage));
        }

        private async void StartCpuMonitoring()
        {
            await RunCpuPythonScript();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)  // 每秒更新
            };
            _timer.Tick += UpdateChartImage;
            _timer.Start();
        }

        private async Task RunCpuPythonScript()
        {
            string ScriptPath1 = @"D:\project\UNO2\KongHui1\Python\cpu_usage_plot.py";
            string ScriptPath2 = @"D:\project\UNO2\KongHui1\Python\cpu_detail.py";

            // 启动 cpu_usage_plot.py 脚本
            var startInfo1 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath1, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
            };

            _CPUpythonProcess = Process.Start(startInfo1);


            var startInfo2 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath2, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python"
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

            string ScriptPath1 = @"..\..\KongHui1\Python\disk_space.py";
            string ScriptPath2 = @"..\..\KongHui1\Python\disk_detail.py";

            var startInfo1 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath1, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
            };

            _diskSpacePythonProcess = Process.Start(startInfo1);

            var startInfo2 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath2, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
            };

            _diskdetailPythonProcess = Process.Start(startInfo2);

            await Task.Delay(1000); // 等待 Python 脚本初始化
        }

        private async void UpdateDiskSpaceChartImage(object sender, object e)
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
            await UpdateDiskSpaceText();
        }

        //private async Task UpdateDiskSpaceText()
        //{
        //    var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "disk_info.json");
        //    if (File.Exists(infoPath))
        //    {
        //        var jsonData = await File.ReadAllTextAsync(infoPath);
        //        var diskInfoList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonData);

        //        if (diskInfoList != null && diskInfoList.Count > 0)
        //        {
        //            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        //            if (dispatcherQueue != null)
        //            {
        //                dispatcherQueue.TryEnqueue(() =>
        //                {
        //                    var diskInfo = diskInfoList[0];

        //                    MountPointText.Text = $"挂载点: {diskInfo["mountpoint"]}";
        //                    DeviceNameText.Text = $"设备名称: {diskInfo["model"]}";
        //                    IsSystemDisk.Text = $"系统主盘: {diskInfo["isSystemDisk"]}";
        //                    DiskTypeText.Text = $"类型: {diskInfo["disk_type"]}";
        //                    TotalSizeText.Text = $"总容量: {diskInfo["total_size"]}";
        //                    Used_size.Text = $"已用容量: {diskInfo["used_size"]}";
        //                    Free_size.Text = $"剩余容量: {diskInfo["free_size"]}";
        //                    DiskUsageText.Text = $"利用率: {diskInfo["usage_percent"]}";
        //                    ReadSpeedText.Text = $"读取速度: {diskInfo["read_speed"]}";
        //                    WriteSpeedText.Text = $"写入速度: {diskInfo["write_speed"]}";

        //                });
        //            }
        //        }
        //    }
        //}
        private async Task UpdateDiskSpaceText()
        {
            var infoPath = Path.Combine(@"D:\project\UNO2\KongHui1\Python", "disk_info.json");

            if (File.Exists(infoPath))
            {
                var jsonData = await File.ReadAllTextAsync(infoPath);
                var diskInfoList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonData);

                if (diskInfoList != null && diskInfoList.Count > 0)
                {
                    // 清空旧的内容
                    DiskInfoPanel.Children.Clear();

                    foreach (var diskInfo in diskInfoList)
                    {
                        var diskInfoPanel = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Margin = new Thickness(10)
                        };

                        // 创建并添加每个磁盘的文本信息
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"挂载点: {diskInfo["mountpoint"]}", FontSize = 18});
                        //diskInfoPanel.Children.Add(new Rectangle { Height = 2, Fill = new SolidColorBrush(Colors.Blue), Width = 100 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"设备名称: {diskInfo["model"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"系统主盘: {diskInfo["isSystemDisk"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"类型: {diskInfo["disk_type"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"总容量: {diskInfo["total_size"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"已用容量: {diskInfo["used_size"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"剩余容量: {diskInfo["free_size"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"利用率: {diskInfo["usage_percent"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"读取速度: {diskInfo["read_speed"]}", FontSize = 16 });
                        diskInfoPanel.Children.Add(new TextBlock { Text = $"写入速度: {diskInfo["write_speed"]}", FontSize = 16 });

                        // 将该控件添加到主面板
                        DiskInfoPanel.Children.Add(diskInfoPanel);
                    }
                }
            }
        }


        public class DiskInfo
        {
            public string MountPoint { get; set; }
            public string Model { get; set; }
            public string IsSystemDisk { get; set; }
            public string DiskType { get; set; }
            public string TotalSize { get; set; }
            public string UsedSize { get; set; }
            public string FreeSize { get; set; }
            public string UsagePercent { get; set; }
            public string ReadSpeed { get; set; }
            public string WriteSpeed { get; set; }
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

            string ScriptPath1 = @"D:\project\UNO2\KongHui1\Python\Graphics_usage.py";
            string ScriptPath2 = @"D:\project\UNO2\KongHui1\Python\Gpu_detail.py";

            var startInfo1 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath1, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
            };

            _GPUusagePythonProcess = Process.Start(startInfo1);

            var startInfo2 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath2, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
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
                    name.Text = $"显卡名称: {GpuInfo["name"]}";
                    utilization_ratio.Text = $"GPU 使用率: {GpuInfo["utilization_ratio"]}";
                    memoryTotal.Text = $"显存总量: {GpuInfo["memoryTotal"]}";
                    Used.Text = $"已用显存: {GpuInfo["Used"]}";
                    Free.Text = $"可用显存: {GpuInfo["Free"]}";
                    driver_version.Text = $"驱动程序版本: {GpuInfo["driver_version"]}";
                    driver_date.Text = $"驱动程序发布日期: {GpuInfo["driver_date"]}";
                    DirectX_version.Text = $"DirectX 版本: {GpuInfo["DirectX_version"]}";

                }
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

            string ScriptPath1 = @"D:\project\UNO2\KongHui1\Python\Memory_usage_plot.py";
            string ScriptPath2 = @"D:\project\UNO2\KongHui1\Python\Memory-detail.py";

            var startInfo1 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath1, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
            };

            _memoryPythonProcess = Process.Start(startInfo1);

            var startInfo2 = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = ScriptPath2, // 使用相对路径
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"D:\project\UNO2\KongHui1\Python" // 使用相对路径作为工作目录
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

        protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // 停止更新定时器
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer = null;
            }
            StopPythonScript();
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
            if (_diskSpacePythonProcess != null && !_diskSpacePythonProcess.HasExited)
            {
                _diskSpacePythonProcess.Kill();
                _diskSpacePythonProcess.Dispose();
                _diskSpacePythonProcess = null;
            }
            if (_diskdetailPythonProcess != null && !_diskdetailPythonProcess.HasExited)
            {
                _diskdetailPythonProcess.Kill();
                _diskdetailPythonProcess.Dispose();
                _diskdetailPythonProcess = null;
            }
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



        private async void CpuButton_Click(object sender, RoutedEventArgs e)
        {
            MemoryDetailPanel.Visibility = Visibility.Collapsed;
            GpuDetailPanel.Visibility = Visibility.Collapsed;
            DiskDetailPanel.Visibility = Visibility.Collapsed;
            this.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255));
            CpuDetailPanel.Visibility = Visibility.Visible;
        }
        private void MemoryButton_Click(object sender, RoutedEventArgs e)
        {
            CpuDetailPanel.Visibility = Visibility.Collapsed;
            GpuDetailPanel.Visibility = Visibility.Collapsed;
            DiskDetailPanel.Visibility = Visibility.Collapsed;
            this.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255));
            MemoryDetailPanel.Visibility = Visibility.Visible;
        }

        private void GpuButton_Click(object sender, RoutedEventArgs e)
        {
            MemoryDetailPanel.Visibility = Visibility.Collapsed;
            CpuDetailPanel.Visibility = Visibility.Collapsed;
            DiskDetailPanel.Visibility = Visibility.Collapsed;
            this.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255));
            GpuDetailPanel.Visibility = Visibility.Visible;
        }


        private void DiskButton_Click(object sender, RoutedEventArgs e)
        {
            MemoryDetailPanel.Visibility = Visibility.Collapsed;
            CpuDetailPanel.Visibility = Visibility.Collapsed;
            GpuDetailPanel.Visibility = Visibility.Collapsed;
            this.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255));
            DiskDetailPanel.Visibility = Visibility.Visible;
        }


    }
}
