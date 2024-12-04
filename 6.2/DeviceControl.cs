using System;

namespace DeviceControl
{
    public enum DeviceType
    {
        LENGTH,
        MASS
    }

    public class DeviceController
    {
        private static DeviceController? instance;
        private DeviceType deviceType;

        // Приватный конструктор
        private DeviceController(DeviceType type)
        {
            deviceType = type;
        }

        // Метод для запуска устройства
        public static DeviceController StartDevice(DeviceType type)
        {
            instance ??= new DeviceController(type);
            return instance;
        }

        // Метод для остановки устройства
        public static void StopDevice()
        {
            instance = null;
        }

        // Метод для получения измерения
        public static int TakeMeasurement()
        {
            // Эмуляция измерений: возвращаем случайное значение
            Random random = new Random();
            return random.Next(1, 100); // Генерация случайного значения от 1 до 100
        }
    }
}