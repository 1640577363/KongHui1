import psutil
import matplotlib.pyplot as plt
import time
import numpy as np
import GPUtil

def monitor_cpu_usage():
    cpu_usage_history = [0] * 60
    time_intervals = np.arange(60)

    while True:
        # 获取 CPU 使用率
        cpu_usage = psutil.cpu_percent(interval=1)
        cpu_usage_history = cpu_usage_history[1:] + [cpu_usage]

        # 创建图像
        plt.figure(figsize=(6, 6))
        plt.plot(time_intervals, cpu_usage_history, label='CPU Usage (%)')
        plt.ylim(0, 100)
        plt.yticks(fontsize=18)

        # 自定义x轴刻度标签
        plt.xticks([0, 59], ['60s', '0s'], fontsize=18) 


        # 将图例放在图表外面
        plt.legend(fontsize=20, bbox_to_anchor=(0.5, 1.15), loc='upper center')

        plt.savefig("cpu_usage_chart.png")
        plt.close()

        time.sleep(1)

def disk_usage_chart():
    # 获取磁盘分区信息
    partitions = psutil.disk_partitions()
    disk_usage = []

    # 遍历所有分区，获取每个分区的使用情况
    for partition in partitions:
        usage = psutil.disk_usage(partition.mountpoint)
        used = usage.used / (1024**3)  # 转换为GB
        free = usage.free / (1024**3)
        total = usage.total / (1024**3)
        used_percentage = (used / total) * 100 
        free_percentage = (free / total) * 100
        
        disk_usage.append((partition.mountpoint, used, free, used_percentage, free_percentage))

    # 创建条形图
    labels = [part[0] for part in disk_usage]
    sizes_used = [part[1] for part in disk_usage]
    sizes_free = [part[2] for part in disk_usage]
    used_percentages = [f"{part[3]:.1f}%" for part in disk_usage] 
    free_percentages = [f"{part[4]:.1f}%" for part in disk_usage]

    # 设置图形大小和条形图的位置
    fig, ax = plt.subplots(figsize=(6, 6))

    # 绘制条形图
    x_pos = range(len(labels))
    ax.bar(x_pos, sizes_used, color='orange', label='Used')
    ax.bar(x_pos, sizes_free, bottom=sizes_used, color='green', label='Free')

    # 在柱形图上标注百分比
    for i in x_pos:
        # 计算柱体的中间位置
        mid_point_used = sizes_used[i] / 2 + sizes_used[i] * 0.05
        mid_point_free = sizes_used[i] + sizes_free[i] / 2 - sizes_free[i] * 0.05
        
        ax.text(i, mid_point_used, used_percentages[i], ha='center', va='center', fontsize=20)
        ax.text(i, mid_point_free, free_percentages[i], ha='center', va='center', fontsize=20)

    ax.set_xticks(x_pos)
    ax.set_xticklabels(labels, rotation=0, fontsize=18)
    ax.legend(fontsize=18, bbox_to_anchor=(0.5, 1.2), loc='upper center', ncol=2)

    ax.tick_params(axis='both', which='major', labelsize=18)
    # 保存图像文件
    plt.tight_layout()  # 自动调整子图参数, 使之填充整个图像区域
    plt.savefig("disk_usage_chart.png")

def monitor_gpu_usage():
    gpu_usage_history = [0] * 60
    time_intervals = np.arange(60) 

    # 获取第一个可用的GPU设备
    gpus = GPUtil.getGPUs()
    gpu = gpus[0] 

    while True:
        # 获取 GPU 使用率
        gpu_usage = gpu.load * 100
        gpu_usage_history=gpu_usage_history[1:]+[gpu_usage]

        plt.figure(figsize=(6, 6)) 
        plt.plot(time_intervals, gpu_usage_history, label='GPU Usage (%)')
        plt.ylim(0, 100) 
        plt.yticks(fontsize=18)
        plt.xticks([0, 59], ['60s', '0s'], fontsize=18)
        plt.legend(fontsize=20, bbox_to_anchor=(0.5, 1.2), loc='upper center')


        plt.savefig("gpu_usage_chart.png")  
        plt.close() 
        time.sleep(1)


def monitor_memory_usage():
    memory_usage_history = [0] * 60  # 保存内存使用率历史，初始化为0
    time_intervals = np.arange(60)  # 保存时间间隔，从0到9

    while True:
        # 获取内存使用率
        memory = psutil.virtual_memory()
        memory_usage = memory.percent  # 获取内存使用率的百分比
        memory_usage_history = memory_usage_history[1:] + [memory_usage]  # 更新内存使用率历史

        # 创建图像
        plt.figure(figsize=(6, 6))  # 调整图像大小
        plt.plot(time_intervals, memory_usage_history, label='Memory Usage (%)')
        plt.ylim(0, 100)  # 设置 y 轴范围为0%到100%
        
        plt.yticks(fontsize=18)
        plt.xticks([0, 59], ['60s', '0s'], fontsize=18)
        plt.legend(fontsize=20,bbox_to_anchor=(0.5, 1.1), loc='upper center')

        # 保存图像文件并关闭图表，避免窗口弹出
        plt.savefig("memory_usage_chart.png")
        plt.close()  # 关闭图表，防止窗口弹出

        # 设置更新间隔
        time.sleep(1)



if __name__ == "__main__":
    monitor_cpu_usage()
    disk_usage_chart()
    monitor_gpu_usage()
    monitor_memory_usage()
