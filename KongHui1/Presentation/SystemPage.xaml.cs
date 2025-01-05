using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SystemPage : Page
{
    public SystemPage()
    {
        this.InitializeComponent();
        LoadMotherboardInfo();
    }
    // 系统镜像下载功能
    private void OnSystemDownloadTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(Mirroring_download));
    }
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
    // 系统备份还原功能
    private void OnSystemBackupTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(system_backup));
    }

    // BIOS 升级功能
    private void OnBiosUpgradeTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(BIOS_upgrade));
    }
    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray); // 鼠标悬停时背景变浅灰
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(Microsoft.UI.Colors.White); // 恢复默认背景
        }
    }
}
