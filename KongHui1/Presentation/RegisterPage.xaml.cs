using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Newtonsoft.Json;
using System.Text.Json;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RegisterPage : Page
{
    public RegisterPage()
    {
        this.InitializeComponent();
    }
    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        // 返回到 MainPage.xaml
        if (this.Frame.CanGoBack)
        {
            this.Frame.GoBack();
        }
    }
    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.GoBack();
    }
    private async void OnRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        bool isValid = true;
        string account = AccountBox.Text;
        string password = PasswordBox.Password;
        string repeat_password = RepeatPasswordBox.Password;
        string email = EmailBox.Text;
        string company = CompanyBox.Text;

        string phone = PhoneBox.Text;


        //// 账号校验
        //if (string.IsNullOrEmpty(account) || !Regex.IsMatch(account, @"^\d{6,13}$"))
        //{
        //    AccountError.Visibility = Visibility.Visible;
        //    isValid = false;
        //}
        //else
        //{
        //    AccountError.Visibility = Visibility.Collapsed;
        //}

        //// 用户名校验
        //if (string.IsNullOrEmpty(company) || !Regex.IsMatch(company, @"^[\u4e00-\u9fa5a-zA-Z0-9]{1,9}$"))
        //{
        //    UserNameError.Visibility = Visibility.Visible;
        //    isValid = false;
        //}
        //else
        //{
        //    UserNameError.Visibility = Visibility.Collapsed;
        //}

        //// 邮箱校验
        //string emailPattern = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        //if (!Regex.IsMatch(email, emailPattern))
        //{
        //    EmailError.Visibility = Visibility.Visible;
        //    isValid = false;
        //}
        //else
        //{
        //    EmailError.Visibility = Visibility.Collapsed;
        //}

        //// 手机号校验
        //string phonePattern = @"^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\d{8}$";
        //if (!Regex.IsMatch(phone, phonePattern))
        //{
        //    PhoneError.Visibility = Visibility.Visible;
        //    isValid = false;
        //}
        //else
        //{
        //    PhoneError.Visibility = Visibility.Collapsed;
        //}

        // 密码校验

        if (password.Length < 6)
        {
            PasswordError.Visibility = Visibility.Visible;
            isValid = false;
        }else if(password!=repeat_password){
            PhoneError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            PasswordError.Visibility = Visibility.Collapsed;
        }

        // 若所有信息均有效，则执行注册逻辑
        if (isValid)
        {
            //将用户信息打包成对象
           var registerData = new
           {
               Account = account,
               Company = company,
               Email = email,
               Phone = phone,
               Password = password
           };

            // 发送数据到后端
            await SendRegisterDataToServer(account, company, email, phone, password);
        }
    }


    // 发送注册信息到后端
    private async Task SendRegisterDataToServer(string account, string company, string email, string phoneNumber, string password)
    {
        // 准备注册信息的 JSON 数据
        var registrationData = new Dictionary<string, string>
        {
            { "userName", account },
            { "company", company },
            { "email", email },
            { "phonenumber", phoneNumber },
            { "password", password },
            { "roleId", "3" }
        };

        // 将数据序列化为 JSON 格式
        var jsonData = JsonConvert.SerializeObject(registrationData);
        string url2 = "http://10.12.36.204:8080/register/client";
        // 使用 HttpClient 发送 POST 请求到后端
        HttpClient client = new HttpClient();

        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url2, content);
        var responseString = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseString);
        var msg = jsonDoc.RootElement.GetProperty("msg").GetString();

        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();

        // 检查请求是否成功
        if (code==(int)System.Net.HttpStatusCode.OK)
        {
            // 请求成功，显示成功提示对话框
            var dialog = new ContentDialog
            {
                Title = "注册成功",
                Content = "您的账号已成功注册。",
                CloseButtonText = "确定"
            };
            dialog.XamlRoot = this.XamlRoot; // 设置 XamlRoot

            // 显示对话框并等待关闭
            await dialog.ShowAsync();

            // 对话框关闭后跳转到主页
            this.Frame.Navigate(typeof(MainPage));  // 假设 HomePage 是你想跳转到的主页



        }
        else
        {
            // 请求失败，显示失败提示对话框
            var dialog = new ContentDialog
            {
                Title = "注册失败",
                Content =msg,
                CloseButtonText = "确定"
            };
            dialog.XamlRoot = this.XamlRoot; // 设置 XamlRoot
            await dialog.ShowAsync();
        }
    }
}

