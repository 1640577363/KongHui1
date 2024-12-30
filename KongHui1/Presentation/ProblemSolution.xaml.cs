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
public sealed partial class ProblemSolution : Page
{
    public ProblemSolution()
    {
        this.InitializeComponent();
    }
    private void ToggleArrow_Click(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = sender as ToggleButton;
        if (toggleButton != null)
        {
            // 根据 ToggleButton 的 Tag 来识别对应的文本块
            switch (toggleButton.Tag)
            {
                case "1":
                    ToggleVisibility(AdditionalTextBlock, toggleButton);
                    break;
                case "2":
                    ToggleVisibility(AdditionalTextBlock2, toggleButton);
                    break;
                case "3":
                    ToggleVisibility(AdditionalTextBlock3, toggleButton);
                    break;
                case "4":
                    ToggleVisibility(AdditionalTextBlock4, toggleButton);
                    break;
                case "5":
                    ToggleVisibility(AdditionalTextBlock5, toggleButton);
                    break;
                case "6":
                    ToggleVisibility(AdditionalTextBlock6, toggleButton);
                    break;
            }
        }
    }

    private void ToggleVisibility(Border additionalTextBlock, ToggleButton toggleButton)
    {
        if (additionalTextBlock.Visibility == Visibility.Collapsed)
        {
            additionalTextBlock.Visibility = Visibility.Visible;
            toggleButton.Content = "▲"; // 更改箭头方向为向上
        }
        else
        {
            additionalTextBlock.Visibility = Visibility.Collapsed;
            toggleButton.Content = "▼"; // 更改箭头方向为向下
        }
    }

    private void BackButton_Click(object sender, PointerRoutedEventArgs e)
    {
        if (Frame.CanGoBack)  // 使用 this.Frame 引用当前页面的 Frame
        {
            Frame.GoBack();   // 返回到前一个页面
        }
    }
}
