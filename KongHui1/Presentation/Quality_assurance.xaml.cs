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
using System.Collections.ObjectModel;
using static KongHui1.Presentation.Quality_assurance;
using System.Diagnostics;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation
{
    public class WarrantyData
    {
        public string hardwareId { get; set; }
        public string warrantyType { get; set; }
        public string status { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }

    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Quality_assurance : Page
    {
        public Quality_assurance()
        {
            this.InitializeComponent();
            WarrantyDatas = new ObservableCollection<WarrantyData>();
            GetWarrantyQueryAsync();
            DataListView.ItemsSource = WarrantyDatas;
        }
        public ObservableCollection<WarrantyData> WarrantyDatas { get; set; }
        // 用于存储从后端获取的数据
       

        private async Task GetWarrantyQueryAsync(string hardwareId = null)
        {
            try
            {
                // 1. 创建 HttpClient 对象
                HttpClient client = new HttpClient();
                if (!string.IsNullOrEmpty(LoginPage.Token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
                }
                //string Token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjEwLjE0LjQwLjE5NkBXZWQgSmFuIDA4IDA5OjM1OjM0IENTVCAyMDI1In0.xv7A2yPHkuD27OrXEz4PzudRvWI6PKW5wVOuZbJgAQkacYxVRzO_9luzPSYKyMhDiTptYJZddpBygiERrxITAA"; // 请用实际的 Token 替换
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
                string url = $"http://10.12.36.204:8080/warranty/warranty/list";
                if (hardwareId != null)
                {
                    url = $"http://10.12.36.204:8080/warranty/warranty/list?hardwareId={hardwareId}"; // 根据序列号查询数据
                }


                // 2. 发送 GET 请求
                HttpResponseMessage response = await client.GetAsync(url);

                // 3. 处理响应
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    UpdateWarrantyQuery(responseData);
                    Debug.WriteLine(responseData);
                }
                else
                {
                    // 请求失败时的处理
                    ShowMessage("无法获取数据，请稍后再试。");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生异常: {ex.Message}");
                 ShowMessage($"发生错误: {ex.Message}");
            }
        }
        // 更新查询结果的方法
        private void UpdateWarrantyQuery(string output)
        {
            try
            {
                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(output);

                if (response.Rows != null && response.Rows.Count > 0)
                {
                    WarrantyDatas.Clear();
                    foreach (var WarrantyRow in response.Rows)
                    {
                        WarrantyDatas.Add(WarrantyRow);  // 将每个查询结果添加到集合中
                    }
                }
                else
                {
                    ShowMessage("没有查询到数据");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"解析失败: {ex.Message}");
            }
        }
        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            string hardwareId = SerialNumberTextBox.Text;
            if (!string.IsNullOrEmpty(hardwareId))
            {
                GetWarrantyQueryAsync(hardwareId); // 使用用户输入的序列号进行查询
            }
            else
            {
                // 调用后端接口查询数据
                GetWarrantyQueryAsync();
                return;
            }
        }


        // 显示消息的辅助方法
        private async Task ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = message,
                CloseButtonText = "确定"
            };

            await dialog.ShowAsync();
        }

        private void BackButton_Click(object sender, PointerRoutedEventArgs e)
        {
            if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
            {
                Frame.GoBack();   // 返回到前一个页面
            }
        }
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue); // 鼠标悬停时背景变浅灰
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
        // API 返回数据模型
        public class JsonResponse
        {
            public int Total { get; set; }
            public List<WarrantyData> Rows { get; set; }
            public int Code { get; set; }

        }


    }
}
