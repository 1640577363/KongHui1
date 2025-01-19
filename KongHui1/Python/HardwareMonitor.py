import subprocess
import requests
import os
import time
import json
import psutil
import asyncio
import websockets

def is_process_running(process_name):

    for proc in psutil.process_iter(['name']):
        if proc.info['name'] == process_name:
            return True
    return False

def start_open_hardware_monitor():

    current_file_path = os.path.abspath(__file__)
    current_dir = os.path.dirname(current_file_path)
    os.chdir(current_dir)
    print(f"当前目录: {current_dir}")
    open_hw_monitor_path = os.path.join('OpenHardwareMonitor', 'OpenHardwareMonitor.exe')
    print (f"当前目录: {open_hw_monitor_path}")
    process_name = "OpenHardwareMonitor.exe"

    if is_process_running(process_name):
        print(f"{process_name} 已经运行，无需重新启动。")
    else:
        try:
            subprocess.Popen(open_hw_monitor_path, shell=True)
            print(f"{process_name} 已启动。")
        except Exception as e:
            print(f"启动 {process_name} 失败: {e}")

def fetch_ohm_data():

    url = "http://localhost:8085/data.json"  # OpenHardwareMonitor 默认地址
    try:
        response = requests.get(url)
        response.raise_for_status()  # 检查 HTTP 请求是否成功
        return response.json()  # 返回 JSON 数据
    except requests.RequestException as e:
        print(f"无法连接 OpenHardwareMonitor: {e}")
        return None

def extract_metrics(json_data):
    metrics = {
        "cpu_temp": None,
        "cpu_usage": None,
        "gpu_temp": None,
        "hdd_temp": None,
        "fan_speed": None,
        "gpu_usage": None,
        "memory_usage": None,
        "hdd_usage": None
    }

    target_keys = {
        "cpu total": "cpu_usage",
        "CPU Core #2": "cpu_temp",
        "gpu core": "gpu_temp",
        "fan #2": "fan_speed",
        "temperature #2": "hdd_temp",
        "gpu memory": "gpu_usage",
        "memory": "memory_usage",
        "used space": "hdd_usage"
    }

    def recursive_search(data, target_keys):
        if isinstance(data, dict):
            # Match specific metrics based on 'Text' key
            text = data.get("Text", "").lower()
            value = data.get("Value", None)

            # Skip invalid values
            if value == "-":
                return  # Skip invalid value
            
            # Handle valid numeric values
            if text in target_keys and value is not None:
                try:
                    metrics[target_keys[text]] = float(value.split(" ")[0])  # Convert to float, strip units if present
                except ValueError:
                    print(f"无法将值 '{value}' 转换为数字，跳过此项")  # 如果值无法转换为数字，输出错误并跳过

            # Recurse into children if available
            for child in data.get("Children", []):
                recursive_search(child, target_keys)
        elif isinstance(data, list):
            for item in data:
                recursive_search(item, target_keys)

    

    recursive_search(json_data, target_keys)
    return metrics

     
def calculate_average(metrics_list):

    avg_metrics = {key: 0 for key in metrics_list[0].keys()}

    for metrics in metrics_list:
        for key, value in metrics.items():
            if value is not None:
                avg_metrics[key] += value

    for key in avg_metrics:
        avg_metrics[key] = round(avg_metrics[key] / len(metrics_list), 1)

    print("平均值计算结果:", avg_metrics)
    return avg_metrics

if __name__ == "__main__":
    start_open_hardware_monitor()
    time.sleep(2)  # 等待 OpenHardwareMonitor 启动

    sampling_duration = 5
    sampling_interval = 1
    metrics_samples = []

    for _ in range(sampling_duration // sampling_interval):
        data = fetch_ohm_data()
        if data:
            metrics = extract_metrics(data)
            metrics_samples.append(metrics)
        time.sleep(sampling_interval)

    avg_metrics = calculate_average(metrics_samples)



