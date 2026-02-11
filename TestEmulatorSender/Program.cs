using System.IO.Ports;
using System.Text;
using System.Text.Json;

namespace TestEmulatorSender;

internal class Program
{
    private static SerialPort? _sendPort; // COM3发送端口
    private static Timer? _timer;
    private static readonly Random _random = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("TestEmulator串口数据发送器启动...");

        try
        {
            // 初始化串口
            InitializePort();

            // 启动数据发送定时器
            StartDataSending();

            Console.WriteLine("串口通信已启动，正在向COM3发送数据...");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
        finally
        {
            CleanupPort();
        }
    }

    private static void InitializePort()
    {
        // 初始化COM3发送端口
        _sendPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

        try
        {
            _sendPort.Open();
            Console.WriteLine("COM3端口已打开 (发送数据)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"无法打开COM3端口: {ex.Message}");
            throw;
        }
    }

    private static void StartDataSending()
    {
        // 每2秒发送一次模拟数据
        _timer = new Timer(SendSimulatedData, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    private static void SendSimulatedData(object? state)
    {
        if (_sendPort?.IsOpen != true) return;

        try
        {
            // 生成模拟数据
            var simulatedData = new
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Temperature = Math.Round(_random.NextDouble() * 50 + 15, 2), // 15-65℃
                DeviceStatus = _random.Next(0, 2) == 0 ? "正常" : "警告",
                Humidity = Math.Round(_random.NextDouble() * 100, 1), // 0-100%
                Pressure = Math.Round(_random.NextDouble() * 200 + 800, 1), // 800-1000 hPa
                Voltage = Math.Round(_random.NextDouble() * 5 + 10, 2), // 10-15V
                Current = Math.Round(_random.NextDouble() * 2, 3), // 0-2A
                DeviceId = $"DEV_{_random.Next(1000, 9999)}",
                ErrorCode = _random.Next(0, 5) == 0 ? _random.Next(100, 999) : 0
            };

            // 序列化为JSON
            var jsonData = JsonSerializer.Serialize(simulatedData, new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            });

            // 添加换行符
            jsonData += "\r\n";

            // 发送数据到COM3
            _sendPort.Write(jsonData);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 已发送到COM3: 温度={simulatedData.Temperature}℃, 状态={simulatedData.DeviceStatus}, 设备ID={simulatedData.DeviceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送数据时出错: {ex.Message}");
        }
    }

    private static void CleanupPort()
    {
        _timer?.Dispose();

        if (_sendPort?.IsOpen == true)
        {
            _sendPort.Close();
            Console.WriteLine("COM3端口已关闭");
        }
        _sendPort?.Dispose();

        Console.WriteLine("串口通信已停止");
    }
}
