using System;

namespace MeasureLengthDeviceNamespace
{
    public interface IMeasuringDevice
    {
        /// <summary>
        /// Преобразует сырые данные, собранные измерительным устройством, в метрическое значение.
        /// </summary>
        /// <returns>Последнее измерение устройства, преобразованное в метрические единицы.</returns>
        decimal MetricValue();
        /// <summary>
        /// Преобразует сырые данные, собранные измерительным устройством, в имперское значение.
        /// </summary>
        /// <returns>Последнее измерение устройства, преобразованное в имперские единицы.</returns>
        decimal ImperialValue();
        /// <summary>
        /// Запускает измерительное устройство.
        /// </summary>
        void StartCollecting();
        /// <summary>
        /// Останавливает измерительное устройство.
        /// </summary>
        void StopCollecting();
        /// <summary>
        /// Предоставляет доступ к сырым данным устройства в любых единицах, которые используются в устройстве.
        /// </summary>
        /// <returns>Сырые данные устройства в родном формате.</returns>
        int[] GetRawData();
        /// <summary>
        /// Возвращает имя файла журнала для устройства.
        /// </summary>
        /// <returns>Имя файла журнала.</returns>
        string GetLoggingFile();
        /// <summary>
        /// Получает единицы измерения, используемые устройством.
        /// </summary>
        Units UnitsToUse { get; }
        /// <summary>
        /// Получает массив измерений, выполненных устройством.
        /// </summary>
        int[] DataCaptured { get; }
        /// <summary>
        /// Получает самое последнее измерение, выполненное устройством.
        /// </summary>
        int MostRecentMeasure { get; }
        /// <summary>
        /// Получает или устанавливает имя файла журнала, используемого устройством.
        /// Если имя файла журнала изменяется, закрывается текущий файл и создается новый.
        /// </summary>
        string LoggingFileName { get; set; }
    }
    interface IEventEnabledMeasuringDevice : IMeasuringDevice
    {
        event EventHandler NewMeasurementTaken;
        // Событие, которое срабатывает при каждом новом измерении.
        event EventHandler<HeartBeatEventArgs> HeartBeat;

    }
    public delegate void HeartBeatEventHandler(object sender, EventArgs e);
}
