from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *
import random
import time
import multiprocessing
import time
import subprocess
import requests
import os
import psutil
sys.stdout = open(os.devnull, 'w')
import pygame
sys.stdout = sys.__stdout__

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

    
    time.sleep(2)

    while time.time() - start_time < duration:
        data = fetch_ohm_data()
        if data:
            metrics = extract_metrics(data)
            metrics_list.append(metrics)
        time.sleep(1)

    for p in processes:
        p.join()

    avg_metrics = calculate_average(metrics_list)
    
    cpu_temp = avg_metrics["cpu_temp"]
    cpu_usage = avg_metrics["cpu_usage"]

    
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

    if cpu_usage is not None:
        if cpu_usage < 70:
            usage_performance = "Poor"
        elif 70 <= cpu_usage < 90:
            usage_performance = "Fair"
        else:
            usage_performance = "Excellent"

        print(f"CPU Usage Performance: {usage_performance}")

    return avg_metrics

def stress_task(memory_size_bytes):
    stress_data = [random.random() for _ in range(memory_size_bytes // 8)]  
    for _ in range(100000):
        stress_data[random.randint(0, len(stress_data) - 1)] = random.random()
        stress_data[random.randint(0, len(stress_data) - 1)] += 1
        stress_data[random.randint(0, len(stress_data) - 1)] *= 2

    time.sleep(0.1)

def memory_stresser(duration):
    memory_info = psutil.virtual_memory()
    memory_size_bytes = memory_info.total
    metrics_list = []

    processes = []
    for _ in range(multiprocessing.cpu_count()):
        p = multiprocessing.Process(target=stress_task, args=(memory_size_bytes,))
        p.start()
        processes.append(p)


    start_time = time.time()
    time.sleep(7)
    while time.time() - start_time < duration:
        memory_usage = psutil.virtual_memory().percent  
        metrics_list.append(memory_usage)
        time.sleep(1)

    for p in processes:
        p.terminate()
        p.join()

    avg_memory_usage = sum(metrics_list) / len(metrics_list)
    #print(f"Average Memory Usage: {avg_memory_usage:.1f}%")

    memoryUsagePerformance = ""
    if avg_memory_usage < 70:
        memoryUsagePerformance = "Poor"
    elif 70 <= avg_memory_usage < 85:
        memoryUsagePerformance = "Fair"
    else:
        memoryUsagePerformance = "Excellent"

    print(f"Memory Performance: {memoryUsagePerformance}")

    return avg_memory_usage

def extract_gpumetrics(json_data):
    metrics = {
        "gpu_temp": None,
        "gpu_usage": None
    }
    target_keys = {
        "gpu core": "gpu_temp",
        "gpu core": "gpu_usage"
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
                    print({value})  

            for child in data.get("Children", []):
                recursive_search(child, target_keys)
        elif isinstance(data, list):
            for item in data:
                recursive_search(item, target_keys)


    recursive_search(json_data, target_keys)
    return metrics

def calculate_gpu_average(metrics_list):

    avg_metrics = {key: 0 for key in metrics_list[0].keys()}

    for metrics in metrics_list:
        for key, value in metrics.items():
            if value is not None:
                avg_metrics[key] += value

    for key in avg_metrics:
        avg_metrics[key] = round(avg_metrics[key] / len(metrics_list), 1)

    return avg_metrics

def gpu_stress_test(duration, num_shapes=1000, complexity_factor=50):
    processes = []
    metrics_list = []

    pygame.init()
    display = (1920, 1080)
    pygame.display.set_mode(display, pygame.DOUBLEBUF | pygame.OPENGL)
    
    start_time = time.time()

    while time.time() - start_time < duration:
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

        for _ in range(num_shapes):
            shape_type = random.choice(['triangle', 'quad'])
            glColor3f(random.random(), random.random(), random.random())

            if shape_type == 'triangle':
                glBegin(GL_TRIANGLES)
                for _ in range(3 * complexity_factor):
                    glVertex3f(random.uniform(-2, 2), random.uniform(-2, 2), random.uniform(-2, 2))
                glEnd()
            elif shape_type == 'quad':
                glBegin(GL_QUADS)
                for _ in range(4 * complexity_factor):
                    glVertex3f(random.uniform(-2, 2), random.uniform(-2, 2), random.uniform(-2, 2))
                glEnd()

        pygame.display.flip()
        pygame.time.wait(5)

        data = fetch_ohm_data()
        if data:
            metrics = extract_gpumetrics(data)
            #print(metrics)
            metrics_list.append(metrics)

    avg_metrics = calculate_gpu_average(metrics_list)
    
    gpu_temp = avg_metrics["gpu_temp"]
    gpu_usage = avg_metrics["gpu_usage"]
    print(f"Average GPU metrics: {avg_metrics}" )
    if gpu_temp is not None:
        if gpu_temp < 60:
            temp_performance = "Excellent"
        elif 60 <= gpu_temp < 80:
            temp_performance = "Good"
        elif 80 <= gpu_temp < 90:
            temp_performance = "Fair"
        else:
            temp_performance = "Poor"
        print(f"gpu Tempture Performance: {temp_performance}")

    if gpu_usage is not None:
        if gpu_usage < 70:
            usage_performance = "Poor"
        elif 70 <= gpu_usage < 90:
            usage_performance = "Fair"
        else:
            usage_performance = "Excellent"

        print(f"gpu Usage Performance: {usage_performance}")

    return avg_metrics

if __name__ == "__main__":
    start_open_hardware_monitor()
    time.sleep(2)  
    duration = 14
    cpu_stresser(duration)
    time.sleep(2)
    duration = 20
    memory_stresser(duration)
    time.sleep(2)
    duration = 14
    gpu_stress_test(duration)
