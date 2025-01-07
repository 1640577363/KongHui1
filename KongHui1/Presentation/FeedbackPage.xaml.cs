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
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI;

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
            StartGetFeedback();

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
            Debug.WriteLine("Button Clicked!");

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
            // 准备请求的 URL 和数据+
            string url = "http://10.14.52.222:8080/Issues_support/Issues_support"; // 后端 API 地址
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
        private async Task StartGetFeedback()
        {

            try
            {
                // Python 脚本路径
                string scriptPath = @"..\..\KongHui1\Python\Tech_Support\main\get_feedback.py";

                var result = await Task.Run(() =>
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "python",
                            Arguments = scriptPath,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    StringBuilder output = new StringBuilder();
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            output.AppendLine($"[Error] {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    return new { ExitCode = process.ExitCode, Output = output.ToString() };
                });

                if (result.ExitCode == 0)
                {
                    UpdateHealthStatus(result.Output); // 更新显示内容
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 根据脚本输出的评分更新健康状态文本和评分圆圈。
        /// </summary>
        private void UpdateHealthStatus(string output)
        {

            try
            {
                // 从文本中提取平均值
                double ExtractMetric(string metricName)
                {
                    string pattern = $"{metricName}: ([\\d\\.]+)"; // 正则匹配类似 "cpu_temp: 37.5"
                    var match = System.Text.RegularExpressions.Regex.Match(output, pattern);
                    if (match.Success && double.TryParse(match.Groups[1].Value, out double value))
                    {
                        return value;
                    }
                    return 0; // 默认值
                }

                // 提取监控数据
                double cpuTemp = ExtractMetric("cpu_temp");
                double cpuUsage = ExtractMetric("cpu_usage");
                double fanSpeed = ExtractMetric("fan_speed");
                double gpuTemp = ExtractMetric("gpu_temp");
                double hddTemp = ExtractMetric("hdd_temp");

                // 更新问题描述
                problemDescription.Text = cpuTemp < 90 ? "正常" : "异常";
                problemDescription.Foreground = new SolidColorBrush(cpuTemp < 90 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

                // 更新解决方案
                resolutionMethod.Text = cpuUsage < 90 ? "正常" : "异常";
                resolutionMethod.Foreground = new SolidColorBrush(cpuUsage < 90 ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));

            }
            catch (Exception ex)
            {
                resolutionMethod.Text = "解析失败";
                resolutionMethod.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                Console.WriteLine($"更新健康状态失败: {ex.Message}");
            }

            

        }
        double ExtractScore(string metricName, string output)
        {
            try
            {
                string pattern = $"{metricName}: (\\d+)";
                var match = Regex.Match(output, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析 {metricName} 失败: {ex.Message}");
            }
            return 0; // 默认值
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
        // 上传文件按钮点击事件
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".pdf");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // 更新TextBlock显示已选择文件
                SelectedFileText.Text = $"已选择文件: {file.Name}";
            }
            else
            {
                SelectedFileText.Text = "没有选择文件";
            }
        }
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray); // 鼠标悬停时背景变浅灰
                VisualStateManager.GoToState((Control)sender, "PointerOver", true);
            }
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Microsoft.UI.Colors.Blue); // 恢复默认背景
                VisualStateManager.GoToState((Control)sender, "Normal", true);
            }
        }


    }
}
