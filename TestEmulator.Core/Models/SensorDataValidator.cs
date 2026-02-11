namespace TestEmulator.Core.Models;

/// <summary>
/// 传感器数据验证器
/// </summary>
public static class SensorDataValidator
{
    /// <summary>
    /// 验证传感器数据是否有效
    /// </summary>
    /// <param name="data">要验证的传感器数据</param>
    /// <returns>验证结果</returns>
    public static ValidationResult ValidateData(SensorData data)
    {
        var errors = new List<string>();

        // 验证时间戳
        if (string.IsNullOrWhiteSpace(data.Timestamp))
        {
            errors.Add("时间戳不能为空");
        }
        else if (data.GetTimestamp() == DateTime.MinValue)
        {
            errors.Add("时间戳格式无效");
        }

        // 验证温度范围
        if (data.Temperature < -50 || data.Temperature > 100)
        {
            errors.Add($"温度值超出合理范围: {data.Temperature}℃");
        }

        // 验证湿度范围
        if (data.Humidity < 0 || data.Humidity > 100)
        {
            errors.Add($"湿度值超出范围: {data.Humidity}%");
        }

        // 验证气压范围
        if (data.Pressure < 500 || data.Pressure > 1200)
        {
            errors.Add($"气压值超出合理范围: {data.Pressure} hPa");
        }

        // 验证电压范围
        if (data.Voltage < 0 || data.Voltage > 50)
        {
            errors.Add($"电压值超出合理范围: {data.Voltage}V");
        }

        // 验证电流范围
        if (data.Current < 0 || data.Current > 20)
        {
            errors.Add($"电流值超出合理范围: {data.Current}A");
        }

        // 验证设备状态
        if (string.IsNullOrWhiteSpace(data.DeviceStatus))
        {
            errors.Add("设备状态不能为空");
        }

        // 验证设备ID
        if (string.IsNullOrWhiteSpace(data.DeviceId))
        {
            errors.Add("设备ID不能为空");
        }

        // 验证错误代码
        if (data.ErrorCode < 0)
        {
            errors.Add($"错误代码不能为负数: {data.ErrorCode}");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 是否验证通过
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// 验证错误列表
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// 错误信息摘要
    /// </summary>
    public string ErrorSummary => string.Join("; ", Errors);

    public ValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList().AsReadOnly();
    }
}