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
using Newtonsoft.Json;
using System.Management;
using Windows.UI.Popups;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;  // 需要引入这个命名空间
using System.Net.Http;
using System.Text;

namespace KongHui1.Presentation
{
    public sealed partial class FeedbackPage : Page, INotifyPropertyChanged
    {
        private string _hardwareId;  // 硬盘ID字段

        public string HardwareId
        {
            get => _hardwareId;
            set
            {
                if (_hardwareId != value)
                {
                    _hardwareId = value;
                    OnPropertyChanged(nameof(HardwareId));  // 通知 UI 更新
                }
            }
        }

        public ObservableCollection<Row> Feedbacks { get; set; }  // 反馈项集合

        public event PropertyChangedEventHandler PropertyChanged;

        public FeedbackPage()
        {
            this.InitializeComponent();
            Feedbacks = new ObservableCollection<Row>();  // 初始化集合
            this.DataContext = this;  // 设置数据上下文，绑定属性
            StartGetFeedback();  // 获取反馈
        }

        private void BackButton_Click(object sender, PointerRoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Button Clicked!");

                // 获取页面上的控件值
                string problemType = GetSelectedProblemTypeIndex();
                string problemDescription = PDTextBox.Text;
                string companyName = CompanyNameTextBox.Text;
                string contactPhone = ContactPhoneTextBox.Text;

                string hardwareId = GetHardDiskID();

                HttpClient client = new HttpClient();

                //if (!string.IsNullOrEmpty(LoginPage.Token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
                //}
                String Token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjEwLjE0LjQzLjMyQFR1ZSBKYW4gMDcgMTQ6NDU6NTUgQ1NUIDIwMjUifQ.Lzy1IdZWJuNzR2h6-jZf9994ct2Yg6ROMYv5kORVHpKs4XbCtRHrBWrMVAd7oAjYC9vAZR713eX04mSZaXRDEw";
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                string url = "http://10.12.36.204:8080/Issues_support/Issues_support"; // 后端 API 地址
                var values = new Dictionary<string, string>
            {
                { "hardwareId", hardwareId },
                { "problemType", problemType },
                { "problemDescription", problemDescription },
                { "company", companyName },
                { "contactPhone", contactPhone }
            };
                string json = JsonConvert.SerializeObject(values);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var code = response.StatusCode;
                if (code.Equals(System.Net.HttpStatusCode.OK))
                {


                    //刷新页面
                    this.Frame.Navigate(typeof(FeedbackPage));
                }
                else
                {
                    ShowMessage("提交失败");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生异常: {ex.Message}");
                ShowMessage("发生了错误，请稍后再试。");
            }
        }

        private async Task StartGetFeedback()
        {
            try
            {
                HttpClient client = new HttpClient();
                string hardwareId = GetHardDiskID();
                
                // 设置硬盘 ID 到绑定属性
                HardwareId = hardwareId;  // 更新 HardwareId 属性
                //if (!string.IsNullOrEmpty(LoginPage.Token))
                //{
                //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
                //}
                String Token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjEwLjE0LjQzLjMyQFR1ZSBKYW4gMDcgMTQ6NDU6NTUgQ1NUIDIwMjUifQ.Lzy1IdZWJuNzR2h6-jZf9994ct2Yg6ROMYv5kORVHpKs4XbCtRHrBWrMVAd7oAjYC9vAZR713eX04mSZaXRDEw";
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                string url = $"http://10.12.36.204:8080/Issues_support/Issues_support/list?hardwareId={hardwareId}";
                var response = await client.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();
                var responseString = await response.Content.ReadAsStringAsync();
                var code = response.StatusCode;

                if (code.Equals(System.Net.HttpStatusCode.OK))
                {
                    UpdateFeedback(responseString);  // 更新反馈列表
                }
                else
                {
                    ShowMessage("获取反馈失败");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"错误: {ex.Message}");
            }
        }

        private void UpdateFeedback(string output)
        {
            try
            {
                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(output);

                if (response.Rows != null && response.Rows.Count > 0)
                {
                    Feedbacks.Clear();
                    foreach (var issue in response.Rows)
                    {
                        Feedbacks.Add(issue);  // 将每个问题添加到集合中
                    }
                }
                else
                {
                    ShowMessage("没有问题数据");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"解析失败: {ex.Message}");
            }
        }

        public class Row
        {
            public int IssueId { get; set; }
            public string ProblemDescription { get; set; }
            public string ResolutionMethod { get; set; }
            public int Status { get; set; }
        }

        public class JsonResponse
        {
            public int Total { get; set; }
            public List<Row> Rows { get; set; }
            public int Code { get; set; }
            public string Msg { get; set; }
        }

        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = message,
                CloseButtonText = "确定"
            };
           // await dialog.ShowAsync();
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
            return "-1";
        }

        // 获取硬盘ID的方法
        static string GetHardDiskID()
        {
            foreach (ManagementObject disk in new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia").Get())
            {
                var serialNumber = disk["SerialNumber"]?.ToString().Trim().TrimEnd('.');
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return serialNumber;
                }
            }
            return "未找到硬盘ID";
        }

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
                SelectedFileText.Text = $"已选择文件: {file.Name}";
            }
            else
            {
                SelectedFileText.Text = "没有选择文件";
            }
        }
        


        // INotifyPropertyChanged 实现
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
