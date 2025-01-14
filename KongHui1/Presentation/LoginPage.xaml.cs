using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Text;
using Newtonsoft.Json;

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{
    public static string Token { get; private set; }
    public static bool IsLogin { get; private set; }

    public LoginPage()
    {
        this.InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string account = AccountBox.Text;
        string password = PasswordBox.Password;

        // 创建 HttpClient 实例
        HttpClient client = new HttpClient();


        // 准备请求的 URL 和数据
        string url = "http://10.12.36.204:8080/login"; // 后端 API 地址
        var values = new Dictionary<string, string>
        {
            { "username", account },
            { "password", password }
        };
        string json = JsonConvert.SerializeObject(values);

        // 将数据转换为 JSON 格式
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 发送 POST 请求
        var response = await client.PostAsync(url, content);

        // 解析服务器响应
        var responseString = await response.Content.ReadAsStringAsync();
        var code = response.StatusCode;
        // 根据后端返回的结果执行相应操作
        if (code.Equals(System.Net.HttpStatusCode.OK))
        {
            var responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

            // 提取并保存 token
            if (responseObject.TryGetValue("token", out string token))
            {
                Token = token;  // 保存 token
                IsLogin = true;  
                this.Frame.Navigate(typeof(MainPage));
            }

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

    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        // 返回到 MainPage.xaml
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }

    private void OnRegisterTapped(object sender, TappedRoutedEventArgs e)
    {
        // 导航到注册页面 RegisterPage.xaml
        this.Frame.Navigate(typeof(RegisterPage));
    }

    private void ForgetPasswordTapped(object sender, TappedRoutedEventArgs e)
    {
        // 导航到找回密码页面 Retrieve_Password.xaml
        this.Frame.Navigate(typeof(Retrieve_Password));
    }
}
