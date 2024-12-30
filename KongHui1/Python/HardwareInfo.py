import psutil
import cpuinfo
import platform
import time
from datetime import timedelta
import wmi

# 内存类型映射表
MEMORY_TYPE_MAPPING = {
    0: "未知",
    1: "其他",
    2: "DRAM",
    3: "同步DRAM",
    4: "缓存DRAM",
    5: "EDO",
    6: "EDRAM",
    7: "VRAM",
    8: "SRAM",
    9: "RAM",
    10: "ROM",
    11: "FLASH",
    12: "EEPROM",
    13: "FEPROM",
    14: "EPROM",
    15: "CDRAM",
    16: "3DRAM",
    17: "SDRAM",
    18: "SGRAM",
    19: "RDRAM",
    20: "DDR",
    21: "DDR2",
    22: "DDR2 FB-DIMM",
    24: "DDR3",
    25: "FBD2",
}

# FormFactor 映射表
FORM_FACTOR_MAPPING = {
    0: "未知",
    1: "其他",
    2: "SIP",
    3: "DIP",
    4: "ZIP",
    5: "SOJ",
    6: "专用",
    7: "SIMM",
    8: "DIMM",
    9: "TSOP",
    10: "PGA",
    11: "RIMM",
    12: "SODIMM",
    13: "SRIMM",
    14: "SMD",
    15: "SSMP",
    16: "QFP",
    17: "TQFP",
    18: "SOIC",
    19: "LCC",
    20: "PLCC",
    21: "BGA",
    22: "FPBGA",
    23: "LGA",
    24: "FB-DIMM",
}

# 磁盘类型映射表
DISK_TYPE_MAPPING = {
    "Fixed hard disk media": "硬盘 (HDD)",
    "Removable media": "可移动媒体",
    "External hard disk media": "外部硬盘",
    "Solid state disk": "固态硬盘 (SSD)"
}

def get_cpu_info():
    cpu_info = cpuinfo.get_cpu_info()
    cpu_brand = cpu_info.get('brand_raw', '未知')
    cpu_arch = cpu_info.get('arch', '未知')
    cpu_speed = cpu_info.get('hz_advertised_friendly', '未知')
    cpu_physical_cores = psutil.cpu_count(logical=False)
    cpu_logical_processors = psutil.cpu_count(logical=True)
    cpu_l1_cache = cpu_info.get('l1_data_cache_size', '未知')
    cpu_l2_cache = cpu_info.get('l2_cache_size', '未知')
    cpu_l3_cache = cpu_info.get('l3_cache_size', '未知')
    cpu_percent = psutil.cpu_percent(interval=1)

    boot_time = psutil.boot_time()
    uptime = timedelta(seconds=(time.time() - boot_time))

    total_processes = len(psutil.pids())
    total_threads = sum(proc.info['num_threads'] for proc in psutil.process_iter(attrs=['num_threads']))

    return {
        "cpu_brand": cpu_brand,
        "cpu_arch": cpu_arch,
        "cpu_speed": cpu_speed,
        "cpu_physical_cores": cpu_physical_cores,
        "cpu_logical_processors": cpu_logical_processors,
        "cpu_l1_cache": cpu_l1_cache,
        "cpu_l2_cache": cpu_l2_cache,
        "cpu_l3_cache": cpu_l3_cache,
        "cpu_percent": f"{cpu_percent:.2f}%",
        "uptime": str(uptime),
        "total_processes": total_processes,
        "total_threads": total_threads
    }

def get_disk_info():
    disk_info_list = []
    partitions = psutil.disk_partitions(all=False)
    disk_io_start = psutil.disk_io_counters(perdisk=True)
    time.sleep(1)
    disk_io_end = psutil.disk_io_counters(perdisk=True)

    for partition in partitions:
        usage = psutil.disk_usage(partition.mountpoint)
        total_size = f"{usage.total / (1024 ** 3):.2f} GB"
        
        device_name = partition.device.split("\\")[-1]
        read_speed = write_speed = "未知"
        
        for dev_name, io_start in disk_io_start.items():
            if device_name in dev_name:
                io_end = disk_io_end[dev_name]
                read_speed = f"{(io_end.read_bytes - io_start.read_bytes) / (1024 ** 2):.2f} MB/s"
                write_speed = f"{(io_end.write_bytes - io_start.write_bytes) / (1024 ** 2):.2f} MB/s"
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

        disk_info_list.append({
            "partition": partition.device,
            "mountpoint": partition.mountpoint,
            "total_size": total_size,
            "read_speed": read_speed,
            "write_speed": write_speed,
            "model": model,
            "disk_type": disk_type
        })

    return disk_info_list

def get_memory_info():
    virtual_memory = psutil.virtual_memory()
    memory_total = f"{virtual_memory.total / (1024 ** 2):.2f} MB"
    memory_percent = f"{virtual_memory.percent:.2f}%"

    memory_info_list = []
    w = wmi.WMI()
    
    for mem in w.Win32_PhysicalMemory():
        form_factor = FORM_FACTOR_MAPPING.get(mem.FormFactor, "未知")
        memory_type = MEMORY_TYPE_MAPPING.get(mem.MemoryType, "未知")
        
        memory_info_list.append({
            "speed": f"{mem.Speed} MHz",
            "capacity": f"{int(mem.Capacity) / (1024 ** 3):.2f} GB",
            "part_number": mem.PartNumber.strip(),
            "manufacturer": mem.Manufacturer.strip(),
            "form_factor": form_factor,
            "memory_type": memory_type
        })

    return {
        "memory_total": memory_total,
        "memory_percent": memory_percent,
        "memory_details": memory_info_list
    }

if __name__ == "__main__":
    cpu_info = get_cpu_info()
    disk_info = get_disk_info()
    memory_info = get_memory_info()
