using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mono.Unix.Native;
using Newtonsoft.Json;
using System.Text;
using System.Management;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeedbackPage : Page
    {
        public FeedbackPage()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, PointerRoutedEventArgs e)
        {
            if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
            {
                Frame.GoBack();   // 返回到前一个页面
            }
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取页面上的控件值
            string problemType = GetSelectedProblemTypeIndex();
            string problemDescription = PDTextBox.Text;
            string companyName = CompanyNameTextBox.Text;
            string contactPhone = ContactPhoneTextBox.Text;

            HttpClient client = new HttpClient();

            if (!string.IsNullOrEmpty(LoginPage.Token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
            }
            else
            {
                ShowMessage("请先登录");
            }
            // 准备请求的 URL 和数据+
            string url = "http://10.12.36.204:8080/Issues_support/Issues_support"; // 后端 API 地址
            string hardwareId = GetHardDiskID();
            var values = new Dictionary<string, string>
            {
                { "hardwareId", hardwareId },
                { "problemType", problemType },
                { "problemDescription", problemDescription },
                { "company", companyName },
                { "contactPhone", contactPhone }
            };
            string json = JsonConvert.SerializeObject(values);

            // 将数据转换为 JSON 格式
            var content = new StringContent(json, Encoding.UTF8, "application/json");



            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();
            var code = response.StatusCode;
            if (code.Equals(System.Net.HttpStatusCode.OK))
            {
                // 登录成功，导航到 MainPage.xaml
                this.Frame.Navigate(typeof(FeedbackPage));
            }
            else
            {
                // 登录失败，显示错误信息
                ShowMessage("账号或密码不正确");
            }
        }
        private async void ShowMessage(string message)
        {
            // 显示消息提示
            var dialog = new ContentDialog
            {
                Title = "登录提示",
                Content = message,
                CloseButtonText = "确定"
            };

            await dialog.ShowAsync();
        }

        private string GetSelectedProblemTypeIndex()
        {
            foreach (var radioButton in ProblemTypeStackPanel.Children)
            {
                var radio = radioButton as RadioButton;
                if (radio != null && radio.IsChecked == true)
                {
                    return radio.Tag.ToString();
                }
            }
            return "-1"; // 如果没有选中的RadioButton，返回-1或其他错误代码
        }
        static string GetHardDiskID()
        {
            foreach (ManagementObject disk in new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia").Get())
            {
                // 获取硬盘的序列号（Serial Number），这是硬盘的唯一标识符
                var serialNumber = disk["SerialNumber"]?.ToString().Trim();
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return $"磁盘 ID: {{{serialNumber}}}";
                }
            }
            return "未找到硬盘ID";
        }

    }
}
