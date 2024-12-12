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

        private Barrier barrier = new Barrier(2, (b) =>
        {
            Console.WriteLine($"Phase {b.CurrentPhaseNumber} completed.");
        });

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

            Task ball1Task = Task.Run(() => MoveBall(Ball1, ref ball1X, ref ball1Y, 5));
            Task ball2Task = Task.Run(() => MoveBall(Ball2, ref ball2X, ref ball2Y, 3));

            // Завершение симуляции, когда обе задачи выполнены
            Task.WhenAll(ball1Task, ball2Task).ContinueWith(_ =>
            {
                isSimulationRunning = false;
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Simulation completed.");
                });
            });
        }

        private readonly object lockObject = new object(); // Объект для защиты координат

        private void MoveBall(Ellipse ball, ref double x, ref double y, int step)
        {
            for (int i = 0; i < 120; i++)
            {
                lock (lockObject)
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
                }

                Thread.Sleep(10);

                barrier.SignalAndWait();
            }
        }
    }
}
