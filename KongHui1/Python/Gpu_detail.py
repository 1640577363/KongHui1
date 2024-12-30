import GPUtil
import wmi
import ctypes
import psutil
import platform
import time
import json
import os

def get_gpu_info():
    w = wmi.WMI()

    # 获取 DirectX 版本
    def get_directx_version():
        try:
            directx_dll = ctypes.windll.d3d9
            return "DirectX 9.0"
        except:
            try:
                directx_dll = ctypes.windll.d3d11
                return "DirectX 11.0"
            except:
                return "DirectX 版本未知"

    # 获取 GPU 使用率和内存
    gpus = GPUtil.getGPUs()
    if not gpus:
        print("未检测到 GPU")
        return

    gpu = gpus[0]  # 只采集第一个 GPU 信息
    name = gpu.name
    utilization_ratio = f"{gpu.load * 100:.2f}%"
    memoryTotal = f"{gpu.memoryTotal} MB"
    Used = f"{gpu.memoryUsed} MB"
    Free = f"{gpu.memoryFree} MB"

    # 使用 WMI 获取驱动程序信息、物理位置和日期
    driver_version = "未知"
    driver_date = "未知"
    location = "未知"
    dx_version = get_directx_version()

    for video_controller in w.Win32_VideoController():
        if gpu.name in video_controller.Name:
            driver_version = video_controller.DriverVersion or "未知"
            driver_date = video_controller.DriverDate.split('.')[0] if video_controller.DriverDate else "未知"
            location = video_controller.PNPDeviceID or "未知"
            break

    # 构造结果字典
    gpu_info = {
        "name": name,
        "utilization_ratio": utilization_ratio,
        "memoryTotal": memoryTotal,
        "Used": Used,
        "Free": Free,
        "driver_version": driver_version,
        "driver_date": driver_date,
        "DirectX_version": dx_version
    }

    # 写入 JSON 文件
    script_directory = os.path.dirname(os.path.abspath(__file__))
    json_path = os.path.join(script_directory, "gpuinfo.json")

    with open(json_path, "w", encoding="utf-8") as json_file:
        json.dump(gpu_info, json_file, ensure_ascii=False, indent=4)


if __name__ == "__main__":
    while True:
        get_gpu_info()
        time.sleep(1)
