namespace TestEmulator.Core.Models;

/// <summary>
/// 传感器数据接收事件参数
/// </summary>
public class SensorDataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// 接收到的传感器数据
    /// </summary>
    public SensorData Data { get; }

    /// <summary>
    /// 原始JSON字符串
    /// </summary>
    public string RawData { get; }

    /// <summary>
    /// 接收时间
    /// </summary>
    public DateTime ReceivedAt { get; }

    /// <summary>
    /// 是否解析成功
    /// </summary>
    public bool IsParseSuccessful { get; }

    /// <summary>
    /// 解析错误信息（如果有）
    /// </summary>
    public string ParseError { get; }

    public SensorDataReceivedEventArgs(SensorData data, string rawData)
    {
        Data = data;
        RawData = rawData;
        ReceivedAt = DateTime.Now;
        IsParseSuccessful = true;
        ParseError = string.Empty;
    }

    public SensorDataReceivedEventArgs(string rawData, string parseError)
    {
        Data = new SensorData();
        RawData = rawData;
        ReceivedAt = DateTime.Now;
        IsParseSuccessful = false;
        ParseError = parseError;
    }
}