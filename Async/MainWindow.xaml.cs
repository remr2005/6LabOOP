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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isSimulationRunning)
            {
                MessageBox.Show("Simulation is already running.");
                return;
            }

            isSimulationRunning = true;

            // Запуск асинхронной симуляции
            await StartSimulation();

            isSimulationRunning = false;
        }

        private async Task StartSimulation()
        {
            // Запуск двух задач параллельно
            var ball1Task = MoveBallAsync(Ball1, 5);
            var ball2Task = MoveBallAsync(Ball2, 3);

            // Ожидание завершения обеих задач
            await Task.WhenAll(ball1Task, ball2Task);
        }

        private async Task MoveBallAsync(Ellipse ball, int step)
        {
            double x = ball == Ball1 ? ball1X : ball2X;
            double y = ball == Ball1 ? ball1Y : ball2Y;

            for (int i = 0; i < 120; i++)
            {
                // Локальные копии координат
                double newX = x + step;
                double newY = y + step;

                // Обновление UI в основном потоке
                await Dispatcher.InvokeAsync(() =>
                {
                    Canvas.SetLeft(ball, newX);
                    Canvas.SetTop(ball, newY);
                });

                // Обновление исходных координат
                x = newX;
                y = newY;

                // Пауза для демонстрации
                await Task.Delay(10);
            }

            // Обновляем глобальные координаты по завершению
            if (ball == Ball1)
            {
                ball1X = x;
                ball1Y = y;
            }
            else
            {
                ball2X = x;
                ball2Y = y;
            }
        }
    }
}
