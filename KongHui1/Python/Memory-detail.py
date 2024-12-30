import psutil
import wmi
import os
import json
import time
import tempfile
import shutil

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


def get_memory_info():
    # Get virtual memory details
    virtual_memory = psutil.virtual_memory()
    memory_total = f"{virtual_memory.total / (1024 ** 2):.2f} MB"
    memory_percent = f"{virtual_memory.percent:.2f}%"

    w = wmi.WMI()
    
    for mem in w.Win32_PhysicalMemory():
        form_factor = FORM_FACTOR_MAPPING.get(mem.FormFactor, "未知")
        memory_type = MEMORY_TYPE_MAPPING.get(mem.MemoryType, "未知")
        
        speed = f"{mem.Speed} MHz"
        capacity = f"{int(mem.Capacity) / (1024 ** 3):.2f} GB"
        part_number = mem.PartNumber.strip()
        manufacturer = mem.Manufacturer.strip()

        # Define the path for JSON output
        script_directory = os.path.dirname(os.path.abspath(__file__))
        json_path = os.path.join(script_directory, "Memory_info.json")

        # 使用临时文件写入数据，避免文件锁定
        temp_file_path = json_path + ".temp"

        # 创建一个临时文件并写入数据
        try:
            with open(temp_file_path, "w", encoding="utf-8") as temp_file:
                json.dump({
                    "speed": speed,
                    "capacity": capacity,
                    "part_number": part_number,
                    "form_factor": form_factor,
                    "memory_total": memory_total,
                    "manufacturer": manufacturer,
                    "memory_type": memory_type,
                    "memory_percent": memory_percent
                }, temp_file, ensure_ascii=False, indent=4)
            
            # 写入完成后，将临时文件重命名为正式的 JSON 文件
            if os.path.exists(json_path):
                os.remove(json_path)  # 删除旧文件
            shutil.move(temp_file_path, json_path)  # 将临时文件重命名为目标文件
        except Exception as e:
            print(f"写入文件时发生错误: {e}")


if __name__ == "__main__":
    while True:
        get_memory_info()
        time.sleep(1)
