using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System;
using Windows.UI.Popups;

namespace KongHui1.Presentation
{
    public sealed partial class CompleteQuery : Page
    {
        public CompleteQuery()
        {
            this.InitializeComponent();
            QueryDatas = new ObservableCollection<QueryData>();
            GetCompleteQueryAsync();
            DataListView.ItemsSource = QueryDatas;  // 将 QueryDatas 绑定到 ListView
        }

        // 用于存储从后端获取的数据
        public class QueryData
        {
            public string serialNumber { get; set; }
            public string model { get; set; }
            public string addr { get; set; }
            public string mainboard { get; set; }
            public string processor { get; set; }
            public string memory { get; set; }
            public string storage { get; set; }
            public string operatingSystem { get; set; }
        }

        // ObservableCollection 用于自动更新 ListView
        public ObservableCollection<QueryData> QueryDatas { get; set; }

        // 获取数据的异步方法
        private async Task GetCompleteQueryAsync(string serialNumber=null)
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
                string url = $"http://10.12.36.204:8080/pcs_info/pcs_info/list";
                if (serialNumber != null)
                {
                    url = $"http://10.12.36.204:8080/pcs_info/pcs_info/list?addr={serialNumber}"; // 根据序列号查询数据
                }
                

                // 2. 发送 GET 请求
                HttpResponseMessage response = await client.GetAsync(url);

                // 3. 处理响应
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    UpdateCompleteQuery(responseData);
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
                await ShowMessage($"发生错误: {ex.Message}");
            }
        }

        // 更新查询结果的方法
        private void UpdateCompleteQuery(string output)
        {
            try
            {
                JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(output);

                if (response.Rows != null && response.Rows.Count > 0)
                {
                    QueryDatas.Clear();
                    foreach (var queryRow in response.Rows)
                    {
                        QueryDatas.Add(queryRow);  // 将每个查询结果添加到集合中
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

        // 查询按钮点击事件
        private async void OnQueryButtonClick(object sender, RoutedEventArgs e)
        {
            // 获取输入框中的序列号
            string serialNumber = SerialNumberTextBox.Text;

            if (!string.IsNullOrEmpty(serialNumber))
            {
                 GetCompleteQueryAsync(serialNumber);
                return;
            }
            else
            {
                // 调用后端接口查询数据
                GetCompleteQueryAsync();
                return;
            }

           
        }

        // 返回按钮事件
        private void BackButton_Click(object sender, PointerRoutedEventArgs e)
        {
            if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
            {
                Frame.GoBack();   // 返回到前一个页面
            }
        }
    }

    // API 返回数据模型
    public class JsonResponse
    {
        public int Total { get; set; }
        public List<CompleteQuery.QueryData> Rows { get; set; }
        public int Code { get; set; }
        public string Msg { get; set; }
    }
}
