import time
import psutil
import GPUtil
import wmi
import subprocess
import json

# 初始化 WMI，用于硬件监控
w = wmi.WMI(namespace="root\\OpenHardwareMonitor")

# 设置监控阈值
CPU_TEMP_LIMIT = 90
GPU_TEMP_LIMIT = 90
HDD_TEMP_LIMIT = 60
CPU_USAGE_LIMIT = 90
MEMORY_USAGE_LIMIT = 90

# OpenHardwareMonitor.exe 的路径
OPEN_HARDWARE_MONITOR_PATH = "D:\\project\\UNO2\\KongHui1\\Python\\OpenHardwareMonitor\\OpenHardwareMonitor.exe"
subprocess.Popen(["runas", "/user:Administrator", OPEN_HARDWARE_MONITOR_PATH])
def get_cpu_temperature():
    for sensor in w.Sensor():
        if sensor.SensorType == 'Temperature' and "CPU" in sensor.Name:
            return sensor.Value
    return None


def get_gpu_temperature():
    gpus = GPUtil.getGPUs()
    if gpus:
        return gpus[0].temperature
    return None


def get_hdd_temperature():
    for sensor in w.Sensor():
        if sensor.SensorType == 'Temperature' in sensor.Name:
            return sensor.Value
    return None


def monitor_system(duration=15 * 60):
    start_time = time.time()

    # 启动 OpenHardwareMonitor.exe 进行硬件数据监控
    subprocess.Popen([OPEN_HARDWARE_MONITOR_PATH])

    result = {}

    while time.time() - start_time < duration:
        # 获取 CPU 和内存使用率
        cpu_usage = psutil.cpu_percent(interval=1)
        memory_usage = psutil.virtual_memory().percent

        # 获取温度数据
        cpu_temp = get_cpu_temperature()
        gpu_temp = get_gpu_temperature()
        hdd_temp = get_hdd_temperature()

        # 填充监控数据字典
        result["cpu_usage"] = cpu_usage
        result["memory_usage"] = memory_usage
        result["cpu_temp"] = cpu_temp
        result["gpu_temp"] = gpu_temp
        result["hdd_temp"] = hdd_temp

        # 每 10 秒获取一次数据
        time.sleep(10)

    # 将结果以 JSON 格式输出
    return json.dumps(result)


if __name__ == "__main__":
    # 调用监控系统函数并输出 JSON 格式结果
    output = monitor_system()
    print(output)
