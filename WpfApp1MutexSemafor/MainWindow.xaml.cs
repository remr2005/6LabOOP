using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private double ball1X = 0, ball1Y = 0;
        private double ball2X = 0, ball2Y = 0;
        private bool isSimulationRunning = false;

        private Barrier barrier = new Barrier(2, (b) =>
        {
            Console.WriteLine($"Phase {b.CurrentPhaseNumber} completed.");
        });

        // Мьютекс для защиты доступа к координатам
        private readonly Mutex mutex = new Mutex();

        // Семафор для управления количеством потоков
        private readonly Semaphore semaphore = new Semaphore(2, 2);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulationRunning) // Проверяем, не запущена ли симуляция уже
            {
                MessageBox.Show("Simulation is already running.");
                return;
            }

            StartSimulation();
        }

        private void StartSimulation()
        {
            isSimulationRunning = true;

            // Запуск потоков
            Thread ball1Thread = new Thread(() => MoveBall(Ball1, ref ball1X, ref ball1Y, 5));
            Thread ball2Thread = new Thread(() => MoveBall(Ball2, ref ball2X, ref ball2Y, 3));

            ball1Thread.Start();
            ball2Thread.Start();
        }

        private void MoveBall(Ellipse ball, ref double x, ref double y, int step)
        {
            for (int i = 0; i < 120; i++)
            {
                // Запрос разрешения у семафора
                semaphore.WaitOne();

                try
                {
                    // Блокировка мьютексом
                    mutex.WaitOne();

                    try
                    {
                        // Локальные копии координат
                        double newX = x + step;
                        double newY = y + step;

                        // Обновление UI в основном потоке
                        Dispatcher.InvokeAsync(() =>
                        {
                            Canvas.SetLeft(ball, newX);
                            Canvas.SetTop(ball, newY);
                        });

                        // Обновление исходных координат
                        x = newX;
                        y = newY;
                    }
                    finally
                    {
                        // Освобождение мьютекса
                        mutex.ReleaseMutex();
                    }
                }
                finally
                {
                    // Освобождение семафора
                    semaphore.Release();
                }

                // Пауза для демонстрации
                Thread.Sleep(10);

                // Синхронизация потоков с помощью Barrier
                barrier.SignalAndWait();
            }

            isSimulationRunning = false;
        }
    }
}
