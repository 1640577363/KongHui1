import psutil
import matplotlib.pyplot as plt
import time
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.font_manager import FontProperties

def monitor_memory_usage():
    memory_usage_history = [0] * 60  # 保存内存使用率历史，初始化为0
    time_intervals = np.arange(60)  # 保存时间间隔，从0到9

    while True:
        # 获取内存使用率
        memory = psutil.virtual_memory()
        memory_usage = memory.percent  # 获取内存使用率的百分比
        memory_usage_history = memory_usage_history[1:] + [memory_usage]  # 更新内存使用率历史
        plt.rcParams['font.sans-serif'] = ['Microsoft YaHei']  
        plt.rcParams['axes.unicode_minus'] = False 
        # 创建图像
        plt.figure(figsize=(10,3))  # 调整图像大小
        plt.plot(time_intervals, memory_usage_history, label="内存使用率 %")
        plt.ylim(0, 100)  # 设置 y 轴范围为0%到100%
        
        plt.yticks(fontsize=18)
        x_ticks = np.arange(0, 60, 10).tolist() + [59]
        x_tick_labels = [f"{60 - t}s" if t != 59 else "0s" for t in x_ticks]
        plt.xticks(x_ticks, x_tick_labels, fontsize=18)

        plt.grid(True, linestyle='--', linewidth=0.5, color='gray', alpha=0.5)
        plt.legend(fontsize=20,bbox_to_anchor=(0.5, 1.3), loc='upper center')

        output_path = r"D:\Project\UNO2\KongHui1\Python\memory_usage_chart.png"
        plt.savefig(output_path, facecolor='none', edgecolor='none', bbox_inches="tight")
        plt.close()  # 关闭图表，防止窗口弹出

        # 设置更新间隔
        time.sleep(1)

if __name__ == "__main__":
    monitor_memory_usage()
