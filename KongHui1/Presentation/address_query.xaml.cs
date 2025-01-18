using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Newtonsoft.Json;
using Windows.Foundation;
using System.Windows.Input;
using Windows.Foundation.Collections;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
public class AddressData
{
    public string siteName { get; set; }
    public string phoneNumber { get; set; }
    public string address { get; set; }
}
public sealed partial class address_query : Page
{
    public ObservableCollection<AddressData> AddressList { get; set; }
    public address_query()
    {
        this.InitializeComponent();
        AddressList = new ObservableCollection<AddressData>();
        DataListView.ItemsSource = AddressList;
        GetAddressQueryAsync();

    }
    private async Task GetAddressQueryAsync(string siteName = null)
    {
        try
        {
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(LoginPage.Token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
            }
            //string Token = "eyJhbGciOiJIUzUxMiJ9.eyJsb2dpbl91c2VyX2tleSI6IjEwLjE0LjMwLjU5QFN1biBKYW4gMTIgMjA6MzU6MzggQ1NUIDIwMjUifQ.VFLs4-gFaFkxy3prNwYUyx8SluaNFdjt0sgw0a8bMKB8XB_9hGCgWlhXLN1mHxr1qLTXXNRxklUZ0OV5nYee6g"; // 请用实际的 Token 替换
            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            // 根据关键词修改 URL
            string url = "http://10.12.36.204:8080/address/address/list";
            if (!string.IsNullOrEmpty(siteName))
            {
                url = $"http://10.12.36.204:8080/address/address/list?siteName={siteName}";  // 根据关键词进行筛选
            }

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                UpdateAddressQuery(responseData);  // 更新前端数据显示
                UpdateRightAddressQuery(responseData);
                Debug.WriteLine(responseData);
            }
            else
            {
                ShowMessage("无法获取数据，请稍后再试。");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"发生异常: {ex.Message}");
            ShowMessage($"发生错误: {ex.Message}");
        }
    }

    private async void UpdateAddressQuery(string output)
    {
        try
        {
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(output);

            if (response.Rows != null && response.Rows.Count > 0)
            {
                AddressList.Clear();
                foreach (var row in response.Rows)
                {
                    AddressList.Add(row);  // 将每个查询结果添加到集合中
                }
            }
            else
            {
               await ShowMessage("没有查询到数据");
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"解析失败: {ex.Message}");
        }
    }

    // 搜索按钮点击事件
    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        // 获取输入框中的搜索关键词
        string keyword = SearchTextBox.Text.Trim();  // 假设你有一个名为 SearchTextBox 的 TextBox 控件

        // 调用 GetAddressQueryAsync 方法并传递关键词
        await GetAddressQueryAsync(keyword);
    }
    private async void SearchButton_Click2(object sender, RoutedEventArgs e)
    {
        // 获取输入框中的搜索关键词（如果需要）
        string keyword = SearchTextBox.Text.Trim();  // 获取搜索框中的文本，并去掉前后空格

        // 调用 GetAddressQueryAsync 方法并传递关键词
        await GetAddressQueryAsync();

        // 清空搜索框
        SearchTextBox.Text = "";
    }


    // 弹出消息对话框
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
        if (Frame.CanGoBack)
        {
            Frame.GoBack();   // 返回到前一个页面
        }
    }

    // JSON 响应类
    public class JsonResponse
    {
        public int Total { get; set; }
        public List<AddressData> Rows { get; set; }
        public int Code { get; set; }
    }

    private async void OnItemClick(object sender, RoutedEventArgs e)
    {
        // 确保 sender 是 Button 类型
        Button clickedButton = sender as Button;

        if (clickedButton != null)
        {
            // 获取 DataContext 为 AddressData 类型
            var clickedItem = clickedButton.DataContext as AddressData;

            if (clickedItem != null)
            {
                // 获取站点名称
                string siteName = clickedItem.siteName;

                try
                {
                    // 创建 HttpClient
                    HttpClient client = new HttpClient();

                    // 如果 Token 存在，设置 Authorization 头
                    if (!string.IsNullOrEmpty(LoginPage.Token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoginPage.Token);
                    }

                    // 根据关键词修改 URL
                    string url = "http://10.12.36.204:8080/address/address/list";
                    if (!string.IsNullOrEmpty(siteName))
                    {
                        url = $"http://10.12.36.204:8080/address/address/list?siteName={siteName}";  // 根据关键词进行筛选
                    }

                    // 发送 GET 请求
                    HttpResponseMessage response = await client.GetAsync(url);

                    // 如果请求成功
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        // 更新前端数据
                        UpdateRightAddressQuery(responseData);

                        // 输出返回的数据
                        Debug.WriteLine(responseData);
                    }
                    else
                    {
                        // 请求失败时显示提示信息
                        ShowMessage("无法获取数据，请稍后再试。");
                    }
                }
                catch (Exception ex)
                {
                    // 捕获异常并输出调试信息
                    Debug.WriteLine($"发生异常: {ex.Message}");
                    ShowMessage($"发生错误: {ex.Message}");
                }
            }
        }
    }


    private async void UpdateRightAddressQuery(string output)
    {
        try
        {
            // 解析 JSON 响应
            JsonResponse response = JsonConvert.DeserializeObject<JsonResponse>(output);

            if (response.Rows != null && response.Rows.Count > 0)
            {
                // 获取第一条记录
                var firstRecord = response.Rows.FirstOrDefault();

                if (firstRecord != null)
                {
                    // 更新 UI 控件
                    CompanyNameTextBlock.Text = firstRecord.siteName;  // 假设第一条记录中包含 "siteName"
                    CompanyAddressTextBlock.Text = firstRecord.address; // 假设包含 "address"
                    CompanyPhoneTextBlock.Text = firstRecord.phoneNumber; // 假设包含 "phoneNumber"
                    CompanyWorkingHoursTextBlock.Text = "周一至周五 9:00-18:00"; // 示例工作时间，实际根据数据调整

                    // 如果有图片链接，可以设置图片源
                    //if (!string.IsNullOrEmpty(firstRecord.imageUrl))
                    //{
                    //    CompanyImage.Source = new Uri(firstRecord.imageUrl);  // 假设返回的第一条记录中有 "imageUrl"
                    //}
                }
            }
            else
            {
                await ShowMessage("没有查询到数据");
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"解析失败: {ex.Message}");
        }
    }


}

