using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Windows.Storage.Pickers;
using System.IO;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProblemSolution : Page
{
    private string documentPath = @"D:\1.txt";
    public ProblemSolution()
    {
        this.InitializeComponent();
    }
    private void ToggleArrow_Click(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = sender as ToggleButton;
        if (toggleButton != null)
        {
            // 根据 ToggleButton 的 Tag 来识别对应的文本块
            switch (toggleButton.Tag)
            {
                case "1":
                    ToggleVisibility(AdditionalTextBlock, toggleButton);
                    break;
                case "2":
                    ToggleVisibility(AdditionalTextBlock2, toggleButton);
                    break;
                case "3":
                    ToggleVisibility(AdditionalTextBlock3, toggleButton);
                    break;
                case "4":
                    ToggleVisibility(AdditionalTextBlock4, toggleButton);
                    break;
                case "5":
                    ToggleVisibility(AdditionalTextBlock5, toggleButton);
                    break;
                case "6":
                    ToggleVisibility(AdditionalTextBlock6, toggleButton);
                    break;
            }
        }
    }

    private void ToggleVisibility(Border additionalTextBlock, ToggleButton toggleButton)
    {
        if (additionalTextBlock.Visibility == Visibility.Collapsed)
        {
            additionalTextBlock.Visibility = Visibility.Visible;
            toggleButton.Content = "▲"; // 更改箭头方向为向上
        }
        else
        {
            additionalTextBlock.Visibility = Visibility.Collapsed;
            toggleButton.Content = "▼"; // 更改箭头方向为向下
        }
    }

    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
        {
            Frame.GoBack();   // 返回到前一个页面
        }
    }
    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        // 创建文件保存对话框
        try
        {
            // 获取指定路径的文件
            StorageFile file = await StorageFile.GetFileFromPathAsync(documentPath);

            // 打开文件
            await Windows.System.Launcher.LaunchFileAsync(file);

            // 提示用户文件已打开
           // var messageDialog = new MessageDialog("文件已成功打开。");
           // await messageDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            // 处理异常
            var messageDialog = new MessageDialog("无法打开文件: " + ex.Message);
            await messageDialog.ShowAsync();
        }
    }
    private async Task ShowMessageDialog(string message)
    {
        // 确保在 UI 线程上显示消息对话框
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        });
    }
}
