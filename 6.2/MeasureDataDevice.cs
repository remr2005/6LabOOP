using DeviceControl;
using System;
using System.ComponentModel; // Для BackgroundWorker
using System.IO; // Для StreamWriter
using System.Timers;

namespace MeasureLengthDeviceNamespace
{
    /// <summary>
    /// Класс для HeartBeat
    /// </summary>
    public class HeartBeatEventArgs : EventArgs
    {
        /// <summary>
        /// Время
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Текущее время
        /// </summary>
        public HeartBeatEventArgs() : base()
        {
            TimeStamp = DateTime.Now;
        }
    }
    public abstract class MeasureDataDevice : IEventEnabledMeasuringDevice
    {
        public event EventHandler<HeartBeatEventArgs> HeartBeat;
        public int HeartBeatInterval = 1000;
        private System.Timers.Timer heartBeatTimer;
        private void StartHeartBeat()
        {
            BackgroundWorker heartBeatWorker = new BackgroundWorker();
            heartBeatWorker.WorkerSupportsCancellation = true;
            heartBeatWorker.WorkerReportsProgress = true;

            heartBeatWorker.DoWork += (o, args) =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(HeartBeatInterval);
                    if (disposed) break;
                    heartBeatWorker.ReportProgress(0);
                }
            };

            heartBeatWorker.ProgressChanged += (o, args) =>
            {
                OnHeartBeat(); // Вызов метода для поднятия события HeartBeat
            };

            heartBeatWorker.RunWorkerAsync(); // Запуск heartBeatWorker для асинхронного выполнения
        }
        protected virtual void OnHeartBeat(object sender, ElapsedEventArgs e)
        {
            // Проверяем, есть ли подписчики на событие
            HeartBeat?.Invoke(this, new HeartBeatEventArgs { TimeStamp = DateTime.Now });
        }
        protected virtual void OnHeartBeat()
        {
            OnHeartBeat(this, null); // Вызываем метод с sender и e
        }
        public MeasureDataDevice()
        {
            heartBeatTimer = new System.Timers.Timer(HeartBeatInterval);
            heartBeatTimer.Elapsed += OnHeartBeat; // Подписываемся на событие Elapsed

            StartHeartBeat(); // Запускаем HeartBeat
        }
        /// <summary>
        /// Используемый юнит
        /// </summary>
        protected Units unitsToUse;
        /// <summary>
        /// Захваченная инфа
        /// </summary>
        protected int[] dataCaptured;
        /// <summary>
        /// Последнее измерение
        /// </summary>
        protected int mostRecentMeasure;
        /// <summary>
        /// Котролер
        /// </summary>
        protected DeviceController? controller;
        /// <summary>
        /// Тип девайса
        /// </summary>
        protected abstract DeviceType measurementType { get; }
        /// <summary>
        /// Он занимается захватом данных
        /// </summary>
        private BackgroundWorker? dataCollector;
        /// <summary>
        /// Нужно для записи логов в файл
        /// </summary>
        private StreamWriter? loggingFileWriter;
        /// <summary>
        /// ГОСПОДИ
        /// </summary>
        public event EventHandler? NewMeasurementTaken;
        /// <summary>
        /// Уничтожен объект или нет
        /// </summary>
        bool disposed = false;
        /// <summary>
        /// Метрический результат
        /// </summary>
        /// <returns></returns>
        public abstract decimal MetricValue();
        /// <summary>
        /// Возвращает имперический результат
        /// </summary>
        /// <returns></returns>
        public abstract decimal ImperialValue();
        /// <summary>
        /// При новом измерении
        /// </summary>
        protected virtual void OnNewMeasurementTaken()
        {
            NewMeasurementTaken?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Начинает генерацию
        /// </summary>
        public void StartCollecting()
        {
            controller = DeviceController.StartDevice(measurementType);
            GetMeasurements();
            StartHeartBeat();
            loggingFileWriter = new StreamWriter(GetLoggingFile());
            loggingFileWriter.WriteLine($"Time: {DateTime.Now}, Start Collecting");
        }
        /// <summary>
        /// Останавливает генерацию
        /// </summary>
        public void StopCollecting()
        {
            if (controller != null)
            {
                DeviceController.StopDevice();
                controller = null;
                try
                {
                    loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Stop Collecting");
                    loggingFileWriter.Close();
                } catch { }
                if (dataCollector != null && dataCollector.IsBusy)
                {
                    dataCollector.CancelAsync(); // Запрос на отмену
                }
            }
            // Остановка heartbeat
            if (heartBeatTimer != null)
            {
                heartBeatTimer.Stop();
            }
        }
        /// <summary>
        /// Врзвращает все что измерил
        /// </summary>
        /// <returns></returns>
        public int[] GetRawData()
        {
            return dataCaptured;
        }
        /// <summary>
        /// Возвращает измерение
        /// </summary>
        private void GetMeasurements()
        {
            dataCollector = new BackgroundWorker
            {
                WorkerSupportsCancellation=true,
                WorkerReportsProgress=true,
            };
            dataCollector.DoWork += dataCollector_DoWork;
            dataCollector.ProgressChanged += dataCollector_ProgressChanged;
            dataCollector.RunWorkerAsync();
        }
        /// <summary>
        /// захватывает инфу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataCollector_DoWork(object sender, DoWorkEventArgs e)
        {
            dataCaptured = new int[10];
            int i = 0;
            while (!dataCollector.CancellationPending)
            {
                if (disposed) break;
                // Получаем новое измерение от контроллера и сохраняем его в массив
                dataCaptured[i % 10] = DeviceController.TakeMeasurement();
                // Обновляем последнее измерение
                mostRecentMeasure = dataCaptured[i % 10];
                loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, collect {mostRecentMeasure}");
                i++;
                dataCollector.ReportProgress(0);
                System.Threading.Thread.Sleep(1000);
            }

        }
        /// <summary>
        /// Когда что то меняется
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataCollector_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnNewMeasurementTaken();
        }
        /// <summary>
        /// Уничтожает, ну или делает вид что уничтожает MeasureDataDevice
        /// </summary>
        public void Dispose()
        {
            // Проверяем, что BackgroundWorker не null
            if (dataCollector != null)
            {
                disposed = true;
                dataCollector.Dispose(); // Освобождаем ресурсы BackgroundWorker
            }
            // Останавливаем и освобождаем таймер heartbeat
            if (heartBeatTimer != null)
            {
                heartBeatTimer.Stop();
                heartBeatTimer.Dispose(); // Освобождаем ресурсы
            }
            try
            {
                // Закрываем файл логов, если он открыт
                loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Dispose");
                loggingFileWriter?.Close();
            } catch { }
        }
        /// <summary>
        /// Путь до файла с логгированием
        /// </summary>
        /// <returns>Возвращает название файла логированния</returns>
        public string GetLoggingFile()
        {
            // Возвращаем путь к лог-файлу
            return $"{LoggingFileName}.log";
        }
        /// <summary>
        /// Сгенеренная инфа
        /// </summary>
        public int[] DataCaptured => dataCaptured;
        /// <summary>
        /// Юнит который используется
        /// </summary>
        public Units UnitsToUse => unitsToUse;
        /// <summary>
        /// Последнее измерение
        /// </summary>
        public int MostRecentMeasure => mostRecentMeasure;
        /// <summary>
        /// имя файла для логгинга
        /// </summary>
        public string? LoggingFileName { get; set; }
    }
}