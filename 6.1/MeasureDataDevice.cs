using DeviceControl;
using System;
using System.ComponentModel; // Для BackgroundWorker
using System.IO; // Для StreamWriter
using System.Timers;

namespace MeasureLengthDeviceNamespace
{
    public abstract class MeasureDataDevice : IEventEnabledMeasuringDevice
    {
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
        /// Я это даже не использовал, ЗАЧЕМ ЭТО НУЖНО
        /// </summary>
        /// <returns></returns>
        public abstract decimal MetricValue();
        /// <summary>
        /// Возвращает МЕтрический результат
        /// </summary>
        /// <returns></returns>
        public abstract decimal ImperialValue();
        /// <summary>
        /// Когда новая функция
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
                loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Stop Collecting");
                loggingFileWriter.Close();
                if (dataCollector != null && dataCollector.IsBusy)
                {
                    dataCollector.CancelAsync(); // Запрос на отмену
                }
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

            // Закрываем файл логов, если он открыт
            loggingFileWriter?.WriteLine($"Time: {DateTime.Now}, Dispose");
            loggingFileWriter?.Close();
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