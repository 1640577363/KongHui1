using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using System.Data.Common;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Data;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;




namespace KongHui1.Presentation
{
    /// <summary>
    /// 显示驱动信息的页面
    /// </summary>
    public sealed partial class DrivePage : Page
    {
        // MySQL 数据库连接字符串
        private string _connectionString = "Server=localhost;Database=qd;User ID=root;Password=123456;";
        List<DriverInfo> driverInfoList = new List<DriverInfo>();
        public DrivePage()
        {
            this.InitializeComponent();
            this.Loaded += DrivePage_Loaded; // 在页面加载完成后再调用
        }

        private async void DrivePage_Loaded(object sender, RoutedEventArgs e)
        {
            await StartDriverMonitoring();
        }


        private async void OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // 读取JSON文件
                string filePath = @"C:\yoloSet\yolov11\drivers_to_update.json";
                if (File.Exists(filePath))
                {
                    // 读取文件内容
                    string jsonContent = File.ReadAllText(filePath);

                    // 反序列化JSON内容为C#对象
                    List<DriverInfo> driversToUpdate = JsonConvert.DeserializeObject<List<DriverInfo>>(jsonContent);

                    // 循环每个驱动并执行更新操作
                    foreach (var driver in driversToUpdate)
                    {
                        DriversCountTextBlock.Text = $"正在更新驱动：{driver.Name}";
                        //await RunUpdateDriverPythonScript(driver.Name);
                        // 绝对路径 - 确保替换为你自己的路径
                        string pythonScriptPath = @"D:\Project\UNO2\KongHui1\Python\driverChange.py";  // Python 脚本路径

                        // 使用环境变量中的 Python 可执行文件
                        string pythonExecutable = "python"; // Python 可执行文件名，已经添加到环境变量中

                        // 构造传递给 Python 脚本的参数
                        string arguments = $"{pythonScriptPath} \"{driver.Name}\""; // 使用双引号包裹驱动名称，以避免空格问题

                        // 输出路径和参数到调试控制台，确认路径是否正确
                        System.Diagnostics.Debug.WriteLine($"Python script path: {pythonScriptPath}");
                        System.Diagnostics.Debug.WriteLine($"Python script arguments: {arguments}");

                        var startInfo = new ProcessStartInfo
                        {
                            FileName = pythonExecutable, // 使用 python 命令来调用
                            Arguments = arguments, // 将驱动名称作为参数传递给 Python 脚本
                            RedirectStandardOutput = true, // 重定向标准输出
                            RedirectStandardError = true,  // 重定向错误输出
                            UseShellExecute = false,      // 不使用外壳执行
                            CreateNoWindow = true,        // 不创建窗口
                            WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)  // 设置工作目录为 Python 脚本所在的目录
                        };

                        // 创建新的进程
                        var pythonProcess = new Process { StartInfo = startInfo };

                        // 订阅输出和错误流
                        pythonProcess.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Python Output: {e.Data}");
                            }
                        };

