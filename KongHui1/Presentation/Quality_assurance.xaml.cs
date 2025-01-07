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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KongHui1.Presentation
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Quality_assurance : Page
	{
		public Quality_assurance()
		{
			this.InitializeComponent();
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
                button.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray); // 鼠标悬停时背景变浅灰
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
    }


}
