# import wmi
#
# # 获取音频驱动信息
# def get_audio_driver_info():
#     w = wmi.WMI()
#     for driver in w.Win32_PnPSignedDriver():
#         if "Realtek High Definition Audio" in driver.Description:
#             return {
#                 "name": driver.Description,
#                 "version": driver.DriverVersion,
#                 "device_id": driver.DeviceID
#             }
#     return None  # 如果没有找到匹配的音频驱动，返回 None
#
# # 调用函数并输出结果
# result = get_audio_driver_info()
# if result:
#     print(f"音频驱动程序名称: {result['name']}")
#     print(f"音频驱动程序版本: {result['version']}")
#     print(f"设备ID: {result['device_id']}")
# else:
#     print("未找到 Realtek 音频驱动程序。")


# import ctypes
# import sys
# import subprocess
# import time
# import pyautogui
#
#
# # 检查是否是管理员权限
# def is_admin():
#     try:
#         return ctypes.windll.shell32.IsUserAnAdmin() != 0
#     except Exception as e:
#         return False
#
#
# # 请求提升权限并运行程序
# def run_as_admin(installer_path):
#     if not is_admin():
#         # 请求提升权限
#         ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{sys.argv[0]}" "{installer_path}"', None,
#                                             1)
#         sys.exit(0)
#
#     # 如果已经是管理员权限，执行安装程序
#     try:
#         print(f"正在安装驱动程序: {installer_path}")
#         process = subprocess.Popen(installer_path, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
#
#         # 模拟按键或点击以自动化安装过程
#         time.sleep(2)  # 等待安装程序启动
#
#
#         # 自动点击"下一步"
#         # pyautogui.press('right')  # 模拟按键 "右箭头" (通常"下一步"按钮是右箭头或者回车)
#         # time.sleep(1)
#
#         # 模拟点击“下一步”或“接受协议”按钮
#         pyautogui.press('enter')  # 模拟按下“Enter”键
#         pyautogui.press('up')
#         pyautogui.press('enter')
#         time.sleep(5)  # 根据需要等待一段时间
#
#         # 继续按 "enter" 或 "next" 等按钮，直到安装结束
#         for _ in range(10):  # 试图按10次 "enter" 键
#             pyautogui.press('enter')
#             time.sleep(2)  # 等待几秒钟
#
#         # 等待安装完成
#         process.wait()
#
#     except subprocess.CalledProcessError as e:
#         print(f"安装过程中出现错误: {e}")
#         print(f"错误输出：{e.stderr.decode()}")
#
#
# # 安装程序路径
# installer_path = r"C:\Users\userLi\Downloads\sp153187.exe"
#
# # 以管理员权限执行安装
# run_as_admin(installer_path)




# import ctypes
# import sys
# import subprocess
# import time
#
# # 检查是否是管理员权限
# def is_admin():
#     try:
#         return ctypes.windll.shell32.IsUserAnAdmin() != 0
#     except Exception as e:
#         return False
#
# # 请求提升权限并运行程序
# def run_as_admin(installer_path):
#     if not is_admin():
#         # 请求提升权限
#         ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{sys.argv[0]}" "{installer_path}"', None, 1)
#         sys.exit(0)
#
#     # 如果已经是管理员权限，执行安装程序
#     try:
#         print(f"正在安装驱动程序: {installer_path}")
#         # 使用静默模式安装驱动
#         subprocess.run([installer_path, '/S'], check=True)
#         print("驱动程序安装完成！")
#     except subprocess.CalledProcessError as e:
#         print(f"安装过程中出现错误: {e}")
#         print(f"错误输出：{e.stderr.decode()}")
#
# # 安装程序路径
# installer_path = r"C:\Users\userLi\Downloads\sp153187.exe"
#
# # 以管理员权限执行安装
# run_as_admin(installer_path)



