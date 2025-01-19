import psutil
import platform
import time
from datetime import timedelta
import wmi
import json
import os


DISK_TYPE_MAPPING = {
    "Fixed hard disk media": "硬盘 (HDD)",
    "Removable media": "可移动媒体",
    "External hard disk media": "外部硬盘",
    "Solid state disk": "固态硬盘 (SSD)"
}

def get_disk_info():
    disk_info_list = []
    partitions = psutil.disk_partitions(all=False)
    disk_io_start = psutil.disk_io_counters(perdisk=True)
    time.sleep(1)
    disk_io_end = psutil.disk_io_counters(perdisk=True)

    # 获取系统盘路径
    system_drive = "C:\\" if platform.system() == "Windows" else "/"

    for partition in partitions:
        try:
            usage = psutil.disk_usage(partition.mountpoint)
            total_size = f"{usage.total / (1024 ** 3):.2f} GB"
            used_size = f"{usage.used / (1024 ** 3):.2f} GB"
            free_size = f"{usage.free / (1024 ** 3):.2f} GB"
            usage_percent = f"{usage.percent:.2f} %"

            device_name = partition.device.split("\\")[-1]
            read_speed = write_speed = "未知"

            for dev_name, io_start in disk_io_start.items():
                if device_name in dev_name:
                    io_end = disk_io_end[dev_name]
                    read_speed = f"{(io_end.read_bytes - io_start.read_bytes) / 1024:.2f} KB/s"
                    write_speed = f"{(io_end.write_bytes - io_start.write_bytes) / 1024:.2f} KB/s"
                    break

            model = "未知"
            disk_type = "未知"

            if platform.system() == "Windows":
                w = wmi.WMI()
                for disk in w.Win32_DiskDrive():
                    if disk.DeviceID.endswith(partition.device.split('\\')[-1]):
                        model = disk.Model.strip()
                        disk_type = DISK_TYPE_MAPPING.get(disk.MediaType, "未知")
                        break

            # 判断是否为系统磁盘
            is_system_disk = partition.mountpoint == system_drive
            temp = "是" if is_system_disk else "否"

            # 添加单个分区的 IO 总量信息
            partition_io = psutil.disk_io_counters(perdisk=True).get(device_name, None)
            for dev_name, io_start in disk_io_start.items():
                # 尝试匹配设备名
                if device_name in dev_name or partition.device in dev_name:
                    io_end = disk_io_end[dev_name]
                    total_read = f"{io_end.read_bytes / (1024 ** 3):.2f} GB"
                    total_write = f"{io_end.write_bytes / (1024 ** 3):.2f} GB"
                    break
                else:
                    total_read = "未知"
                    total_write = "未知"

            # print(total_write)
            # 组装每个分区的信息
            disk_info_list.append({
                "mountpoint": str(partition.mountpoint),
                "total_size": total_size,
                "used_size": used_size,
                "free_size": free_size,
                "usage_percent": usage_percent,
                "read_speed": read_speed,
                "write_speed": write_speed,
                "model": model,
                "disk_type": disk_type,
                "isSystemDisk": temp,
                "total_read": total_read,
                "total_write": total_write
            })
        except Exception as e:
            print(f"无法获取分区 {partition.device} 的信息: {e}")

    # 保存到 JSON 文件
    script_directory = os.path.dirname(os.path.abspath(__file__))
    json_path = os.path.join(script_directory, "disk_info.json")
    with open(json_path, "w", encoding="utf-8") as json_file:
        json.dump(disk_info_list, json_file, ensure_ascii=False, indent=4)

if __name__ == "__main__":
    get_disk_info()
