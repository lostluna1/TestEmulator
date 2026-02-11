using System.Text.Json.Serialization;

namespace TestEmulator.Core.Models;

/// <summary>
/// 传感器数据模型，用于接收串口传输的设备数据
/// </summary>
public class SensorData
{
    /// <summary>
    /// 数据时间戳
    /// </summary>
    [JsonPropertyName("Timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// 温度值（摄氏度）
    /// </summary>
    [JsonPropertyName("Temperature")]
    public double Temperature { get; set; }

    /// <summary>
    /// 设备状态
    /// </summary>
    [JsonPropertyName("DeviceStatus")]
    public string DeviceStatus { get; set; } = string.Empty;

    /// <summary>
    /// 湿度百分比 (0-100%)
    /// </summary>
    [JsonPropertyName("Humidity")]
    public double Humidity { get; set; }

    /// <summary>
    /// 大气压强 (hPa)
    /// </summary>
    [JsonPropertyName("Pressure")]
    public double Pressure { get; set; }

    /// <summary>
    /// 电压值 (V)
    /// </summary>
    [JsonPropertyName("Voltage")]
    public double Voltage { get; set; }

    /// <summary>
    /// 电流值 (A)
    /// </summary>
    [JsonPropertyName("Current")]
    public double Current { get; set; }

    /// <summary>
    /// 设备唯一标识符
    /// </summary>
    [JsonPropertyName("DeviceId")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 错误代码，0表示无错误
    /// </summary>
    [JsonPropertyName("ErrorCode")]
    public int ErrorCode { get; set; }

    /// <summary>
    /// 获取解析后的时间戳
    /// </summary>
    public DateTime GetTimestamp()
    {
        if (DateTime.TryParse(Timestamp, out var dateTime))
        {
            return dateTime;
        }
        return DateTime.MinValue;
    }

    /// <summary>
    /// 检查设备是否正常
    /// </summary>
    public bool IsDeviceNormal => DeviceStatus == "正常" && ErrorCode == 0;

    /// <summary>
    /// 获取温度状态描述
    /// </summary>
    public string GetTemperatureStatus()
    {
        return Temperature switch
        {
            < 20 => "低温",
            > 45 => "高温",
            _ => "正常"
        };
    }

    /// <summary>
    /// 获取湿度状态描述
    /// </summary>
    public string GetHumidityStatus()
    {
        return Humidity switch
        {
            < 30 => "干燥",
            > 70 => "潮湿",
            _ => "适宜"
        };
    }

    /// <summary>
    /// 计算功率 (P = V * I)
    /// </summary>
    public double CalculatedPower => Math.Round(Voltage * Current, 3);

    public override string ToString()
    {
        return $"设备: {DeviceId}, 温度: {Temperature}℃, 状态: {DeviceStatus}, 时间: {Timestamp}";
    }
}