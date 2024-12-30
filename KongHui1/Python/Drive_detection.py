import subprocess

def check_drivers():
    # 定义要检查的硬件类型及其中文名称
    hardware_types = {
        "cpu": "CPU驱动",
        "display": "显卡驱动",
        "network": "网络适配器驱动",
        "sound": "声卡驱动",
        "disk": "存储设备驱动",
        "bluetooth": "蓝牙驱动",
        "usb": "USB驱动"
    }
    
    # 定义一个字典来存储硬件类型和对应的WMIC查询命令
    wmic_queries = {
        "cpu": ["cpu", "get", "name"],
        "display": ["path", "win32_videocontroller", "get", "name"],
        "network": ["path", "win32_networkadapterconfiguration", "where", "IPEnabled='TRUE'", "get", "name"],
        "sound": ["path", "win32_sounddevice", "get", "name"],
        "disk": ["path", "win32_diskdrive", "get", "name"],
        "bluetooth": ["path", "win32_pnpentity", "where", "service='bthport'", "get", "name"],
        "usb": ["path", "win32_usbhub", "get", "name"]
    }
    
    # 检查每个硬件的驱动
    for hardware, name in hardware_types.items():
        try:
            # 执行WMIC命令
            command = ["wmic"] + wmic_queries[hardware]
            result = subprocess.run(command, capture_output=True, text=True, check=True)
            # 输出结果
            if result.stdout:
                print(f"{name}已安装。")
            else:
                print(f"{name}检查失败，可能未安装或查询出错。")
        except subprocess.CalledProcessError as e:
            print(f"检查{name}时发生错误：{e} 返回码 {e.returncode}")

if __name__ == "__main__":
    check_drivers()
