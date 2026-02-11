using System.Text;
using System.Text.Json;
using TestEmulator.Core.Models;

namespace TestEmulator.Core.Helpers;

/// <summary>
/// 串口JSON数据解析器，处理数据分片和缓冲问题
/// </summary>
public class SerialJsonParser
{
    private StringBuilder _buffer = new();

    /// <summary>
    /// 数据成功解析事件
    /// </summary>
    public event EventHandler<SensorDataReceivedEventArgs> DataParsed;

    /// <summary>
    /// 数据解析错误事件
    /// </summary>
    public event EventHandler<SensorDataReceivedEventArgs> ParseError;

    /// <summary>
    /// 添加接收到的原始数据
    /// </summary>
    /// <param name="rawData">原始数据字符串</param>
    public void AddData(string rawData)
    {
        if (string.IsNullOrEmpty(rawData))
            return;

        _buffer.Append(rawData);

        // 尝试解析完整的JSON对象
        ProcessBuffer();
    }

    /// <summary>
    /// 处理缓冲区中的数据
    /// </summary>
    private void ProcessBuffer()
    {
        var bufferContent = _buffer.ToString();

        // 按换行符分割可能的多个JSON对象
        var lines = bufferContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 清空缓冲区
        _buffer.Clear();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // 跳过空行
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // 检查是否是完整的JSON对象
            if (IsCompleteJson(trimmedLine))
            {
                TryParseJson(trimmedLine);
            }
            else
            {
                // 如果不完整，放回缓冲区等待更多数据
                _buffer.Append(trimmedLine);
            }
        }
    }

    /// <summary>
    /// 检查字符串是否是完整的JSON对象
    /// </summary>
    /// <param name="jsonString">JSON字符串</param>
    /// <returns>是否完整</returns>
    private static bool IsCompleteJson(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return false;

        jsonString = jsonString.Trim();

        // 简单检查：以{开始，以}结束
        if (!jsonString.StartsWith("{") || !jsonString.EndsWith("}"))
            return false;

        // 计算大括号配对
        int braceCount = 0;
        bool inString = false;
        bool escaped = false;

        foreach (char c in jsonString)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (!inString)
            {
                if (c == '{')
                    braceCount++;
                else if (c == '}')
                    braceCount--;
            }
        }

        return braceCount == 0;
    }

    /// <summary>
    /// 尝试解析JSON字符串
    /// </summary>
    /// <param name="jsonString">JSON字符串</param>
    private void TryParseJson(string jsonString)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };

            var sensorData = JsonSerializer.Deserialize<SensorData>(jsonString, options);

            if (sensorData != null)
            {
                var eventArgs = new SensorDataReceivedEventArgs(sensorData, jsonString);
                DataParsed?.Invoke(this, eventArgs);
            }
            else
            {
                var errorArgs = new SensorDataReceivedEventArgs(jsonString, "反序列化结果为null");
                ParseError?.Invoke(this, errorArgs);
            }
        }
        catch (JsonException ex)
        {
            var errorArgs = new SensorDataReceivedEventArgs(jsonString, $"JSON解析错误: {ex.Message}");
            ParseError?.Invoke(this, errorArgs);
        }
        catch (Exception ex)
        {
            var errorArgs = new SensorDataReceivedEventArgs(jsonString, $"未知解析错误: {ex.Message}");
            ParseError?.Invoke(this, errorArgs);
        }
    }

    /// <summary>
    /// 清空缓冲区
    /// </summary>
    public void ClearBuffer()
    {
        _buffer.Clear();
    }

    /// <summary>
    /// 获取当前缓冲区内容（用于调试）
    /// </summary>
    public string GetBufferContent()
    {
        return _buffer.ToString();
    }
}