import subprocess
import requests
import os
import time
import json
import psutil
import asyncio
import websockets


def is_process_running(process_name):
    """
    检测指定名称的进程是否已运行。
    :param process_name: 进程名称 (如 "OpenHardwareMonitor.exe")
    :return: 如果进程已运行返回 True，否则返回 False
    """
    for proc in psutil.process_iter(['name']):
        if proc.info['name'] == process_name:
            return True
    return False


def start_open_hardware_monitor():
    """
    启动 OpenHardwareMonitor，如果尚未运行。
    """
    open_hw_monitor_path = r"D:\Project\UNO2\KongHui1\Python\OpenHardwareMonitor\OpenHardwareMonitor.exe"
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
    """
    从 OpenHardwareMonitor 获取 JSON 数据。
    :return: JSON 数据字典
    """
    url = "http://localhost:8085/data.json"  # OpenHardwareMonitor 默认地址
    try:
        response = requests.get(url)
        response.raise_for_status()  # 检查 HTTP 请求是否成功
        return response.json()  # 返回 JSON 数据
    except requests.RequestException as e:
        print(f"无法连接 OpenHardwareMonitor: {e}")
        return None


def extract_metrics(json_data):
    
    metrics = {"cpu_temp": None, "cpu_usage": None, "gpu_temp": None, "hdd_temp": None, "fan_speed": None}

    def recursive_search(data, target_keys):
        
        if isinstance(data, dict):
            # Match specific metrics based on 'Text' key
            text = data.get("Text", "").lower()
            value = data.get("Value", None)

            if text in target_keys and value is not None:
                metrics[target_keys[text]] = float(value.split(" ")[0])  # Convert to float, strip units if present

            # Recurse into children if available
            for child in data.get("Children", []):
                recursive_search(child, target_keys)
        elif isinstance(data, list):
            for item in data:
                recursive_search(item, target_keys)

    # Define mapping of text keys to metrics
    target_keys = {
        "cpu total": "cpu_usage",
        "temperature #1": "cpu_temp",
        "gpu core": "gpu_temp",
        "fan #2": "fan_speed",
        "temperature #2": "hdd_temp",
    }

    # Start recursive search
    recursive_search(json_data, target_keys)
    
    return metrics


async def send_progress(websocket, total_duration):
    """
    发送任务的进度到 WebSocket 客户端。
    :param websocket: WebSocket 连接
    :param total_duration: 任务的总时长 (秒)
    """
    sampling_interval = 1  # 每次采样间隔 (秒)
    metrics_samples = []

    for elapsed_time in range(0, total_duration, sampling_interval):
        # 模拟采集数据
        progress = elapsed_time / total_duration * 100
        print(f"任务进度: {progress:.2f}%")
        await websocket.send(json.dumps({"progress": progress}))

        # 假装采集数据 (实际逻辑可以调用 fetch_ohm_data 和 extract_metrics)
        metrics_samples.append({"cpu_temp": 60, "cpu_usage": 50})  # 示例数据
        time.sleep(sampling_interval)

    # 完成任务，发送完成信号
    print("任务完成")
    await websocket.send(json.dumps({"progress": 100, "status": "complete"}))


async def websocket_server():
    try:
        async def handler(websocket, path):
            print("WebSocket 客户端已连接")
            await send_progress(websocket, total_duration=10)

        async with websockets.serve(handler, "localhost", 8765):
            print("WebSocket 服务器已启动")
            await asyncio.Future()  # 阻止退出
    except Exception as ex:
        print(f"WebSocket 服务器启动失败: {ex}")


def calculate_average(metrics_list):
    """
    计算多个采样点的平均值，保留一位小数。
    :param metrics_list: 采样点列表 (每个点是一个字典)
    :return: 平均值字典
    """
    avg_metrics = {key: 0 for key in metrics_list[0].keys()}

    for metrics in metrics_list:
        for key, value in metrics.items():
            if value is not None:
                avg_metrics[key] += value

    # 平均值计算，保留一位小数
    for key in avg_metrics:
        avg_metrics[key] = round(avg_metrics[key] / len(metrics_list), 1)

    print("平均值计算结果:", avg_metrics)
    return avg_metrics



def score_metrics(avg_metrics):
    """
    根据平均监控指标进行评分。
    :param avg_metrics: 平均监控指标字典
    :return: 评分详情和总分
    """
    scores = {"CPU": 0, "GPU": 0, "HDD": 0, "Fan": 0}
    total_score = 0

    # CPU 分数
    if avg_metrics["cpu_temp"] is not None:
        scores["CPU"] += max(0, 40 - max(0, (avg_metrics["cpu_temp"] - 90) * 2))
    if avg_metrics["cpu_usage"] is not None:
        scores["CPU"] += max(0, 40 - max(0, (avg_metrics["cpu_usage"] - 90)))
    scores["CPU"] = min(40, scores["CPU"])

    # GPU 分数
    if avg_metrics["gpu_temp"] is not None:
        scores["GPU"] = max(0, 30 - max(0, (avg_metrics["gpu_temp"] - 90) * 2))

    # HDD 分数
    if avg_metrics["hdd_temp"] is not None:
        scores["HDD"] = max(0, 20 - max(0, (avg_metrics["hdd_temp"] - 60) * 2))

    # Fan 分数
    if avg_metrics["fan_speed"] is not None and avg_metrics["fan_speed"] > 0:
        scores["Fan"] = 10

    # 总分 
    total_score = scores["CPU"] + scores["GPU"] + scores["HDD"] + scores["Fan"]

    return scores, total_score


if __name__ == "__main__":
    start_open_hardware_monitor()
    time.sleep(3)  # 等待 OpenHardwareMonitor 启动

    # 采样5秒内的数据
    sampling_duration = 5
    sampling_interval = 1
    metrics_samples = []

    for _ in range(sampling_duration // sampling_interval):
        data = fetch_ohm_data()
        if data:
            metrics = extract_metrics(data)
            metrics_samples.append(metrics)
        time.sleep(sampling_interval)

    # 计算平均值
    avg_metrics = calculate_average(metrics_samples)

    # 根据平均值评分
    scores, total_score = score_metrics(avg_metrics)

    # 输出结果
    print("硬件评分详情:")
    for component, score in scores.items():
        print(f"{component}: {score} 分")
    print(f"总分: {total_score} 分")
