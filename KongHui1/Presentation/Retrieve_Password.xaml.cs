using System;
using System.Text.RegularExpressions; // 用于验证正则表达式
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace KongHui1.Presentation
{
    public sealed partial class Retrieve_Password : Page
    {
        public Retrieve_Password()
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

        // 获取验证码按钮点击事件
        private void GetCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            // 模拟获取验证码，可以通过调用后端 API
            string email = EmailBox.Text;

            // 验证邮箱格式
            if (IsValidEmail(email))
            {
                // 调用后端获取验证码的 API
                // 示例代码：SendCaptchaToEmail(email);
                CaptchaError.Visibility = Visibility.Collapsed;
                // 模拟获取验证码成功
                ContentDialog dialog = new ContentDialog
                {
                    Title = "验证码已发送",
                    Content = "验证码已发送至您的邮箱，请查收。",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot
                    // 设置 XamlRoot
                };
                dialog.ShowAsync();
            }
            else
            {
                // 如果邮箱格式不正确
                CaptchaError.Visibility = Visibility.Visible;
            }
        }

        // 修改密码按钮点击事件
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string account = AccountBox.Text;
            string email = EmailBox.Text;
            string captcha = CaptchaBox.Text;
            string newPassword = ResetPasswordBox.Password;
            string reconfirmPassword = ReconfirmBox.Password;

            bool isValid = true;

            // 验证账号格式（6-13位纯数字）
            if (!IsValidAccount(account))
            {
                AccountError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                AccountError.Visibility = Visibility.Collapsed;
            }

            // 验证邮箱格式
            if (!IsValidEmail(email))
            {
                EmailError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                EmailError.Visibility = Visibility.Collapsed;
            }

            // 验证密码长度
            if (newPassword.Length < 6)
            {
                ResetPasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ResetPasswordError.Visibility = Visibility.Collapsed;
            }

            // 验证两次密码是否一致
            if (newPassword != reconfirmPassword)
            {
                ReconfirmError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ReconfirmError.Visibility = Visibility.Collapsed;
            }

            // 如果所有输入都合法，调用后端 API 修改密码
            if (isValid)
            {
                // 示例代码：SendPasswordChangeRequest(account, email, captcha, newPassword);
                ContentDialog dialog = new ContentDialog
                {
                    Title = "修改密码成功",
                    Content = "您的密码已成功修改。",
                    CloseButtonText = "确定",
                    XamlRoot = this.XamlRoot // 设置 XamlRoot
                };
                dialog.ShowAsync();
            }
        }

        // 验证账号格式（6-13位数字）
        private bool IsValidAccount(string account)
        {
            return Regex.IsMatch(account, @"^\d{6,13}$");
        }

        // 验证邮箱格式
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }
    }
}
