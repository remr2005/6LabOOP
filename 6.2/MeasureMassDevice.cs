using DeviceControl;

namespace MeasureLengthDeviceNamespace
{
    public class MeasureMassDevice : MeasureDataDevice, IMeasuringDevice
    {
        protected override DeviceType measurementType => DeviceType.MASS;
        public MeasureMassDevice(Units units)
        {
            unitsToUse = units;
            dataCaptured = new int[10]; // Буфер данных фиксированного размера
            LoggingFileName = "MeasureMassDevice";
        }
        /// <summary>
        /// Converts the raw data collected by the measuring device into a metric value
        /// </summary>
        ///<returns>The latest measurement from the device converted to metric units.</returns>
        public override decimal MetricValue() => (unitsToUse.Equals(Units.Metric)) ? mostRecentMeasure : mostRecentMeasure * 0.4536m;
        /// <summary>
        /// Converts the raw data collected by the measuring device into an imperial value.
        /// </summary>
        ///<returns>The latest measurement from the device converted to imperial units.</returns>
        public override decimal ImperialValue() => (unitsToUse.Equals(Units.Imperial)) ? mostRecentMeasure : mostRecentMeasure * 2.2046m;

    }
}