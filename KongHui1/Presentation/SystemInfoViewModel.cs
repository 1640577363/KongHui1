using System.Collections.Generic;

namespace KongHui1.Presentation
{
    // 系统信息的 ViewModel
    public partial class SystemInfoViewModel
    {
        public _CPUInfo _Cpu { get; set; } = new _CPUInfo(); // CPU 信息
        public List<_DiskInfo> _Disks { get; set; } = new List<_DiskInfo>(); // 磁盘信息
        public _MemoryInfo _Memory { get; set; } = new _MemoryInfo(); // 内存信息
    }

    // CPU 信息类
    public class _CPUInfo
    {
        public string Brand { get; set; } // 品牌
        public string Architecture { get; set; } // 架构
        public string BaseSpeed { get; set; } // 基准速度
        public int PhysicalCores { get; set; } // 物理核心数
        public int LogicalProcessors { get; set; } // 逻辑处理器数
        public string L1Cache { get; set; } // L1 缓存
        public string L2Cache { get; set; } // L2 缓存
        public string L3Cache { get; set; } // L3 缓存
        public string Utilization { get; set; } // 利用率
        public int TotalProcesses { get; set; } // 总进程数
        public int TotalThreads { get; set; } // 总线程数
        public string Uptime { get; set; } // 系统正常运行时间
    }

    // 磁盘信息类
    public class _DiskInfo
    {
        public string DeviceName { get; set; } // 设备名称
        public string MountPoint { get; set; } // 挂载点
        public string TotalCapacity { get; set; } // 总容量
        public string ReadSpeed { get; set; } // 读取速度
        public string WriteSpeed { get; set; } // 写入速度
        public string Model { get; set; } // 型号
        public string Type { get; set; } // 类型
    }

    // 内存信息类
    public class _MemoryInfo
    {
        public string TotalSize { get; set; } // 总内存大小
        public string Usage { get; set; } // 当前内存使用率
        public string Speed { get; set; } // 速度
        public string Capacity { get; set; } // 容量
        public string Model { get; set; } // 型号
        public string Manufacturer { get; set; } // 制造商
        public string FormFactor { get; set; } // 外形规格
    }
}
