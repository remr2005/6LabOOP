using System;
using System.Text;
using System.Windows;
using MeasureLengthDeviceNamespace;

namespace _5._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EventHandler<HeartBeatEventArgs> heartBeatHandler;
        private IEventEnabledMeasuringDevice device;
        private Units unit = Units.Imperial;
        public MainWindow() => InitializeComponent();
        private EventHandler newMeasurementTaken;
        /// <summary>
        /// Обработчик для createInstance_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createInstance_Click(object sender, RoutedEventArgs e)
        {
            device = new MeasureMassDevice(unit);
            logs.Content = "Device created";
        }
        /// <summary>
        /// Когда пользователь прожимает один из радиобаттонов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Radio_Button_Click(object sender, RoutedEventArgs e) => unit = (Imperial_Button.IsChecked == true) ? Units.Imperial : Units.Metric;
        /// <summary>
        /// обработчик для прожатия StartCollecting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Collecting_Click(object sender, RoutedEventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null) 
            {
                // Инициализация делегата, который будет вызывать обработчик события при каждом новом измерении
                newMeasurementTaken = new EventHandler(device_NewMeasurementTaken);
                heartBeatHandler = (o, args) =>
                {
                    HeartBeatLabel.Content = $"HeartBeat Timestamp: {args.TimeStamp}"; // Обновление метки
                };
                // Привязка обработчика события HeartBeat с использованием лямбда-выражения
                device.HeartBeat += heartBeatHandler;
                // Привязка делегата к событию NewMeasurementTaken устройства
                device.NewMeasurementTaken += newMeasurementTaken;
                device.StartCollecting();
                logs.Content = "Start Collecting";
            }
        }
        /// <summary>
        /// обработчик для GetRawData_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetRawData_Click(object sender, RoutedEventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null)
            {
                StringBuilder res = new StringBuilder();
                foreach (int i in device.GetRawData()) { res.Append($"{i} "); }
                logs.Content = res.ToString();
            }
        }
        /// <summary>
        /// обработка для GetMetric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetMetric_Click(object sender, RoutedEventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null)
            {
                logs.Content = $"MetricValue: {device.MetricValue()}";
            }
        }
        /// <summary>
        /// обработчик для GetMetric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetImperial_Click(object sender, RoutedEventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null)
            {
                logs.Content = $"ImperialValue: {device.ImperialValue()}";
            }
        }
        /// <summary>
        /// Обработчик для Stop_Collecting_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Collecting_Click(object sender, RoutedEventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null)
            {
                device.StopCollecting();
                device.NewMeasurementTaken -= newMeasurementTaken;
                logs.Content = $"Device stop collecting";
                if (heartBeatHandler != null)
                {
                    device.HeartBeat -= heartBeatHandler;
                    heartBeatHandler = null;
                }
            }
        }
        /// <summary>
        /// Обработчик для не пойми чего(зааааааааааааааааачем блять это нужно???)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void device_NewMeasurementTaken(object sender, EventArgs e)
        {
            logs.Content = "Please make device";
            if (device != null)
            {
                StringBuilder res = new StringBuilder();
                foreach (int i in device.GetRawData()) { res.Append($"{i} "); }
                logs.Content = res.ToString();
            }
        }
    }
}
