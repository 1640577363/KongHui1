import psutil
import hashlib
import os
import threading

# 假设我们已有的病毒库，包含已知病毒文件的哈希值
virus_signatures = {
    'd2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2d2',  # 示例病毒文件1
    'e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3e3'   # 示例病毒文件2
}

def get_memory_files():
    """获取当前系统运行中进程的内存文件"""
    files = []
    for proc in psutil.process_iter(['pid', 'name', 'exe']):
        try:
            exe_path = proc.info['exe']
            if exe_path and os.path.exists(exe_path):
                files.append(exe_path)
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            continue
    return files

def calculate_file_hash(file_path):
    """计算文件的SHA256哈希值"""
    hash_sha256 = hashlib.sha256()
    try:
        with open(file_path, 'rb') as file:
            while chunk := file.read(4096):  # 分块读取
                hash_sha256.update(chunk)
        return hash_sha256.hexdigest()
    except Exception as e:
        print(f"无法读取文件 {file_path}: {e}")
        return None

def scan_file(file_path, virus_signatures):
    """扫描文件并匹配病毒签名"""
    file_hash = calculate_file_hash(file_path)
    if file_hash:
        if file_hash in virus_signatures:
            print(f"警告：文件 {file_path} 可能是病毒！")
        else:
            print(f"文件 {file_path} 安全")
    else:
        print(f"无法计算文件 {file_path} 的哈希值")

def scan_process_memory(virus_signatures):
    """扫描运行中的进程"""
    files = get_memory_files()
    threads = []
    
    for file in files:
        thread = threading.Thread(target=scan_file, args=(file, virus_signatures))
        thread.start()
        threads.append(thread)

    for thread in threads:
        thread.join()

def quick_scan(virus_signatures):
    """快速扫描，扫描所有当前加载的内存文件"""
    print("开始快速扫描...")
    scan_process_memory(virus_signatures)
    print("快速扫描完成")

if __name__ == "__main__":
    quick_scan(virus_signatures)
