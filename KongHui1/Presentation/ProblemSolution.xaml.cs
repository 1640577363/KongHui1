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
using Windows.Storage;
//using Windows.Web.Http;
//using System.Net.Http;
using Windows.Storage.Streams;
using Windows.Web.Http; // 使用 UWP 的 HttpClient 和 HttpRequestHeaders




using System.IO;
using Windows.Storage.Streams;
using Windows.Media.Protection.PlayReady;
using Windows.Web.Http.Headers;


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
        long id = 1;

        var fileUrl = "http://10.12.36.204:8080/file/file/download/" + 121; // 下载链接

        // 获取保存文件的路径（例如，用户的 Documents 文件夹）
        // 获取当前应用程序的 LocalFolder（当前目录）
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        // 获取文件名
        var fileName =id+".exe";

        // 在当前目录下创建文件
        var fileToSave = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        try
        {
            var httpClient = new Windows.Web.Http.HttpClient();
            //string Token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjEwLjE0LjQzLjMyQEZyaSBKYW4gMTAgMTA6NDU6MDkgQ1NUIDIwMjUifQ.e4YptARM4l-ClnR2zHrbC0HFlWP2QVN92itI0P2k2ZWkiDxMxmNRwk4wGda6QvnKs44waqpn-dfhsMuOHjNtcQ"; // 请用实际的 Token 替换
           
            // 创建一个 HttpRequestMessage 对象
            var requestMessage = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(fileUrl));

            // 设置 Authorization 头
            requestMessage.Headers.Authorization = new HttpCredentialsHeaderValue("Bearer", LoginPage.Token);

            // 发送请求
            var response = await httpClient.SendRequestAsync(requestMessage);

            // 检查响应是否成功
            if (response.IsSuccessStatusCode)
            {
                // 获取响应内容的流
                IInputStream contentStream = await response.Content.ReadAsInputStreamAsync();

                // 获取文件保存的流
                var fileStream = await fileToSave.OpenAsync(FileAccessMode.ReadWrite);
                using (var outputStream = fileStream.GetOutputStreamAt(0))
                {
                    // 将内容流写入文件
                     RandomAccessStream.CopyAndCloseAsync(contentStream, outputStream);
                }

                // 提示用户下载成功
                var dialog = new MessageDialog("文件下载成功！");
                dialog.ShowAsync();
            }
            else
            {
                // 提示用户下载失败
                var dialog = new MessageDialog("文件下载失败，请检查网络连接！");
                 dialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            var dialog = new MessageDialog("发生错误：" + ex.Message);
            await dialog.ShowAsync();
        }
    }


    //private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    //{
    //    var fileUrl = "https://example.com/path/to/your/file.zip";

    //    // 获取保存文件的路径（例如，用户的 Downloads 文件夹）
    //    var downloadsFolder = await StorageFolder.GetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    //    var fileName = Path.GetFileName(fileUrl); // 从 URL 中获取文件名
    //    var fileToSave = await downloadsFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);




    //    // 创建文件保存对话框
    //    try
    //    {
    //        // 创建 HTTP 请求对象
    //        var httpClient = new HttpClient();
    //        var response = await httpClient.GetAsync(new Uri(fileUrl));
    //        if (response.IsSuccessStatusCode)
    //        {

    //            // 获取响应内容的流
    //            IInputStream contentStream = response.Content.ReadAsStreamAsync();

    //            // 获取文件保存的流
    //            var fileStream = await fileToSave.OpenAsync(FileAccessMode.ReadWrite);
    //            using (var outputStream = fileStream.GetOutputStreamAt(0))
    //            {
    //                // 将内容流写入文件
    //                await RandomAccessStream.CopyAndCloseAsync(contentStream, outputStream);
    //            }

    //            // 提示用户下载成功
    //            var dialog = new MessageDialog("文件下载成功！");
    //            await dialog.ShowAsync();
    //        }
    //        else
    //        {
    //            // 提示用户下载失败
    //            var dialog = new MessageDialog("文件下载失败，请检查网络连接！");
    //            await dialog.ShowAsync();
    //        }



    //        //// 获取指定路径的文件
    //        //StorageFile file = await StorageFile.GetFileFromPathAsync(documentPath);

    //        //// 打开文件
    //        //await Windows.System.Launcher.LaunchFileAsync(file);

    //        // 提示用户文件已打开
    //        // var messageDialog = new MessageDialog("文件已成功打开。");
    //        // await messageDialog.ShowAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        // 处理异常
    //        var messageDialog = new MessageDialog("无法打开文件: " + ex.Message);
    //        await messageDialog.ShowAsync();
    //    }
    //}
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