                        pythonProcess.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Python Error: {e.Data}");
                            }
                        };

                        // 启动 Python 脚本并开始读取输出
                        if (pythonProcess.Start())
                        {
                            pythonProcess.BeginOutputReadLine();
                            pythonProcess.BeginErrorReadLine();

                            // 等待 Python 脚本执行完成
                            await Task.Run(() => pythonProcess.WaitForExit());

                            // Python 脚本执行完毕后，更新 TextBlock 内容
                            string updateStatus = pythonProcess.ExitCode == 0 ? "更新成功" : "更新失败";

                            // 使用 Dispatcher 在 UI 线程更新 TextBlock
                            // 确保从 UI 线程中更新 TextBlock

                            DriversCountTextBlock.Text = $"{driver.Name}: {updateStatus}";

                            var timer = new DispatcherTimer();
                            timer.Interval = TimeSpan.FromSeconds(5);
                            timer.Tick += (sender, e) =>
                            {
                                // 更新第二个 TextBlock



                                // 停止计时器
                                timer.Stop();
                            };

                            // 启动计时器
                            timer.Start();

                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to start Python script.");

                            // 如果启动失败，更新 UI

                            DriversCountTextBlock.Text = "驱动更新状态: 启动失败";

                        }
                        // 输出正在更新的驱动信息到控制台
                        //Debug.WriteLine($"正在更新驱动：{driver.Name}");
                        System.Diagnostics.Debug.WriteLine($"更新的驱动名称: {driver.Name}");
                    }
                    StartDriverMonitoring();
                    // 更新完成后输出信息
                    Debug.WriteLine("所有驱动程序已更新！");
                }
                else
                {
                    // 输出错误信息
                    Debug.WriteLine("找不到JSON文件，请检查路径。");
                }
            }
            catch (Exception ex)
            {
                // 如果发生错误，输出错误信息
                Debug.WriteLine($"发生错误: {ex.Message}");
            }
        }
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前点击按钮的驱动名称
            if (sender is Button button && button.Tag is string driverName)
            {
                // 打印驱动名称到控制台或调试输出
                System.Diagnostics.Debug.WriteLine($"更新的驱动名称: {driverName}");

                // 读取 drivers_to_update.json 文件中的数据
                string filePath = @"D:\Project\UNO2\KongHui1\Python\drivers_to_update.json";

                if (File.Exists(filePath))
                {
                    try
                    {
                        // 读取 JSON 文件内容
                        string jsonContent = File.ReadAllText(filePath);
                        var driversToUpdate = JsonConvert.DeserializeObject<List<DriverInfo>>(jsonContent);

                        // 检查驱动名称是否存在于 JSON 文件中
                        bool driverExists = driversToUpdate.Any(driver => driver.Name == driverName);

                        if (driverExists)
                        {
                            // 如果存在该驱动名称，调用后端的 Python 脚本进行更新
                            System.Diagnostics.Debug.WriteLine("驱动需要更新，开始更新...");
                            DriversCountTextBlock.Text = $"正在更新驱动：{driverName}";
                            await RunUpdateDriverPythonScript(driverName);


                        }
                        else
                        {
                            // 如果不在列表中，输出提示信息
                            System.Diagnostics.Debug.WriteLine("驱动不在更新列表中，不进行更新.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理读取 JSON 文件时的异常
                        System.Diagnostics.Debug.WriteLine($"读取 JSON 文件时出错: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"文件 {filePath} 不存在！");
                }
            }
        }
        private async Task RunUpdateDriverPythonScript(string driverName)
        {
            // 绝对路径 - 确保替换为你自己的路径
            string pythonScriptPath = @"C:\Users\LANY\Desktop\python脚本\driverChange.py";  // Python 脚本路径

            // 使用环境变量中的 Python 可执行文件
            string pythonExecutable = "python"; // Python 可执行文件名，已经添加到环境变量中

            // 构造传递给 Python 脚本的参数
            string arguments = $"{pythonScriptPath} \"{driverName}\""; // 使用双引号包裹驱动名称，以避免空格问题

            // 输出路径和参数到调试控制台，确认路径是否正确
            System.Diagnostics.Debug.WriteLine($"Python script path: {pythonScriptPath}");
            System.Diagnostics.Debug.WriteLine($"Python script arguments: {arguments}");

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExecutable, // 使用 python 命令来调用
                Arguments = arguments, // 将驱动名称作为参数传递给 Python 脚本
                RedirectStandardOutput = true, // 重定向标准输出
                RedirectStandardError = true,  // 重定向错误输出
                UseShellExecute = false,      // 不使用外壳执行
                CreateNoWindow = true,        // 不创建窗口
                WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)  // 设置工作目录为 Python 脚本所在的目录
            };

            // 创建新的进程
            var pythonProcess = new Process { StartInfo = startInfo };

            // 订阅输出和错误流
            pythonProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Python Output: {e.Data}");
                }
            };

            pythonProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Python Error: {e.Data}");
                }
            };

            // 启动 Python 脚本并开始读取输出
            if (pythonProcess.Start())
            {
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();

                // 等待 Python 脚本执行完成
                await Task.Run(() => pythonProcess.WaitForExit());

                // Python 脚本执行完毕后，更新 TextBlock 内容
                string updateStatus = pythonProcess.ExitCode == 0 ? "更新成功" : "更新失败";

                // 使用 Dispatcher 在 UI 线程更新 TextBlock
                // 确保从 UI 线程中更新 TextBlock

                DriversCountTextBlock.Text = $"{driverName}: {updateStatus}";

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2.5);
                timer.Tick += (sender, e) =>
                {
                    // 更新第二个 TextBlock

                    StartDriverMonitoring();

                    // 停止计时器
                    timer.Stop();
                };

                // 启动计时器
                timer.Start();

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to start Python script.");

                // 如果启动失败，更新 UI

                DriversCountTextBlock.Text = "驱动更新状态: 启动失败";

            }
        }




        private async void OnDetectButtonClick(object sender, RoutedEventArgs e)
        {
            // 调用 StartDriverMonitoring 重新执行驱动检测逻辑
            await StartDriverMonitoring();

            Console.WriteLine($"更新的驱动名称: ");
        }



        // 启动驱动查找的 Python 脚本,修改为返回 Task，而不是 void
        private async Task StartDriverMonitoring()
        {
            DriversCountTextBlock.Text = "正在检测驱动中，请稍候...";
            // 执行 Python 脚本查找驱动
            // 绝对路径 - 确保替换为你自己的路径
            string pythonScriptPath = @"D:\Project\UNO2\KongHui1\Python\driversFind.py"; // 绝对路径指向 Python 脚本

            // 使用环境变量中的 Python 可执行文件
            string pythonExecutable = "python"; // Python 可执行文件名，已经添加到环境变量中

            // 输出路径到调试控制台，确认路径是否正确
            System.Diagnostics.Debug.WriteLine($"Python script path: {pythonScriptPath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExecutable,  // 使用 python 命令来调用
                Arguments = pythonScriptPath, // Python 脚本的绝对路径
                RedirectStandardOutput = true, // 重定向标准输出
                RedirectStandardError = true,  // 重定向错误输出
                UseShellExecute = false,      // 不使用外壳执行
                CreateNoWindow = true,        // 不创建窗口
                WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)  // 设置工作目录为 Python 脚本所在的目录
            };

            // 创建新的进程
            var pythonProcess = new Process { StartInfo = startInfo };

            // 订阅输出和错误流
            pythonProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Python Output: {e.Data}");
                }
            };

            pythonProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Python Error: {e.Data}");
                }
            };

            // 启动 Python 脚本并开始读取输出
            if (pythonProcess.Start())
            {
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
                await pythonProcess.WaitForExitAsync(); // 显式等待脚本执行完成
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Failed to start Python script.");
            }






            //// 获取数据库中的驱动信息
            //var driverInfoList = await GetDriverInfoFromDatabaseAsync();

            driverInfoList.Clear(); // 清除上次检测的残留数据
            //List<DriverInfo> driverInfoList = new List<DriverInfo>();
            string query = "SELECT name, version, newversion, category,isButton FROM drivers";  // 根据实际情况修改 SQL 查询

            try
            {
                // 创建 MySQL 连接对象
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // 使用 DbDataReader 而非 MySqlDataReader
                        using (DbDataReader reader = await command.ExecuteReaderAsync()) // 使用 DbDataReader
                        {
                            while (await reader.ReadAsync())
                            {
                                // 创建 DriverInfo 对象并添加到列表中
                                driverInfoList.Add(new DriverInfo
                                {
                                    Name = reader.GetString(0),
                                    Version = reader.GetString(1),
                                    NewVersion = reader.GetString(2),
                                    Category = reader.GetString(3),
                                    IsButton = reader.GetString(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库查询出错: {ex.Message}");
            }




            // 确保数据库数据完全加载完成后才调用 DisplayDriverInfo
            if (driverInfoList != null && driverInfoList.Any())
            {
                // 调用 DisplayDriverInfo 来按设备类别进行分组并绑定数据
                var groupedData = driverInfoList
                .GroupBy(d => d.Category)  // 按设备类别进行分组
                .Select(g => new
                {
                    Key = $"{g.Key}（{g.Count()}）", // 动态拼接驱动类别和数量
                    Items = g.ToList()  // 获取该类别下的所有驱动信息
                })
                .ToList();

                // 绑定分组数据到 ItemsControl
                DriverInfoListView.ItemsSource = groupedData;

                // 更新驱动数量显示
                DriversCountTextBlock.Text = $"本地共发现 {driverInfoList.Count} 个驱动";
            }
            else
            {
                DriversCountTextBlock.Text = "未找到任何驱动";
            }



        }


        // 显示驱动信息
        //private void DisplayDriverInfo(List<DriverInfo> driverInfoList)
        //{
        //    // 按设备类别进行分组，并动态生成名称和数量
        //    var groupedData = driverInfoList
        //        .GroupBy(d => d.Category)  // 按设备类别进行分组
        //        .Select(g => new
        //        {
        //            Key = $"{g.Key}（{g.Count()}）", // 动态拼接驱动类别和数量
        //            Items = g.ToList()  // 获取该类别下的所有驱动信息
        //        })
        //        .ToList();

        //    // 绑定分组数据到 ItemsControl
        //    DriverInfoListView.ItemsSource = groupedData;
        //}

        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray); // 鼠标悬停时背景变浅灰
            }
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // 将颜色字符串 "#71A6F7" 转换为 ARGB 值
                byte a = 255;  // 默认完全不透明
                byte r = 113;  // 红色分量
                byte g = 166;  // 绿色分量
                byte b = 247;  // 蓝色分量

                button.Background = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue); // 恢复默认背景
            }
        }

        // 启动查找驱动程序的 Python 脚本
        //private async Task RunDriverFindPythonScript()
        //{
        //    // 绝对路径 - 确保替换为你自己的路径
        //    string pythonScriptPath = @"C:\yoloSet\yolov11\driversFind.py"; // 绝对路径指向 Python 脚本

        //    // 使用环境变量中的 Python 可执行文件
        //    string pythonExecutable = "python"; // Python 可执行文件名，已经添加到环境变量中

        //    // 输出路径到调试控制台，确认路径是否正确
        //    System.Diagnostics.Debug.WriteLine($"Python script path: {pythonScriptPath}");

        //    var startInfo = new ProcessStartInfo
        //    {
        //        FileName = pythonExecutable,  // 使用 python 命令来调用
        //        Arguments = pythonScriptPath, // Python 脚本的绝对路径
        //        RedirectStandardOutput = true, // 重定向标准输出
        //        RedirectStandardError = true,  // 重定向错误输出
        //        UseShellExecute = false,      // 不使用外壳执行
        //        CreateNoWindow = true,        // 不创建窗口
        //        WorkingDirectory = Path.GetDirectoryName(pythonScriptPath)  // 设置工作目录为 Python 脚本所在的目录
        //    };

        //    // 创建新的进程
        //    var pythonProcess = new Process { StartInfo = startInfo };

        //    // 订阅输出和错误流
        //    pythonProcess.OutputDataReceived += (sender, e) =>
        //    {
        //        if (e.Data != null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Python Output: {e.Data}");
        //        }
        //    };

        //    pythonProcess.ErrorDataReceived += (sender, e) =>
        //    {
        //        if (e.Data != null)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"Python Error: {e.Data}");
        //        }
        //    };

        //    // 启动 Python 脚本并开始读取输出
        //    if (pythonProcess.Start())
        //    {
        //        pythonProcess.BeginOutputReadLine();
        //        pythonProcess.BeginErrorReadLine();
        //        await Task.Delay(1000); // 等待 Python 脚本初始化
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine("Failed to start Python script.");
        //    }
        //}

        // 从数据库获取驱动信息
        //private async Task<List<DriverInfo>> GetDriverInfoFromDatabaseAsync()
        //{

        //    driverInfoList.Clear(); // 清除上次检测的残留数据
        //    //List<DriverInfo> driverInfoList = new List<DriverInfo>();
        //    string query = "SELECT name, version, newversion, category,isButton FROM drivers";  // 根据实际情况修改 SQL 查询

        //    try
        //    {
        //        // 创建 MySQL 连接对象
        //        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        //        {
        //            await connection.OpenAsync();

        //            using (MySqlCommand command = new MySqlCommand(query, connection))
        //            {
        //                // 使用 DbDataReader 而非 MySqlDataReader
        //                using (DbDataReader reader = await command.ExecuteReaderAsync()) // 使用 DbDataReader
        //                {
        //                    while (await reader.ReadAsync())
        //                    {
        //                        // 创建 DriverInfo 对象并添加到列表中
        //                        driverInfoList.Add(new DriverInfo
        //                        {
        //                            Name = reader.GetString(0),
        //                            Version = reader.GetString(1),
        //                            NewVersion = reader.GetString(2),
        //                            Category = reader.GetString(3),
        //                            IsButton = reader.GetString(4)
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"数据库查询出错: {ex.Message}");
        //    }

        //    return driverInfoList;
        //}



    }





    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 根据 isButton 字符串的值 ("1" 或 "0") 返回不同的颜色
            if (value is string str)
            {
                if (str == "1")
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.LightGreen); // 如果是 "1"，返回绿色
                }
                else if (str == "0")
                {
                    return new SolidColorBrush(Microsoft.UI.Colors.LightGray); // 如果是 "0"，返回灰色
                }
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Gray); // 默认颜色（如果值不是 "1" 或 "0"）
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    // 数据模型
    public class DriverInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string NewVersion { get; set; }
        public string Category { get; set; }

        public string IsButton { get; set; }
    }




}