#
#
# import os
# import subprocess
# import sys
# import ctypes
#
# # 检查是否是管理员权限
# def is_admin():
#     try:
#         return ctypes.windll.shell32.IsUserAnAdmin() != 0
#     except Exception:
#         return False
#
# # 如果没有管理员权限，请求提升
# def run_as_admin(installer_path):
#     if not is_admin():
#         # 请求提升权限，执行当前脚本，并传递安装程序路径
#         ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{sys.argv[0]}" "{installer_path}"', None, 1)
#         sys.exit(0)
#
#     # 如果已经是管理员权限，执行安装程序并隐藏命令行窗口
#     try:
#         print(f"正在安装驱动程序: {installer_path}")
#         subprocess.Popen(
#             [installer_path, '/S'],  # 如果安装程序支持静默安装
#             stdout=subprocess.PIPE,
#             stderr=subprocess.PIPE,
#             creationflags=subprocess.CREATE_NO_WINDOW  # 隐藏命令行窗口
#         )
#         print("驱动程序正在安装...")
#
#     except Exception as e:
#         print(f"安装过程中出现错误: {e}")
#
# # 安装程序路径
# installer_path = r"C:\Users\userLi\Downloads\sp153187.exe"
#
# # 以管理员权限执行安装并隐藏窗口
# run_as_admin(installer_path)



# import subprocess
# import sys
# import ctypes
# import pymysql  # 用于连接 MySQL 数据库
#
# # 数据库连接配置
# DB_CONFIG = {
#     'host': 'localhost',         # 数据库地址
#     'user': 'root',              # 数据库用户名
#     'password': '123456',        # 数据库密码
#     'database': 'qd',            # 数据库名称
#     'port': 3306                 # MySQL 默认端口
# }
#
# # 检查是否是管理员权限
# def is_admin():
#     try:
#         return ctypes.windll.shell32.IsUserAnAdmin() != 0
#     except Exception:
#         return False
#
# # 获取驱动路径（根据名字查询）
# def get_driver_path(driver_name):
#     try:
#         connection = pymysql.connect(**DB_CONFIG)
#         cursor = connection.cursor()
#         query = "SELECT qdwhere FROM newdrivers WHERE name = %s"
#         print(f"执行的 SQL: {query}")  # 调试信息
#         print(f"参数: {driver_name}")  # 调试信息
#         cursor.execute(query, (driver_name,))
#         result = cursor.fetchone()
#         if result:
#             print(f"查询结果: {result}")  # 调试信息
#             return result[0]
#         else:
#             print(f"未找到名称为 {driver_name} 的驱动程序。")
#             sys.exit(1)
#     except Exception as e:
#         print(f"数据库查询错误: {e}")
#         sys.exit(1)
#     finally:
#         if connection:
#             connection.close()
#
# # 如果没有管理员权限，请求提升
# def run_as_admin(installer_path):
#     if not is_admin():
#         # 请求提升权限，执行当前脚本，并传递安装程序路径
#         ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{sys.argv[0]}" "{installer_path}"', None, 1)
#         sys.exit(0)
#
#     # 如果已经是管理员权限，执行安装程序并隐藏命令行窗口
#     try:
#         print(f"正在安装驱动程序: {installer_path}")
#         subprocess.Popen(
#             [installer_path, '/S'],  # 如果安装程序支持静默安装
#             stdout=subprocess.PIPE,
#             stderr=subprocess.PIPE,
#             creationflags=subprocess.CREATE_NO_WINDOW  # 隐藏命令行窗口
#         )
#         print("驱动程序正在安装...")
#
#     except Exception as e:
#         print(f"安装过程中出现错误: {e}")
#
# # 主函数
# if __name__ == "__main__":
#     if len(sys.argv) > 1:
#         # 如果从提升权限后返回，参数将传递为路径
#         installer_path = sys.argv[1]
#     else:
#         # 正常运行时从数据库获取驱动路径
#         driver_name = input("请输入驱动名称: ")  # 根据名称查询
#         installer_path = get_driver_path(driver_name)
#
#     # 以管理员权限执行安装并隐藏窗口
#     run_as_admin(installer_path)


