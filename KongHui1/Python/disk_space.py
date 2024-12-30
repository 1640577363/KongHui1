import psutil
import matplotlib.pyplot as plt
import math

def disk_usage_chart():
    # Get disk partitions
    partitions = psutil.disk_partitions()
    disk_usage = []

    # Gather disk usage information for each partition
    for partition in partitions:
        usage = psutil.disk_usage(partition.mountpoint)
        used = usage.used / (1024**3)  # Convert to GB
        free = usage.free / (1024**3)
        total = usage.total / (1024**3)
        used_percentage = (used / total) * 100
        free_percentage = (free / total) * 100
        disk_usage.append((partition.mountpoint, used, free, used_percentage, free_percentage))

    # Calculate grid dimensions for displaying pie charts
    num_partitions = len(disk_usage)
    cols = min(3, num_partitions)  # Up to 3 columns or fewer if fewer partitions
    rows = math.ceil(num_partitions / cols)

    fig, axs = plt.subplots(rows, cols, figsize=(6 * cols, 6 * rows))
    axs = axs.flatten() if num_partitions > 1 else [axs]  # Flatten the axs array if needed

    for i, part in enumerate(disk_usage):
        # Labels for the segments with line breaks
        labels = [f'{part[3]:.1f}%\n({part[1]:.2f} GB)', f'{part[4]:.1f}%\n({part[2]:.2f} GB)']
        sizes = [part[3], part[4]]
        colors = ['orange', 'green']

        # Create the pie chart with larger font size for labels
        wedges, texts = axs[i].pie(
            sizes, labels=labels, startangle=90, colors=colors,
            textprops={'fontsize': 18, 'verticalalignment': 'center', 'horizontalalignment': 'center'}
        )
        # Set title to only the drive letter
        axs[i].set_title(part[0].split(":")[0], fontsize=20, y=0.95)

    # Hide any unused subplots
    for j in range(i + 1, rows * cols):
        axs[j].axis('off')

    plt.tight_layout()

    output_path = r"D:\Project\UNO2\KongHui1\Python\disk_usage_chart.png"
    plt.savefig(output_path, facecolor='none', edgecolor='none')
    plt.close()

if __name__ == "__main__":
    disk_usage_chart()
