import multiprocessing
import time
import subprocess
import requests
import os
import psutil

def is_process_running(process_name):
    for proc in psutil.process_iter(['name']):
        if proc.info['name'] == process_name:
            return True
    return False


def start_open_hardware_monitor():
    current_file_path = os.path.abspath(__file__)
    current_dir = os.path.dirname(current_file_path)
    os.chdir(current_dir)
    
    open_hw_monitor_path = os.path.join('OpenHardwareMonitor', 'OpenHardwareMonitor.exe')
    process_name = "OpenHardwareMonitor.exe"

    if is_process_running(process_name):
        print(f"{process_name} already running.")
    else:
        try:
            subprocess.Popen(open_hw_monitor_path, shell=True)
            print(f"{process_name} is running.")
        except Exception as e:
            print(f" {process_name} restared failed: {e}")


def fetch_ohm_data():
    url = "http://localhost:8085/data.json"  
    try:
        response = requests.get(url)
        response.raise_for_status() 
        return response.json()
    except requests.RequestException as e:
        print(f"Could not fetch data from OpenHardwareMonitor: {e}")
        return None


def extract_metrics(json_data):
    metrics = {
        "cpu_temp": None,
        "cpu_usage": None
    }

    def recursive_search(data, target_keys):
        if isinstance(data, dict):
            text = data.get("Text", "").lower()
            value = data.get("Value", None)

            if value == "-":
                return 
            
            if text in target_keys and value is not None:
                try:
                    metrics[target_keys[text]] = float(value.split(" ")[0]) 
                except ValueError:
                    pass

            for child in data.get("Children", []):
                recursive_search(child, target_keys)
        elif isinstance(data, list):
            for item in data:
                recursive_search(item, target_keys)

    target_keys = {
        "cpu total": "cpu_usage",
        "temperature #1": "cpu_temp"
    }

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

    print("Average metrics:", avg_metrics)
    return avg_metrics


def stress_task(duration):
    end_time = time.time() + duration
    while time.time() < end_time:
        sum(i * i for i in range(10000))


def cpu_stresser(duration):
    processes = []
    metrics_list = []

    for _ in range(multiprocessing.cpu_count()):
        p = multiprocessing.Process(target=stress_task, args=(duration,))
        p.start()
        processes.append(p)

    start_time = time.time()

    # 延迟 1 秒后开始采集数据
    time.sleep(1)

    while time.time() - start_time < duration:
        data = fetch_ohm_data()
        if data:
            metrics = extract_metrics(data)
            metrics_list.append(metrics)
        time.sleep(1)

    for p in processes:
        p.join()

    # 获取平均值
    avg_metrics = calculate_average(metrics_list)
    
    # 根据平均温度判断 CPU 性能
    cpu_temp = avg_metrics["cpu_temp"]
    cpu_usage = avg_metrics["cpu_usage"]

    # 判断温度
    if cpu_temp is not None:
        if cpu_temp < 60:
            temp_performance = "Excellent"
        elif 60 <= cpu_temp < 80:
            temp_performance = "Good"
        elif 80 <= cpu_temp < 90:
            temp_performance = "Fair"
        else:
            temp_performance = "Poor"
        print(f"CPU Tempture Performance: {temp_performance}")

    # 判断使用率
    if cpu_usage is not None:
        if cpu_usage < 70:
            usage_performance = "Poor"
        elif 70 <= cpu_usage < 90:
            usage_performance = "Fair"
        else:
            usage_performance = "Excellent"

        print(f"CPU Usage Performance: {usage_performance}")

    return avg_metrics



if __name__ == "__main__":
    start_open_hardware_monitor()
    time.sleep(2)  
    duration = 11
    cpu_stresser(duration)
