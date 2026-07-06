using System;
using System.Threading;
using System.Windows;

namespace Lesson5.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // --- Существующий метод для подсчета СУММЫ ---
        private void btnSum_Click(object sender, RoutedEventArgs e)
        {
            if (!long.TryParse(txtN.Text, out long n) || n <= 0)
            {
                MessageBox.Show("Введите корректное положительное число N", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblSumResult.Text = "Вычисление...";
            progressBar.IsIndeterminate = true;

            // Запускаем расчет суммы в отдельном фоновом потоке, чтобы UI не зависал
            Thread mainComputationThread = new Thread(() =>
            {
                long totalSum = CalculateParallelSum(n);

                // Возвращаем результат в UI-поток через Dispatcher
                Dispatcher.Invoke(() =>
                {
                    lblSumResult.Text = totalSum.ToString("N0");
                    progressBar.IsIndeterminate = false;
                });
            });

            mainComputationThread.Start();
        }

        private long CalculateParallelSum(long n)
        {
            int threadCount = Environment.ProcessorCount; // Число ядер процессора (например, 4, 8 или 16)
            Thread[] threads = new Thread[threadCount];
            long[] partialSums = new long[threadCount];
            long chunkSize = n / threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                long start = threadIndex * chunkSize + 1;
                long end = (threadIndex == threadCount - 1) ? n : start + chunkSize - 1;

                threads[i] = new Thread(() =>
                {
                    long sum = 0;
                    for (long j = start; j <= end; j++)
                    {
                        sum += j;
                    }
                    partialSums[threadIndex] = sum;
                });

                threads[i].Start();
            }

            foreach (var t in threads)
            {
                t.Join(); // Ждем завершения каждого вычислительного потока
            }

            long totalSum = 0;
            foreach (var s in partialSums)
            {
                totalSum += s;
            }

            return totalSum;
        }


        // =========================================================================
        // --- ДОРАБОТКА: НОВЫЙ КОД ДЛЯ ПОДCЧЕТА РАЗНОСТИ ЧИСЕЛ (ПАРАЛЛЕЛЬНО) ---
        // =========================================================================
        
        private void btnDiff_Click(object sender, RoutedEventArgs e)
        {
            if (!long.TryParse(txtN.Text, out long n) || n <= 0)
            {
                MessageBox.Show("Введите корректное положительное число N", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblDiffResult.Text = "Вычисление...";
            progressBar.IsIndeterminate = true;

            // Запускаем вычисление разности в фоновом потоке, сохраняя отзывчивость UI
            Thread mainComputationThread = new Thread(() =>
            {
                long totalDiff = CalculateParallelDifference(n);

                // Выводим результат в UI
                Dispatcher.Invoke(() =>
                {
                    lblDiffResult.Text = totalDiff.ToString("N0");
                    progressBar.IsIndeterminate = false;
                });
            });

            mainComputationThread.Start();
        }

        private long CalculateParallelDifference(long n)
        {
            int threadCount = Environment.ProcessorCount; // Динамически определяем количество логических ядер
            Thread[] threads = new Thread[threadCount];
            long[] partialSums = new long[threadCount]; // Массив для частичных сумм вычитаемых чисел
            long chunkSize = n / threadCount;

            // Параллельно вычисляем суммы порций вычитаемых чисел
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                long start = threadIndex * chunkSize + 1;
                long end = (threadIndex == threadCount - 1) ? n : start + chunkSize - 1;

                threads[i] = new Thread(() =>
                {
                    long localSum = 0;
                    for (long j = start; j <= end; j++)
                    {
                        localSum += j;
                    }
                    partialSums[threadIndex] = localSum; // Сохраняем результат вычислений потока
                });

                threads[i].Start();
            }

            // Ожидаем завершения параллельной работы всех созданных потоков
            foreach (var t in threads)
            {
                t.Join();
            }

            // Вычисляем финальную разность: изначальное значение (0) последовательно уменьшаем на частичные суммы
            long totalDiff = 0;
            foreach (var partialSum in partialSums)
            {
                totalDiff -= partialSum;
            }

            return totalDiff;
        }
    }
}