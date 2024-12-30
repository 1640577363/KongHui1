using System;
using System.Collections.Generic;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class User_infromation : Page
{
    public User_infromation()
    {
        this.InitializeComponent();
    } // 返回按钮点击事件
    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        if (Frame.CanGoBack) // 检查是否可以返回
        {
            Frame.GoBack(); // 返回到前一个页面
        }
    }

    // 修改信息按钮点击事件：切换到 ModifyInfoPanel
    private void ModifyInfoButton_Click(object sender, RoutedEventArgs e)
    {
        UserInfoPanel.Visibility = Visibility.Collapsed; // 隐藏用户信息面板
        ModifyInfoPanel.Visibility = Visibility.Visible; // 显示修改信息面板
    }

    // 保存修改按钮点击事件
    private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
    {
        // 获取用户输入的信息
        string newUserName = UserNameBox1.Text;
        string newEmail = EmailBox1.Text;
        string newPhoneNumber = PhoneBox1.Text;

        // 添加简单校验逻辑（可扩展）
        if (string.IsNullOrWhiteSpace(newUserName) ||
            string.IsNullOrWhiteSpace(newEmail) ||
            string.IsNullOrWhiteSpace(newPhoneNumber))
        {
            return;
        }

        // 保存逻辑（这里可以扩展为将信息发送到服务器等）
        UpdateUserInfo(newUserName, newEmail, newPhoneNumber);

        // 切换回用户信息面板
        ModifyInfoPanel.Visibility = Visibility.Collapsed;
        UserInfoPanel.Visibility = Visibility.Visible;
    }

    // 取消修改按钮点击事件
    private void CancelChangesButton_Click(object sender, RoutedEventArgs e)
    {
        // 切换回用户信息面板
        ModifyInfoPanel.Visibility = Visibility.Collapsed;
        UserInfoPanel.Visibility = Visibility.Visible;
    }

    // 显示错误消息


    // 更新用户信息的方法
    private void UpdateUserInfo(string userName, string email, string phoneNumber)
    {
        // 更新 UI 显示的用户信息
        // 假设用户信息显示 TextBlock 的名称是固定的
        foreach (var child in UserInfoPanel.Children)
        {
            if (child is StackPanel panel && panel.Children.Count > 1)
            {
                if (panel.Children[0] is TextBlock label)
                {
                    if (label.Text.Contains("用户名"))
                        (panel.Children[1] as TextBlock).Text = userName;
                    if (label.Text.Contains("邮箱"))
                        (panel.Children[1] as TextBlock).Text = email;
                    if (label.Text.Contains("手机号"))
                        (panel.Children[1] as TextBlock).Text = phoneNumber;
                }
            }
        }

        // TODO: 如果需要，添加与后端交互的逻辑，将新数据同步到服务器。
    }
}

