import psutil
import cpuinfo
import time
from datetime import timedelta
import wmi
import json
import os


def format_uptime(uptime_seconds):
    uptime_delta = timedelta(seconds=uptime_seconds)
    days = uptime_delta.days
    hours, remainder = divmod(uptime_delta.seconds, 3600)
    minutes, seconds = divmod(remainder, 60)
    return f"{days}:{hours}:{minutes}:{seconds}"


def get_cpu_info():
    # 获取 CPU 的基本信息
    cpu_info = cpuinfo.get_cpu_info()
    cpu_brand = cpu_info.get('brand_raw', '未知')
    cpu_arch = cpu_info.get('arch', '未知')
    cpu_speed = cpu_info.get('hz_advertised_friendly', '未知')
    cpu_physical_cores = psutil.cpu_count(logical=False)
    cpu_logical_processors = psutil.cpu_count(logical=True)

    # 缓存信息，初始为 B，将其转换为 MB
    cpu_l1_cache = cpu_info.get('l1_data_cache_size', 0)
    cpu_l2_cache = cpu_info.get('l2_cache_size', 0)
    cpu_l3_cache = cpu_info.get('l3_cache_size', 0)

    # 单位转换，B 转为 MB，结果保留两位小数
    cpu_l1_cache = f"{float(cpu_l1_cache) / (1024 * 1024):.2f} MB" if cpu_l1_cache else '未知'
    cpu_l2_cache = f"{float(cpu_l2_cache) / (1024 * 1024):.2f} MB" if cpu_l2_cache else '未知'
    cpu_l3_cache = f"{float(cpu_l3_cache) / (1024 * 1024):.2f} MB" if cpu_l3_cache else '未知'

    # CPU 使用率加上单位
    cpu_percent = f"{psutil.cpu_percent(interval=1)}%"

    boot_time = psutil.boot_time()
    uptime_seconds = time.time() - boot_time
    uptime = format_uptime(uptime_seconds)

    total_processes = len(psutil.pids())
    total_threads = sum(proc.info['num_threads'] for proc in psutil.process_iter(attrs=['num_threads']))

    # 获取总句柄数
    try:
        c = wmi.WMI()
        total_handles = sum(proc.HandleCount for proc in c.Win32_Process())
    except Exception as e:
        total_handles = "未知"

    script_directory = os.path.dirname(os.path.abspath(__file__))
    json_path = os.path.join(script_directory, "cpu_usage_info.json")
    
    with open(json_path, "w") as json_file:
        json.dump({
            "cpu_percent": cpu_percent,
            "cpu_speed": cpu_speed,
            "total_processes": total_processes,
            "total_threads": total_threads,
            "total_handles": total_handles,
            "uptime": uptime,
            "cpu_l1_cache": cpu_l1_cache,
            "cpu_l2_cache": cpu_l2_cache,
            "cpu_l3_cache": cpu_l3_cache
        }, json_file)


if __name__ == "__main__":
    while True:
        get_cpu_info()
        time.sleep(1)
