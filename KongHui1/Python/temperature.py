# temperature_monitor.py
import psutil
import GPUtil
import time

def get_cpu_temps():
    temps = psutil.sensors_temperatures()
    cpu_temperatures = []
    for name, entries in temps.items():
        for entry in entries:
            if 'core' in entry.label.lower():
                cpu_temperatures.append(entry.current)
    return cpu_temperatures

def get_gpu_temps():
    gpus = GPUtil.getGPUs()
    gpu_temperatures = []
    for gpu in gpus:
        gpu_temperatures.append(gpu.temperature)
    return gpu_temperatures

def get_disk_temps():
    temps = psutil.sensors_temperatures()
    disk_temperatures = []
    if temps:
        for name, entries in temps.items():
            for entry in entries:
                if 'disk' in entry.label.lower():
                    disk_temperatures.append(entry.current)
    return disk_temperatures

def monitor_temperature(interval=2):
    while True:
        cpu_temperatures = get_cpu_temps()
        gpu_temperatures = get_gpu_temps()
        disk_temperatures = get_disk_temps()

        cpu_temp_str = ', '.join(f"{temp}°C" for temp in cpu_temperatures)
        gpu_temp_str = ', '.join(f"{temp}°C" for temp in gpu_temperatures)
        disk_temp_str = ', '.join(f"{temp}°C" for temp in disk_temperatures)

        print(f"CPU Temperatures: {cpu_temp_str}")
        print(f"GPU Temperatures: {gpu_temp_str}")
        print(f"Disk Temperatures: {disk_temp_str}")
        
        time.sleep(interval)

if __name__ == "__main__":
    monitor_temperature()
