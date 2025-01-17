import random
import time
import multiprocessing
import time
import subprocess
import requests
import os
import psutil
import json

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
        "fan_speed": None
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
        "cpu package": "cpu_temp",
        "fan #2": "fan_speed"
    }

    recursive_search(json_data, target_keys)

    if metrics["cpu_temp"] is None:
        metrics["cpu_temp"] = 0.0
    else:
        metrics["cpu_temp"] = float(metrics["cpu_temp"])

    if metrics["fan_speed"] is None:
        metrics["fan_speed"] = 0.0
    else:
        metrics["fan_speed"] = float(metrics["fan_speed"])
   
    return metrics

def stress_task(memory_size_bytes):
    stress_data = [random.random() for _ in range(memory_size_bytes // 8)]  
    for _ in range(100000):
        stress_data[random.randint(0, len(stress_data) - 1)] = random.random()
        stress_data[random.randint(0, len(stress_data) - 1)] += 1
        stress_data[random.randint(0, len(stress_data) - 1)] *= 2

    def cpu_stress():
        count = 0
        while True:
            for num in range(100000):
                if all(num % i != 0 for i in range(2, int(num ** 0.5) + 1)):
                    count += 1
            if count >= 1000:
                break

    cpu_process = multiprocessing.Process(target=cpu_stress)
    cpu_process.start()
    time.sleep(0.1)
    cpu_process.terminate()
    cpu_process.join()
    time.sleep(0.1)

def cpu_memory_stresser(duration):
    memory_info = psutil.virtual_memory()
    memory_size_bytes = memory_info.total
    memory_metrics_list = []
    cpu_metrics_list = []
    metrics_samples = []

    processes = []
    for _ in range(multiprocessing.cpu_count()):
        p = multiprocessing.Process(target=stress_task, args=(memory_size_bytes,))
        p.start()
        processes.append(p)

    time.sleep(1)

    start_time = time.time()

    data_dict = {
        "hardwareId": "",
        "company": "LANY",
        "temperature":
        {
            "cpu": [],
            "gpu": [],
            "disk": []
        },
        "usageRate":
        {
            "cpu": [],
            "gpu": [],
            "disk": []
        },
        "fanSpeed": [],
        "power": []
    }
    last_time_recorded = start_time
    while time.time() - start_time < duration:

        data = fetch_ohm_data()
        if data:
            metrics = extract_metrics(data)
            metrics_samples.append(metrics)

        memory_usage = psutil.virtual_memory().percent
        cpu_usage = psutil.cpu_percent(interval=1)
        memory_metrics_list.append(memory_usage)
        cpu_metrics_list.append(cpu_usage)

        if time.time() - last_time_recorded >= 10:
            avg_memory_usage = sum(memory_metrics_list) / len(memory_metrics_list) if memory_metrics_list else 0
            avg_cpu_usage = sum(cpu_metrics_list) / len(cpu_metrics_list) if cpu_metrics_list else 0
            
            avg_cpu_temp = [m["cpu_temp"] for m in metrics_samples if "cpu_temp" in m and m["cpu_temp"] != 0]
            if avg_cpu_temp:
                avg_cpu_temp = sum(avg_cpu_temp) / len(avg_cpu_temp)
            else:
                avg_cpu_temp = 0.0

            
            avg_fun_speed = [m["fan_speed"] for m in metrics_samples if "fan_speed" in m and m["fan_speed"] != 0]
            if avg_fun_speed:
                avg_fun_speed = sum(avg_fun_speed) / len(avg_fun_speed)
            else:
                avg_fun_speed = 0.0

            
            avg_memory_usage = round(avg_memory_usage, 1)
            avg_cpu_usage = round(avg_cpu_usage, 1)
            avg_cpu_temp = round(avg_cpu_temp, 1)
            avg_fun_speed = round(avg_fun_speed, 1)

            memory_metrics_list.clear()
            cpu_metrics_list.clear()
            metrics_samples.clear()

            
            data_dict["temperature"]["cpu"].append(avg_cpu_temp)
            data_dict["usageRate"]["cpu"].append(avg_cpu_usage)
            data_dict["fanSpeed"].append(avg_fun_speed)
            data_dict["usageRate"]["disk"].append(avg_memory_usage)

            last_time_recorded = time.time()

        time.sleep(1)

    for p in processes:
        p.terminate()
        p.join()

    avg_memory_usage = sum(data_dict["usageRate"]["disk"]) / len(data_dict["usageRate"]["disk"]) if data_dict["usageRate"]["disk"] else 0
    avg_cpu_usage = sum(data_dict["usageRate"]["cpu"]) / len(data_dict["usageRate"]["cpu"]) if data_dict["usageRate"]["cpu"] else 0
    avg_cpu_temp = sum(data_dict["temperature"]["cpu"]) / len(data_dict["temperature"]["cpu"]) if data_dict["temperature"]["cpu"] else 0
    avg_fun_speed = sum(data_dict["fanSpeed"]) / len(data_dict["fanSpeed"]) if data_dict["fanSpeed"] else 0
    avg_memory_usage = round(avg_memory_usage, 1)
    avg_cpu_usage = round(avg_cpu_usage, 1)
    avg_cpu_temp = round(avg_cpu_temp, 1)
    avg_fun_speed = round(avg_fun_speed, 1)

    hardware_id = get_hard_drive_id()
    data_dict["hardwareId"] = hardware_id

    current_file_path = os.path.abspath(__file__)
    current_dir = os.path.dirname(current_file_path)
    for key, value in data_dict.items():
        data_dict[key] = json.dumps(value)
    print(f"Data saved to {current_dir}/data.json")
    with open('data.json', 'w') as f:
        json.dump(data_dict, f, indent=4)

    print(data_dict)
    print(f"cpu_usage:{avg_cpu_usage}")
    print(f"cpu_temp:{avg_cpu_temp}")
    print(f"fan_speed:{avg_fun_speed}")
    print(f"memory_usage:{avg_memory_usage}")

    return data_dict

def get_hard_drive_id():
    try:
        result = subprocess.check_output("wmic diskdrive get SerialNumber", shell=True)
        result = result.decode().split('\n')
        result = [x.strip() for x in result if x.strip() and 'SerialNumber' not in x]
        if result and len(result) > 1:
            res= result[1]
            res= res[:-1]
            return res
        else:
            return "No second hard drive ID found"
    except subprocess.CalledProcessError as e:
        return f"Error occurred: {e}"

if __name__ == "__main__":
    start_open_hardware_monitor()
    time.sleep(2)
    duration = 30
    cpu_memory_stresser(duration)
