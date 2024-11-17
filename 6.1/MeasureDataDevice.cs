using DeviceControl;
using System;
using System.ComponentModel; // Для BackgroundWorker
using System.IO; // Для StreamWriter
using System.Timers;

namespace MeasureLengthDeviceNamespace
{
    public abstract class MeasureDataDevice : IEventEnabledMeasuringDevice
    {
        protected Units unitsToUse;
        protected int[] dataCaptured;
        protected int mostRecentMeasure;
        protected DeviceController? controller;
        protected abstract DeviceType measurementType { get; }

        private BackgroundWorker? dataCollector;
        private StreamWriter? loggingFileWriter;
        public event EventHandler? NewMeasurementTaken;
        public event HeartBeatEventHandler HeartBeat;

        // Интервал для HeartBeat
        public int HeartBeatInterval { get; private set; }

        // Абстрактные методы для конкретных устройств
        public abstract decimal MetricValue();
        public abstract decimal ImperialValue();

        protected virtual void OnNewMeasurementTaken()
        {
            NewMeasurementTaken?.Invoke(this, EventArgs.Empty);
        }
        public void StartCollecting()
        {
            controller = DeviceController.StartDevice(measurementType);
            GetMeasurements();
            loggingFileWriter = new StreamWriter(GetLoggingFile());
            loggingFileWriter.WriteLine($"Time: {DateTime.Now}, Start Collecting");
        }

        public void StopCollecting()
        {
            if (controller != null)
            {
                DeviceController.StopDevice();
                controller = null;
                loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Stop Collecting");
                loggingFileWriter.Close();
            }
        }

        public int[] GetRawData()
        {
            return dataCaptured;
        }

        // Метод для получения измерений
        private void GetMeasurements()
        {
            dataCollector = new BackgroundWorker();
            dataCollector.DoWork += dataCollector_DoWork;
            dataCollector.ProgressChanged += dataCollector_ProgressChanged;
            dataCollector.RunWorkerAsync();
        }

        private void dataCollector_DoWork(object sender, DoWorkEventArgs e)
        {
            dataCaptured = new int[10];
            int i = 0;
            while (!dataCollector.CancellationPending)
            {
                // Получаем новое измерение от контроллера и сохраняем его в массив
                dataCaptured[i % 10] = DeviceController.TakeMeasurement();
                // Обновляем последнее измерение
                mostRecentMeasure = dataCaptured[i % 10];
                loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, collect {mostRecentMeasure}");
                i++;
                System.Threading.Thread.Sleep(1000);
            }

        }
        private void dataCollector_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnNewMeasurementTaken();
        }
        public void Dispose()
        {
            // Проверяем, что BackgroundWorker не null
            if (dataCollector != null)
            {
                dataCollector.Dispose(); // Освобождаем ресурсы BackgroundWorker
            }

            // Закрываем файл логов, если он открыт
            loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Dispose");
            loggingFileWriter?.Close();
        }
        public string GetLoggingFile()
        {
            // Возвращаем путь к лог-файлу
            return $"{LoggingFileName}.log";
        }

        public int[] DataCaptured => dataCaptured;
        public Units UnitsToUse => unitsToUse;
        public int MostRecentMeasure => mostRecentMeasure;
        public string? LoggingFileName { get; set; }
    }
}