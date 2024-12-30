using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Microsoft.UI.Xaml.Controls;

namespace KongHui1.Presentation
{
    /// <summary>
    /// 显示驱动信息的页面
    /// </summary>
    public sealed partial class DrivePage : Page
    {
        private static bool isLoad = false; // 静态变量，用于标记是否已加载驱动信息
        private static List<DriverInfo> driverInfoList = new(); // 静态变量，存储驱动信息

        public DrivePage()
        {
            this.InitializeComponent();

            // 首次加载时检测驱动信息
            if (!isLoad)
            {
                CollectDriverInfo();
                isLoad = true;
            }

            // 显示驱动信息
            DisplayDriverInfo();
        }

        // 驱动信息类
        public class DriverInfo
        {
            public string DeviceName { get; set; }
            public string DeviceClass { get; set; }
            public string Manufacturer { get; set; }
            public string DriverVersion { get; set; } // 驱动版本号属性
            public string LatestDriverVersion { get; set; } = "未知"; // 最新版本属性

            public string DeviceClassChinese => GetDeviceClassChinese(DeviceClass);

            private string GetDeviceClassChinese(string deviceClass)
            {
                return deviceClass switch
                {
                    "DISPLAY" => "显卡",
                    "SOUND" => "声卡",
                    "SYSTEM" => "主板芯片组",
                    "NET" => "网卡",
                    "USB" => "USB 控制器",
                    "DISKDRIVE" => "硬盘和存储控制器",
                    "BLUETOOTH" => "蓝牙",
                    "PRINTER" => "打印机",
                    "IMAGE" => "摄像头",
                    "INPUT" => "触摸板和输入设备",
                    _ => "未知设备"
                };
            }
        }

        // 定义允许显示的驱动类型
        private readonly HashSet<string> allowedDeviceClasses = new()
        {
            "DISPLAY", // 显卡驱动
            "SOUND", // 声卡驱动
            "SYSTEM", // 主板芯片组驱动
            "NET", // 网卡驱动
            "USB", // USB 控制器驱动
            "DISKDRIVE", // 硬盘和存储控制器驱动
            "BLUETOOTH", // 蓝牙驱动
            "PRINTER", // 打印机驱动
            "IMAGE", // 摄像头驱动
            "INPUT" // 触摸板和输入设备驱动
        };

        // 收集驱动信息
        private void CollectDriverInfo()
        {
            driverInfoList.Clear(); // 清除上次检测的残留数据

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject driver in searcher.Get())
            {
                string deviceName = driver["DeviceName"]?.ToString();
                string driverVersion = driver["DriverVersion"]?.ToString();
                string manufacturer = driver["Manufacturer"]?.ToString();
                string deviceClass = driver["DeviceClass"]?.ToString();

                // 检查驱动类别是否允许
                if (!string.IsNullOrEmpty(deviceClass) && allowedDeviceClasses.Contains(deviceClass))
                {
                    // 添加到列表
                    driverInfoList.Add(new DriverInfo
                    {
                        DeviceName = deviceName,
                        DriverVersion = driverVersion, // 绑定版本号
                        Manufacturer = manufacturer,
                        DeviceClass = deviceClass
                    });
                }
            }
        }

        // 手动检测驱动的按钮事件
        private void Detection_drive(object sender, RoutedEventArgs e)
        {
            CollectDriverInfo(); // 收集驱动信息（清除旧数据后）
            DisplayDriverInfo(); // 显示驱动信息
        }

        // 显示驱动信息
        private void DisplayDriverInfo()
        {
            // 按设备类别进行分组，并动态生成名称和数量
            var groupedData = driverInfoList
                .GroupBy(d => d.DeviceClassChinese) // 按中文设备类别分组
                .Select(g => new
                {
                    Key = $"{g.Key}（{g.Count()}）", // 动态拼接驱动类别和数量
                    Items = g.ToList()
                })
                .ToList();

            // 绑定分组数据到 ItemsControl
            DriversListBox.ItemsSource = groupedData;

            // 更新驱动数量显示
            DriversCountTextBlock.Text = $"本地共发现 {driverInfoList.Count} 个驱动";
        }
    }
}
