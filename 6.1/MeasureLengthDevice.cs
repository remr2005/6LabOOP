using DeviceControl;

namespace MeasureLengthDeviceNamespace
{
    public class MeasureLengthDevice : MeasureDataDevice, IMeasuringDevice
    {
        protected override DeviceType measurementType => DeviceType.LENGTH;
        public MeasureLengthDevice(Units units)
        {
            unitsToUse = units;
            dataCaptured = new int[10]; // Буфер данных фиксированного размера
            LoggingFileName = "MeasureLengthDevice";
        }
        /// <summary>
        /// Converts the raw data collected by the measuring device into a metric value
        /// </summary>
        ///<returns>The latest measurement from the device converted to metric units.</returns>
        public override decimal MetricValue() => (unitsToUse.Equals(Units.Metric)) ? mostRecentMeasure : mostRecentMeasure * 25.4m;
        /// <summary>
        /// Converts the raw data collected by the measuring device into an imperial value.
        /// </summary>
        ///<returns>The latest measurement from the device converted to imperial units.</returns>
        public override decimal ImperialValue() => (unitsToUse.Equals(Units.Imperial)) ? mostRecentMeasure : mostRecentMeasure * 0.03937m;
    }
}
