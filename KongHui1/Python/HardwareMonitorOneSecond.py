import requests

def fetch_ohm_data():
    """获取 OpenHardwareMonitor 数据"""
    try:
        response = requests.get("http://localhost:8085/data.json", timeout=5)
        response.raise_for_status()  # 检查 HTTP 请求是否成功
        return response.json()
    except requests.RequestException as e:
        print(f"无法连接 OpenHardwareMonitor: {e}")
        return None

def extract_metrics(json_data):
    """提取硬件监控数据"""
    metrics = {
        "cpu_temp": None,
        "cpu_usage": None,
        "gpu_temp": None,
        "hdd_temp": None,
        "fan_speed": None,
        "gpu_usage": None,
        "memory_usage": None,
        "hdd_usage": None
    }

    target_keys = {
        "cpu total": "cpu_usage",
        "temperature #1": "cpu_temp",
        "gpu core": "gpu_temp",
        "fan #2": "fan_speed",
        "temperature #2": "hdd_temp",
        "gpu memory": "gpu_usage",
        "memory": "memory_usage",
        "used space": "hdd_usage"
    }

    def extract_from_data(data):
        """递归提取数据"""
        if isinstance(data, dict):
            text = data.get("Text", "").lower()
            value = data.get("Value", None)
            if text in target_keys and value != "-":
                try:
                    metrics[target_keys[text]] = float(value.split(" ")[0])  # 转为浮动数
                except ValueError:
                    pass
            for child in data.get("Children", []):
                extract_from_data(child)
        elif isinstance(data, list):
            for item in data:
                extract_from_data(item)

    extract_from_data(json_data)
    return metrics

if __name__ == "__main__":
    data = fetch_ohm_data()
    if data:
        metrics = extract_metrics(data)
        print("当前硬件监控数据:", metrics)
