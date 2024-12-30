using System;
using System.Collections.Generic;
using System.Diagnostics;
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
public sealed partial class HelpPage : Page
{
    public HelpPage()
    {
        this.InitializeComponent();
        LoadMotherboardInfo(); // 额外调用以加载主板信息

    }
    private void OnProblemResolutionTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(ProblemSolution));
    }

    private void OnFeedbackTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(FeedbackPage));
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
    private void OnRepairPlatformTapped(object sender, TappedRoutedEventArgs e)
    {
        // 在这里添加维修平台的逻辑
        // 例如，打开维修服务平台的链接
        Debug.WriteLine("维修平台按钮被点击");
    }

    private void OnCompleteQueryTapped(object sender, TappedRoutedEventArgs e)
    {
        // 在这里添加整机查询的逻辑
        // 例如，查询设备的详细信息
        Debug.WriteLine("整机查询按钮被点击");
    }

    private void OnWarrantyQueryTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(Quality_assurance));
    }

    private void OnAddressQueryTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(Address_inquiry));
    }
}


