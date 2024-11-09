using System;

namespace UP_01_02_Opsinnikova_Daria_ISIP_221
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int m = ReadInt("Введите количество поставщиков:");
            int n = ReadInt("Введите количество потребителей:");

            int[,] costs = new int[m, n];
            int[] supply = new int[m];
            int[] demand = new int[n];

            Console.WriteLine("Введите матрицу стоимостей перевозок:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    costs[i, j] = ReadInt($"Введите стоимость перевозки для поставщика {i + 1} и потребителя {j + 1}:");
                }
            }

            Console.WriteLine("Введите запасы поставщиков:");
            for (int i = 0; i < m; i++)
            {
                supply[i] = ReadInt($"Введите запас поставщика {i + 1}:");
            }

            Console.WriteLine("Введите потребности потребителей:");
            for (int j = 0; j < n; j++)
            {
                demand[j] = ReadInt($"Введите потребность потребителя {j + 1}:");
            }

            Console.WriteLine("\nМатрица стоимостей перевозок до балансировки:");
            PrintMatrix(costs, supply, demand);

            // Проверка баланса
            int totalSupply = 0;
            int totalDemand = 0;

            foreach (int s in supply)
            {
                totalSupply += s;
            }

            foreach (int d in demand)
            {
                totalDemand += d;
            }

            if (totalSupply != totalDemand)
            {
                Console.WriteLine("\nЗадача не сбалансирована. Автоматическая балансировка...");

                if (totalSupply > totalDemand)
                {
                    // Добавляем фиктивного потребителя
                    int[,] newCosts = new int[m, n + 1];
                    int[] newDemand = new int[n + 1];

                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            newCosts[i, j] = costs[i, j];
                        }
                        newCosts[i, n] = 0; // Стоимость перевозки к фиктивному потребителю
                    }

                    for (int j = 0; j < n; j++)
                    {
                        newDemand[j] = demand[j];
                    }
                    newDemand[n] = totalSupply - totalDemand;

                    costs = newCosts;
                    demand = newDemand;
                    n++;
                }
                else
                {
                    // Добавляем фиктивного поставщика
                    int[,] newCosts = new int[m + 1, n];
                    int[] newSupply = new int[m + 1];

                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            newCosts[i, j] = costs[i, j];
                        }
                        newSupply[i] = supply[i];
                    }

                    for (int j = 0; j < n; j++)
                    {
                        newCosts[m, j] = 0; // Стоимость перевозки от фиктивного поставщика
                    }
                    newSupply[m] = totalDemand - totalSupply;

                    costs = newCosts;
                    supply = newSupply;
                    m++;
                }

                Console.WriteLine("\nМатрица стоимостей перевозок после балансировки:");
                PrintMatrix(costs, supply, demand);
            }
            else
            {
                Console.WriteLine("\nМатрица не нуждается в балансировке");
            }

            int[,] result = NorthWestCornerMethod(costs, supply, demand);

            Console.WriteLine("\nОпорный план:");
            PrintPlan(result);

            bool isOptimal = CheckOptimality(costs, result);

            if (isOptimal)
            {
                Console.WriteLine("План является оптимальным.");
            }
            else
            {
                Console.WriteLine("План не является оптимальным. Улучшение плана...");
                result = OptimizePlan(costs, result);
                Console.WriteLine("\nОптимальный план:");
                PrintPlan(result);
            }
            Console.ReadLine();
        }

        static int ReadInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.WriteLine(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out result))
                {
                    return result;
                }
                else
                {
                    Console.WriteLine("Ошибка ввода. Пожалуйста, введите целое число.");
                }
            }
        }

        static int[,] NorthWestCornerMethod(int[,] costs, int[] supply, int[] demand)
        {
            int m = supply.Length;
            int n = demand.Length;
            int[,] result = new int[m, n];

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int quantity = Math.Min(supply[i], demand[j]);
                result[i, j] = quantity;

                supply[i] -= quantity;
                demand[j] -= quantity;

                if (supply[i] == 0)
                    i++;
                if (demand[j] == 0)
                    j++;
            }

            return result;
        }

        static bool CheckOptimality(int[,] costs, int[,] plan)
        {
            int m = costs.GetLength(0);
            int n = costs.GetLength(1);

            int[] u = new int[m];
            int[] v = new int[n];

            // Инициализация потенциалов
            for (int i = 0; i < m; i++)
            {
                u[i] = int.MinValue;
            }
            for (int j = 0; j < n; j++)
            {
                v[j] = int.MinValue;
            }

            // Установка потенциала u[0] = 0
            u[0] = 0;

            // Расчет потенциалов
            bool[] uCalculated = new bool[m];
            bool[] vCalculated = new bool[n];
            uCalculated[0] = true;

            while (true)
            {
                bool updated = false;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (plan[i, j] > 0)
                        {
                            if (uCalculated[i] && !vCalculated[j])
                            {
                                v[j] = costs[i, j] - u[i];
                                vCalculated[j] = true;
                                updated = true;
                            }
                            else if (!uCalculated[i] && vCalculated[j])
                            {
                                u[i] = costs[i, j] - v[j];
                                uCalculated[i] = true;
                                updated = true;
                            }
                        }
                    }
                }

                if (!updated)
                    break;
            }

            // Проверка оптимальности
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (plan[i, j] == 0)
                    {
                        int delta = costs[i, j] - u[i] - v[j];
                        if (delta < 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        static int[,] OptimizePlan(int[,] costs, int[,] plan)
        {
            int m = costs.GetLength(0);
            int n = costs.GetLength(1);

            while (true)
            {
                int[] u = new int[m];
                int[] v = new int[n];

                // Инициализация потенциалов
                for (int i = 0; i < m; i++)
                {
                    u[i] = int.MinValue;
                }
                for (int j = 0; j < n; j++)
                {
                    v[j] = int.MinValue;
                }

                // Установка потенциала u[0] = 0
                u[0] = 0;

                // Расчет потенциалов
                bool[] uCalculated = new bool[m];
                bool[] vCalculated = new bool[n];
                uCalculated[0] = true;

                while (true)
                {
                    bool updated = false;

                    for (int i = 0; i < m; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (plan[i, j] > 0)
                            {
                                if (uCalculated[i] && !vCalculated[j])
                                {
                                    v[j] = costs[i, j] - u[i];
                                    vCalculated[j] = true;
                                    updated = true;
                                }
                                else if (!uCalculated[i] && vCalculated[j])
                                {
                                    u[i] = costs[i, j] - v[j];
                                    uCalculated[i] = true;
                                    updated = true;
                                }
                            }
                        }
                    }

                    if (!updated)
                        break;
                }

                // Поиск ячейки с наибольшим отрицательным дельта
                int maxDeltaI = -1, maxDeltaJ = -1;
                int maxDelta = 0;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (plan[i, j] == 0)
                        {
                            int delta = costs[i, j] - u[i] - v[j];
                            if (delta < maxDelta)
                            {
                                maxDelta = delta;
                                maxDeltaI = i;
                                maxDeltaJ = j;
                            }
                        }
                    }
                }

                if (maxDeltaI == -1)
                    break; // План оптимален

                // Построение цикла пересчета
                bool[,] inCycle = new bool[m, n];
                inCycle[maxDeltaI, maxDeltaJ] = true;
                FindCycle(plan, inCycle, maxDeltaI, maxDeltaJ, maxDeltaI, maxDeltaJ);

                // Пересчет плана
                int minQuantity = int.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (inCycle[i, j])
                        {
                            if ((i + j) % 2 == 1)
                            {
                                minQuantity = Math.Min(minQuantity, plan[i, j]);
                            }
                        }
                    }
                }

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (inCycle[i, j])
                        {
                            if ((i + j) % 2 == 0)
                            {
                                plan[i, j] += minQuantity;
                            }
                            else
                            {
                                plan[i, j] -= minQuantity;
                            }
                        }
                    }
                }
            }

            return plan;
        }

        static bool FindCycle(int[,] plan, bool[,] inCycle, int i, int j, int startI, int startJ)
        {
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            if (i != startI && j == startJ)
                return true;

            for (int k = 0; k < m; k++)
            {
                if (k != i && plan[k, j] > 0 && !inCycle[k, j])
                {
                    inCycle[k, j] = true;
                    if (FindCycle(plan, inCycle, k, j, startI, startJ))
                        return true;
                    inCycle[k, j] = false;
                }
            }

            for (int k = 0; k < n; k++)
            {
                if (k != j && plan[i, k] > 0 && !inCycle[i, k])
                {
                    inCycle[i, k] = true;
                    if (FindCycle(plan, inCycle, i, k, startI, startJ))
                        return true;
                    inCycle[i, k] = false;
                }
            }

            return false;
        }

        static void PrintPlan(int[,] plan)
        {
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            // Определение максимальной длины числа для выравнивания
            int maxLength = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    maxLength = Math.Max(maxLength, plan[i, j].ToString().Length);
                }
            }

            // Вывод опорного плана
            for (int i = 0; i < m; i++)
            {
                Console.Write("    ");
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{plan[i, j]}".PadRight(maxLength + 2));
                }
                Console.WriteLine();
            }

        }

        static void PrintMatrix(int[,] matrix, int[] supply, int[] demand)
        {
            int m = matrix.GetLength(0);
            int n = matrix.GetLength(1);

            // Определение максимальной длины числа для выравнивания
            int maxLength = Math.Max(supply.Max().ToString().Length, demand.Max().ToString().Length);
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    maxLength = Math.Max(maxLength, matrix[i, j].ToString().Length);
                }
            }

            // Вывод матрицы стоимостей перевозок и запасов поставщиков
            for (int i = 0; i < m; i++)
            {
                Console.Write("    ");
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{matrix[i, j]}".PadRight(maxLength + 2));
                }
                Console.WriteLine($"| {supply[i]}".PadRight(maxLength + 2));
            }

            // Вывод разделителя
            Console.WriteLine(new string('-', (maxLength + 3) * n + 3));

            // Вывод потребностей потребителей
            Console.Write("    ");
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{demand[j]}".PadRight(maxLength + 2));
            }
            Console.WriteLine();
        }
    }
}