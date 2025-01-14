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
        // 获取输入框中的搜索关键词
        //string keyword = SearchTextBox.Text.Trim();  // 假设你有一个名为 SearchTextBox 的 TextBox 控件

        // 调用 GetAddressQueryAsync 方法并传递关键词
        await GetAddressQueryAsync();
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
}

