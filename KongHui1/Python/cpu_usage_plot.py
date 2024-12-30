import psutil
import time
import numpy as np
import matplotlib.pyplot as plt

def monitor_cpu_usage():
    cpu_usage_history = [0] * 60
    time_intervals = np.arange(60)

    while True:
        cpu_usage = psutil.cpu_percent(interval=1)
        cpu_usage_history = cpu_usage_history[1:] + [cpu_usage]

        plt.figure(figsize=(10,3))
        plt.rcParams['font.sans-serif'] = ['Microsoft YaHei']
        plt.rcParams['axes.unicode_minus'] = False

        plt.plot(time_intervals, cpu_usage_history, label="CPU使用率 %")
        plt.ylim(0, 100)
        plt.yticks(fontsize=18)

        x_ticks = np.arange(0, 60, 10).tolist() + [59]
        x_tick_labels = [f"{60 - t}s" if t != 59 else "0s" for t in x_ticks]
        plt.xticks(x_ticks, x_tick_labels, fontsize=18)

        plt.grid(True, linestyle='--', linewidth=0.5, color='gray', alpha=0.5)

        plt.legend(fontsize=20, bbox_to_anchor=(0.5, 1.3), loc='upper center')

        output_path = r"D:\Project\UNO2\KongHui1\Python\cpu_usage_chart.png"
        plt.savefig(output_path, facecolor='none', edgecolor='none', bbox_inches="tight")
        plt.close()

        time.sleep(1)

if __name__ == "__main__":
    monitor_cpu_usage()
