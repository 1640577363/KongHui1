import json
import os
import pymysql
import wmi
from collections import defaultdict

# 创建 WMI 客户端对象
w = wmi.WMI()

# 查询所有已安装的驱动程序
drivers = w.Win32_PnPSignedDriver()

# 数据库连接配置
DB_CONFIG = {
    'host': '10.12.36.204',
    'user': 'root',
    'password': 'konghui@iuhgnok',
    'database': 'ry-vue',
    'port': 3306
}

# 驱动分类规则
CATEGORY_RULES = {
    "Audio": ["Audio", "Realtek"],
    "Network": ["Network", "Ethernet", "Wi-Fi"],
    "Display": ["Display", "Graphics"],
    "Storage": ["Storage", "Disk"]
}

# 分类和去重逻辑
driver_categories = defaultdict(lambda: {"drivers": [], "count": 0})
seen_drivers = {}

for driver in drivers:
    description = driver.Description if driver.Description else ""
    version = driver.DriverVersion if driver.DriverVersion else "未知"

    driver_key = (description, version)
    if driver_key not in seen_drivers:
        seen_drivers[driver_key] = True
        category = "Others"
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
def execute_batch_query(connection, query, data_list):
    """批量执行数据库操作"""
    try:
        with connection.cursor() as cursor:
            cursor.executemany(query, data_list)
        connection.commit()
    except Exception as e:
        print(f"批量执行数据库操作时出现错误: {e}")

# 连接到数据库
connection = pymysql.connect(**DB_CONFIG)

# 清空和重置数据库表
try:
    with connection.cursor() as cursor:
        cursor.execute("DELETE FROM drivers")
        cursor.execute("ALTER TABLE drivers AUTO_INCREMENT = 1")
    connection.commit()
    print("数据库已清空并重置 AUTO_INCREMENT。")
except Exception as e:
    print(f"清空数据库时出现错误: {e}")

# 查询最新驱动表
latest_driver_dict = {}
try:
    with connection.cursor() as cursor:
        cursor.execute("SELECT file_name, version, file_path FROM file")
        latest_driver_dict = {row[0]: {"version": row[1], "file_path": row[2]} for row in cursor.fetchall()}
    print("最新驱动信息已加载。")
except Exception as e:
    print(f"查询最新驱动表时出现错误: {e}")

# 数据分类
same_version_drivers = []
different_version_drivers = []

for category, data in driver_categories.items():
    for driver_info in data["drivers"]:
        name = driver_info["name"]
        version = driver_info["version"]
        device_ids = ",".join(driver_info["device_ids"])

        latest_version = latest_driver_dict.get(name, {}).get("version", "未知")
        qdwhere = latest_driver_dict.get(name, {}).get("qdwhere", "未知")

        driver_data = {
            "name": name,
            "version": version,
            "device_ids": device_ids,
            "newversion": latest_version,
            "qdwhere": qdwhere,
            "category": category
        }

        if latest_version == "未知" or version == latest_version:
            driver_data["isButton"] = 0
            same_version_drivers.append(driver_data)
        else:
            driver_data["isButton"] = 1
            different_version_drivers.append(driver_data)

# 准备批量插入数据
insert_query = """
    INSERT INTO drivers (name, version, device_ids, newversion, qdwhere, category, isButton)
    VALUES (%s, %s, %s, %s, %s, %s, %s)
    ON DUPLICATE KEY UPDATE 
        device_ids = VALUES(device_ids),
        newversion = VALUES(newversion),
        qdwhere = VALUES(qdwhere),
        category = VALUES(category),
        isButton = VALUES(isButton)
"""

same_version_data = [(d["name"], d["version"], d["device_ids"], d["newversion"], d["qdwhere"], d["category"], d["isButton"]) for d in same_version_drivers]
different_version_data = [(d["name"], d["version"], d["device_ids"], d["newversion"], d["qdwhere"], d["category"], d["isButton"]) for d in different_version_drivers]

# 批量插入
# 先插入不同版本的驱动
execute_batch_query(connection, insert_query, different_version_data)

# 再插入相同版本的驱动
execute_batch_query(connection, insert_query, same_version_data)


# 保存需要更新的驱动到文件
try:
    current_file_path = os.path.abspath(__file__)
    current_dir = os.path.dirname(current_file_path)
    os.chdir(current_dir)
    drivers_to_update = [{"name": d["name"], "newversion": d["newversion"]} for d in different_version_drivers]
    with open(os.path.join(current_dir, 'drivers_to_update.json'), 'w', encoding='utf-8') as f:
        json.dump(drivers_to_update, f, ensure_ascii=False, indent=4)
    print("需要更新的驱动程序信息已保存为 drivers_to_update.json")
except Exception as e:
    print(f"生成 JSON 文件时出现错误: {e}")

# 关闭数据库连接
connection.close()