# import subprocess
# import sys
# import ctypes
# import pymysql  # 用于连接 MySQL 数据库
#
# # 数据库连接配置
# DB_CONFIG = {
#     'host': 'localhost',  # 数据库地址
#     'user': 'root',  # 数据库用户名
#     'password': '123456',  # 数据库密码
#     'database': 'qd',  # 数据库名称
#     'port': 3306  # MySQL 默认端口
# }
#
#
# # 获取驱动路径（根据名字查询）
# def get_driver_path(driver_name):
#     try:
#         connection = pymysql.connect(**DB_CONFIG)
#         cursor = connection.cursor()
#         query = "SELECT qdwhere FROM newdrivers WHERE name = %s"
#         print(f"执行的 SQL: {query}")  # 调试信息
#         print(f"参数: {driver_name}")  # 调试信息
#         cursor.execute(query, (driver_name,))
#         result = cursor.fetchone()
#         if result:
#             print(f"查询结果: {result}")  # 调试信息
#             return result[0]
#         else:
#             print(f"未找到名称为 {driver_name} 的驱动程序。")
#             sys.exit(1)
#     except Exception as e:
#         print(f"数据库查询错误: {e}")
#         sys.exit(1)
#     finally:
#         if connection:
#             connection.close()
#
#
# # 如果没有管理员权限，直接请求提升权限并执行安装程序
# def run_as_admin(installer_path):
#     # 请求提升权限，执行安装程序并隐藏命令行窗口
#     try:
#         print(f"正在安装驱动程序: {installer_path}")
#         subprocess.Popen(
#             [installer_path, '/S'],  # 如果安装程序支持静默安装
#             stdout=subprocess.PIPE,
#             stderr=subprocess.PIPE,
#             creationflags=subprocess.CREATE_NO_WINDOW  # 隐藏命令行窗口
#         )
#         print("驱动程序正在安装...")
#
#     except Exception as e:
#         print(f"安装过程中出现错误: {e}")
#
#
# # 主函数
# def main(driver_name):
#     # 获取驱动安装路径
#     installer_path = get_driver_path(driver_name)
#
#     # 使用管理员权限执行安装程序
#     if not ctypes.windll.shell32.IsUserAnAdmin():
#         print("当前没有管理员权限，正在请求提升权限...")
#         # 使用 ShellExecute 请求提升权限并执行安装
#         ctypes.windll.shell32.ShellExecuteW(None, "runas", sys.executable, f'"{sys.argv[0]}" "{installer_path}"', None,
#                                             1)
#         sys.exit(0)
#
#     # 如果已经是管理员权限，直接执行安装程序
#     run_as_admin(installer_path)
#
#
# if __name__ == "__main__":
#     if len(sys.argv) > 1:
#         # 如果从提升权限后返回，参数将传递为路径
#         driver_name = sys.argv[1]
#     else:
#         # 正常运行时根据用户输入获取驱动名称
#         driver_name = input("请输入驱动名称: ")
#
#     # 执行主函数
#     main(driver_name)


import subprocess
import sys
import pymysql  # 用于连接 MySQL 数据库

# 数据库连接配置
DB_CONFIG = {
    'host': '10.12.36.204',  # 数据库地址
    'user': 'root',  # 数据库用户名
    'password': 'konghui@iuhgnok',  # 数据库密码
    'database': 'ry-vue',  # 数据库名称
    'port': 3306  # MySQL 默认端口
}

# 获取驱动路径（根据名字查询）
def get_driver_path(driver_name):
    try:
        # 连接数据库
        connection = pymysql.connect(**DB_CONFIG)
        cursor = connection.cursor()

        # 查询SQL，根据驱动名称查找安装路径
        query = "SELECT file_path FROM file WHERE name = %s"
        cursor.execute(query, (driver_name,))

        result = cursor.fetchone()
        if result:
            driver_path = result[0]
            print(f"驱动程序路径: {driver_path}")  # 调试信息
            return driver_path
        else:
            print(f"未找到名称为 {driver_name} 的驱动程序。")
            sys.exit(1)
    except Exception as e:
        print(f"数据库查询错误: {e}")
        sys.exit(1)
    finally:
        if connection:
            connection.close()


# 执行安装操作（不需要管理员权限）
def run_installer(installer_path):
    try:
        print(f"正在安装驱动程序: {installer_path}")
        subprocess.run([installer_path, '/S'], shell=True)  # 执行安装命令，支持静默安装
        print("驱动程序正在安装...")
    except Exception as e:
        print(f"安装过程中出现错误: {e}")


# 主函数
if __name__ == "__main__":
    # 检查是否传递了驱动名称作为命令行参数
    if len(sys.argv) > 1:
        driver_name = sys.argv[1]  # 获取命令行参数中的驱动名称
        print(f"查询驱动名称: {driver_name}")

        # 从数据库获取对应的安装路径
        installer_path = get_driver_path(driver_name)

        # 执行安装操作
        run_installer(installer_path)
    else:
        print("没有提供驱动名称作为参数，请输入驱动名称。")
        sys.exit(1)






