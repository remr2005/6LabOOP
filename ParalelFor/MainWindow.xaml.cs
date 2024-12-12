using System;
using System.Threading;
using System.Threading.Tasks;
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulationRunning)
            {
                MessageBox.Show("Simulation is already running.");
                return;
            }

            StartSimulation();
        }

        private void StartSimulation()
        {
            isSimulationRunning = true;

            // Запускаем многопоточный цикл с использованием Parallel.For
            Task.Run(() =>
            {
                Parallel.For(0, 120, i =>
                {
                    // Обновление шарика 1
                    MoveBall(Ball1, ref ball1X, ref ball1Y, 5);

                    // Обновление шарика 2
                    MoveBall(Ball2, ref ball2X, ref ball2Y, 3);
                });

                // После завершения цикла сбрасываем флаг
                Dispatcher.Invoke(() => isSimulationRunning = false);
            });
        }

        private readonly object lockObject = new object(); // Объект для защиты координат

        private void MoveBall(Ellipse ball, ref double x, ref double y, int step)
        {
            lock (lockObject) // Защита от одновременного обновления
            {
                double newX = x + step;
                double newY = y + step;

                Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(ball, newX);
                    Canvas.SetTop(ball, newY);
                });

                x = newX;
                y = newY;

                Thread.Sleep(10); // Задержка для демонстрации
            }
        }
    }
}
