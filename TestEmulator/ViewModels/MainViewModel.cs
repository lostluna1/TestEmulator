using System.Collections.ObjectModel;
using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Dispatching;
using TestEmulator.Core.Helpers;
using TestEmulator.Core.Models;

namespace TestEmulator.ViewModels;

public partial class MainViewModel : ObservableRecipient, IBaseViewModel
{
    private readonly SerialPort _serialPort;
    private readonly SerialJsonParser _jsonParser;
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _isReading = false;

    [ObservableProperty]
    public partial SensorData Sensor { get; set; } = new();

    [ObservableProperty]
    public partial bool IsConnected { get; set; } = false;

    [ObservableProperty]
    public partial string Status { get; set; } = "未连接";

    // 温度数据点集合，用于实时更新图表
    public ObservableCollection<ObservableValue> TemperatureValues { get; set; } = new();

    // 图表系列集合
    [ObservableProperty]
    public partial ISeries[] TemperatureSeries
    {
        get; set;
    }

    // 最大显示数据点数
    private const int MaxDataPoints = 50;

    public MainViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _serialPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
        _jsonParser = new SerialJsonParser();

        // 初始化温度图表
        InitializeTemperatureChart();

        // 订阅解析器事件
        _jsonParser.DataParsed += OnDataParsed;
        _jsonParser.ParseError += OnParseError;

        _serialPort.DataReceived += OnSerialDataReceived;

        StartReading();
    }

    [RelayCommand]
    void StartReading()
    {
        try
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                Status = "已连接 COM4";
                IsConnected = true;
            }

            _isReading = true;
            Status = "正在实时接收数据...";
        }
        catch (Exception ex)
        {
            Status = $"连接失败: {ex.Message}";
            IsConnected = false;
        }
    }

    [RelayCommand]
    void StopReading()
    {
        _isReading = false;

        if (_serialPort.IsOpen)
        {
            Status = "已连接，停止接收";
        }
    }

    [RelayCommand]
    void Close()
    {
        StopReading();

        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        IsConnected = false;
        Status = "未连接";
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (!_isReading || sender is not SerialPort port)
            return;

        try
        {
            var data = port.ReadExisting();
            if (!string.IsNullOrEmpty(data))
            {
                _jsonParser.AddData(data);
            }
        }
        catch (Exception ex)
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                Status = $"读取错误: {ex.Message}";
            });
        }
    }

    private void OnDataParsed(object sender, SensorDataReceivedEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            Sensor = e.Data;
            Status = $"实时数据 {DateTime.Now:HH:mm:ss.fff} - 温度: {e.Data.Temperature}°C";

            UpdateTemperatureChart(e.Data.Temperature);
        });
    }

    private void OnParseError(object sender, SensorDataReceivedEventArgs e)
    {
        _dispatcherQueue?.TryEnqueue(() =>
        {
            Status = $"解析错误: {e.ParseError}";
        });
    }

    public void Dispose()
    {
        _isReading = false;

        // 取消事件订阅
        _jsonParser.DataParsed -= OnDataParsed;
        _jsonParser.ParseError -= OnParseError;
        _serialPort.DataReceived -= OnSerialDataReceived;

        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        _serialPort?.Dispose();
    }

    /// <summary>
    /// 初始化温度图表
    /// </summary>
    private void InitializeTemperatureChart()
    {
        // 创建温度折线图系列
        TemperatureSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Values = TemperatureValues,
                Name = "温度 (°C)",
                Fill = null, // 不填充区域
                GeometrySize = 5, // 数据点大小
                LineSmoothness = 0.7, // 平滑度
                DataPadding = new LiveChartsCore.Drawing.LvcPoint(0.5f, 0.5f)
            }
        };
    }

    /// <summary>
    /// 更新温度图表数据
    /// </summary>
    /// <param name="temperature">新的温度值</param>
    private void UpdateTemperatureChart(double temperature)
    {
        // 添加新的温度数据点
        TemperatureValues.Add(new ObservableValue(temperature));

        // 限制数据点数量，移除最旧的数据点
        if (TemperatureValues.Count > MaxDataPoints)
        {
            TemperatureValues.RemoveAt(0);
        }
    }
}