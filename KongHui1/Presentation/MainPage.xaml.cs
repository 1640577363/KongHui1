namespace KongHui1.Presentation
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Windows.UI;
    using Windows.UI.Core;
    using Microsoft.UI.Xaml;

    public sealed partial class MainPage : Page
    {
        // 用于记录当前选中的按钮
        private Button _selectedButton;

        public MainPage()
        {
            this.InitializeComponent();
        }
        
        // 登陆页面
        private void UserImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        // 各按钮点击事件
        private void HomeCheckButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonSelection((Button)sender);
            MainFrame.Navigate(typeof(HomeCheckPage));
        }

        private void DriveManagementButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonSelection((Button)sender);
            MainFrame.Navigate(typeof(DrivePage));
        }

        private void SystemButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButtonSelection((Button)sender);
            MainFrame.Navigate(typeof(SystemPage));
        }

        private void HelpPageButton_Click(object sender, RoutedEventArgs e)
        {
            ////旧版本
            //UpdateButtonSelection((Button)sender);
            //MainFrame.Navigate(typeof(HelpPage));

            //新版本
            UpdateButtonSelection((Button)sender);

            // 检查用户是否已经登录
            if (LoginPage.IsLogin)
            {
                // 如果已登录，导航到 HelpPage.xaml
                MainFrame.Navigate(typeof(HelpPage));
            }
            else
            {
                // 如果未登录，导航到 LoginPage.xaml
                this.Frame.Navigate(typeof(LoginPage));
            }
        }

        // PointerEntered 事件处理程序 - 将光标改为手形
        private void UserImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (Microsoft.UI.Xaml.Window.Current?.CoreWindow != null)
            {
                Microsoft.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
            }
        }

        // PointerExited 事件处理程序 - 恢复默认光标
        private void UserImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Microsoft.UI.Xaml.Window.Current?.CoreWindow != null)
            {
                Microsoft.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            }
        }

        // PointerEntered - 当鼠标进入按钮时，设置背景颜色
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button != _selectedButton)
            {
                button.Background = new SolidColorBrush(Color.FromArgb(255, 222, 236, 255)); // 浅蓝色
            }
        }

        // PointerExited - 当鼠标离开按钮时，恢复背景颜色
        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button != _selectedButton)
            {
                button.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255)); // 默认颜色
            }
        }

        // 更新按钮选择状态，选中按钮设置背景色
        private void UpdateButtonSelection(Button button)
        {
            // 如果当前按钮已选中，则不需要切换
            if (_selectedButton != null && _selectedButton != button)
            {
                // 恢复上一个选中按钮的颜色
                _selectedButton.Background = new SolidColorBrush(Color.FromArgb(255, 244, 249, 255)); // 恢复默认颜色
            }

            _selectedButton = button;
            // 设置选中按钮的背景颜色
            _selectedButton.Background = new SolidColorBrush(Color.FromArgb(255, 222, 236, 255)); // 设置选中颜色为DEECFF
        }
    }
}
