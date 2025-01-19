import json
import pymysql
import wmi
from collections import defaultdict
from datetime import datetime  # 导入 datetime 模块

# 创建 WMI 客户端对象
w = wmi.WMI()

# 查询所有已安装的驱动程序
drivers = w.Win32_PnPSignedDriver()

# 数据库连接配置
DB_CONFIG = {
    'host': '10.12.36.204',  # 远程数据库地址
    'user': 'root',
    'password': 'konghui@iuhgnok',
    'database': 'ry-vue',
    'port': 3306
}

# 驱动分类规则
CATEGORY_RULES = {
    "音频驱动": ["Audio", "Realtek", "Sound", "Speaker"],
    "网络驱动": ["Network", "Ethernet", "Wi-Fi", "LAN", "Broadcom", "Intel"],
    "显示驱动": ["Display", "Graphics", "NVIDIA", "AMD", "Intel HD Graphics", "GPU", "Monitor"],
    "存储驱动": ["Storage", "Disk", "SATA", "SSD", "HDD", "RAID", "Storage Controller"],
    "USB驱动": ["USB", "Universal Serial Bus", "Controller", "USB Hub", "USB Device"],
    "打印机驱动": ["Printer", "HP", "Canon", "Epson", "Brother"],
    "摄像头驱动": ["Camera", "Webcam", "Video", "Microsoft", "Intel", "Logitech"],
    "蓝牙驱动": ["Bluetooth", "Bluetooth Device", "BT", "Wireless","Qualcom"],
    "硬盘驱动": ["Hard Disk", "Disk Drive", "SATA", "NVMe", "Storage"],
    "HID 设备驱动": ["HID-compliant", "HID Keyboard Device", "HID-compliant game controller",
                     "Microsoft Input Configuration Device"]
}

# 特殊系统管理组件，避免分类为 "其他驱动"
SYSTEM_MANAGEMENT_DRIVERS = [
    "Composite Bus Enumerator",
    "Volume",
    "Generic volume shadow copy",
    "Volume Manager",
    "通信端口",
    "PCI Express Root Complex",
    "UMBus Root Bus Enumerator"
]

# 分类和去重逻辑
driver_categories = defaultdict(lambda: {"drivers": [], "count": 0})
seen_drivers = {}

for driver in drivers:
    description = driver.Description if driver.Description else ""
    version = driver.DriverVersion if driver.DriverVersion else "未知"

    driver_key = (description, version)
    if driver_key not in seen_drivers:
        seen_drivers[driver_key] = True
        category = "其他驱动"  # 默认类别是“其他驱动”

        # 如果是系统管理组件或硬件组件，跳过分类
        if any(system_component in description for system_component in SYSTEM_MANAGEMENT_DRIVERS):
            category = "系统硬件组件"

        # 根据分类规则查找匹配的类别
        for cat, keywords in CATEGORY_RULES.items():
            if any(keyword in description for keyword in keywords):
                category = cat
                break

        driver_info = {
            "name": description,
            "version": version,
            "device_ids": [driver.DeviceID],
            "category": category
        }
        driver_categories[category]["drivers"].append(driver_info)
        driver_categories[category]["count"] += 1
    else:
        # 添加设备ID到已有的驱动
        for category_data in driver_categories.values():
            for driver_info in category_data["drivers"]:
                if driver_info["name"] == description and driver_info["version"] == version:
                    driver_info["device_ids"].append(driver.DeviceID)
                    break


# 批量处理数据库操作
def execute_query(connection, query, params):
    """执行数据库查询"""
    try:
        with connection.cursor() as cursor:
            cursor.execute(query, params)
        return cursor.fetchall()
    except Exception as e:
        print(f"数据库操作时出现错误: {e}")
        return []


# 连接到数据库
connection = pymysql.connect(**DB_CONFIG)

# 查询最新驱动表
latest_driver_dict = {}
try:
    with connection.cursor() as cursor:
        cursor.execute("SELECT file_name, version FROM file")
        latest_driver_dict = {row[0].strip(): row[1].strip() for row in cursor.fetchall()}
    print("最新驱动信息已加载。")
except Exception as e:
    print(f"查询最新驱动表时出现错误: {e}")

# 数据分类
isButton_1_drivers = []  # 存放 isButton 为 1 的驱动
isButton_0_drivers = []  # 存放 isButton 为 0 的驱动

for category, data in driver_categories.items():
    for driver_info in data["drivers"]:
        name = driver_info["name"].strip()  # 去除名称中的空格
        version = driver_info["version"].strip()  # 去除版本号中的空格
        device_ids = ",".join(driver_info["device_ids"])

        # 在数据库中查询该驱动名称的版本号
        latest_version = latest_driver_dict.get(name, version)  # 如果在file表中找不到对应记录，newversion与version相同

        # 获取当前时间并格式化
        current_time = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        driver_data = {
            "name": name,
            "version": version,
            "device_ids": device_ids,
            "newversion": latest_version,
            "category": category,
            "update_time": current_time  # 添加更新时间
        }

        if latest_version == version:
            driver_data["isButton"] = 0  # 相同版本
            isButton_0_drivers.append(driver_data)  # 放入 isButton 为 0 的列表
        else:
            driver_data["isButton"] = 1  # 不同版本
            isButton_1_drivers.append(driver_data)  # 放入 isButton 为 1 的列表

# 合并 isButton 为 1 和 0 的驱动，isButton 为 1 的在前
all_drivers = []


# 排序函数：isButton 为 1 的在前，并确保类别为“其他驱动”的排在最后
def sort_drivers(driver):
    # 排序条件：isButton 为 1 的排前面，类别为"其他驱动"的排后面
    is_button_priority = 0 if driver["isButton"] == 1 else 1
    is_other_priority = 1 if driver["category"] == "其他驱动" else 0
    return (is_button_priority, is_other_priority)


# 对 isButton 为 1 和 isButton 为 0 的驱动分别排序
isButton_1_drivers_sorted = sorted(isButton_1_drivers, key=sort_drivers)
isButton_0_drivers_sorted = sorted(isButton_0_drivers, key=sort_drivers)

# 合并排序后的驱动
all_drivers.extend(isButton_1_drivers_sorted)
all_drivers.extend(isButton_0_drivers_sorted)

# 准备保存为 JSON 文件
try:
    with open(r'drivers_info.json', 'w', encoding='utf-8') as f:
        json.dump(all_drivers, f, ensure_ascii=False, indent=4)
    print("驱动程序信息已保存为 drivers_info.json")
except Exception as e:
    print(f"生成 JSON 文件时出现错误: {e}")

# 只保存 isButton 为 1 的驱动
drivers_to_update = [driver for driver in all_drivers if driver["isButton"] == 1]

# 保存为 drivers_to_update.json
try:
    with open(r'drivers_to_update.json', 'w', encoding='utf-8') as f:
        json.dump(drivers_to_update, f, ensure_ascii=False, indent=4)
    print("需要更新的驱动程序已保存为 drivers_to_update.json")
except Exception as e:
    print(f"生成更新驱动 JSON 文件时出现错误: {e}")

# 关闭数据库连接
connection.close()

# 打印分类结果
for category, data in driver_categories.items():
    print(f"{category}: {data['count']} 个驱动")
