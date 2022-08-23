using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cinema
{
    /// <summary>
    /// Основной класс приложения.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Общая прибыль кинотеатра.
        /// </summary>
        private static int totalProfit = 0;

        /// <summary>
        /// Количество обслуженных посетителей.
        /// </summary>
        private static int peopleCount = 0;

        /// <summary>
        /// Количество рядов.
        /// </summary>
        private static int rows;

        /// <summary>
        /// Количество мест в ряду.
        /// </summary>
        private static int columns;

        /// <summary>
        /// Матрица мест.
        /// </summary>
        private static Place[,] places;

        /// <summary>
        /// Максимальное количество попыток ввести пароль.
        /// </summary>
        private const int ATTEMPTS_TO_INPUT_PASSWORD = 5;

        /// <summary>
        /// Служебная команда на случай забытого пароля.
        /// </summary>
        private const string COMMAND_FOR_FORGOTTEN_PASSWORD = "!forgot";

        /// <summary>
        /// Точка входа в программу.
        /// </summary>
        private static void Main()
        {
            try
            {
                Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
                Console.ResetColor();
                var title = Console.Title = (Assembly.GetEntryAssembly()?.GetName().Name ?? "My_Cinema").ToUpper();
                OutputMessage($"ПРОГРАММА:  \"{title}\"", MessageType.Title);
                OutputMessage($"Приветствуем, {Environment.UserName}!{Environment.NewLine}", MessageType.Title);

                OutputMessage($"Введите пароль. У Вас есть {ATTEMPTS_TO_INPUT_PASSWORD} попыток ввести пароль.", MessageType.Info);
                OutputMessage($"Если вы забыли пароль, введите команду \"{COMMAND_FOR_FORGOTTEN_PASSWORD}\".", MessageType.Info);
                var password = GetPassword();
                for (int attemptsCount = 0; attemptsCount < ATTEMPTS_TO_INPUT_PASSWORD; attemptsCount++)
                {
                    OutputMessage($"Введите пароль или команду \"{COMMAND_FOR_FORGOTTEN_PASSWORD}\": ", MessageType.Info, false);
                    var input = Console.ReadLine();
                    if (input == COMMAND_FOR_FORGOTTEN_PASSWORD)
                    {
                        try
                        {
                            ChangePassword(out password);
                            OutputMessage("Новый пароль успешно задан.", MessageType.Success);
                        }
                        catch (FormatException e)
                        {
                            OutputMessage(e.Message, MessageType.Error);
                        }
                    }
                    else if (input != password)
                    {
                        if (attemptsCount != ATTEMPTS_TO_INPUT_PASSWORD - 1)
                        {
                            OutputMessage($"Пароль неверный. Введите пароль еще раз или введите команду \"{COMMAND_FOR_FORGOTTEN_PASSWORD}\".{Environment.NewLine}У Вас осталось {ATTEMPTS_TO_INPUT_PASSWORD - attemptsCount - 1} попыток.", MessageType.Error);
                        }
                        else
                        {
                            OutputMessage($"Попытки ввести пароль кончились.{Environment.NewLine}Выход из программы.", MessageType.Error);
                            return;
                        }
                    }
                    else
                    {
                        OutputMessage("Пароль верный.", MessageType.Success);
                        break;
                    }
                }
                GetRowsAndColumnsNumber();
                OutputMessage($"{Environment.NewLine}Зал размером {rows}x{columns} создан.", MessageType.Success);

                RepeatingBlock();
            }
            catch (Exception e)
            {
                OutputMessage(Environment.NewLine + e.Message, MessageType.Error);
            }
            finally
            {
                OutputMessage(Environment.NewLine + Environment.NewLine + "Приложение завершено!", MessageType.Success);
            }
        }

        /// <summary>
        /// Максимальный возможный размер зала (количество рядов или количество мест в ряду).
        /// </summary>
        private const int MAX_SIZE = 9999;

        /// <summary>
        /// Минимальный возможный размер зала (количество рядов или количество мест в ряду)
        /// </summary>
        private const int MIN_SIZE = 1;

        /// <summary>
        /// Вспомогательный метод для получения размеров зала различными способами, также выбора этих способов.
        /// </summary>
        private static void GetRowsAndColumnsNumber()
        {
            OutputMessage(Environment.NewLine + "Выберите, каким способом вы хотите задать размеры зала.", MessageType.Title);
            OutputMessage("1. Ввести в консоль вручную.", MessageType.Info);
            OutputMessage("2. Задать рандомно.", MessageType.Info);
            OutputMessage("3. Получить размеры в качестве аргументов командной строки.", MessageType.Info);
            OutputMessage("4. Получить размеры зала из заданного файла.", MessageType.Info);
            OutputMessage("Номер способа: ", MessageType.Info, false);
            switch (RemoveSpacesFromString(Console.ReadLine()))
            {
                case "1":
                {
                    OutputMessage(
                        $"{Environment.NewLine}Введите размеры зала через пробел (сначала ряды затем количество мест в ряду). Размеры должны быть больше, чем {MIN_SIZE} и меньше, чем {MAX_SIZE}{Environment.NewLine}Размеры: ",
                        MessageType.Info, false);
                    if (!TryParseTwoNumbers(Console.ReadLine(), out rows, out columns) || columns < MIN_SIZE ||
                        rows < MIN_SIZE || columns > MAX_SIZE || rows > MAX_SIZE)
                        throw new FormatException(
                            $"Некорректный формат ввода размеров зала.{Environment.NewLine}Размеры зала должны быть больше, чем {MIN_SIZE} и меньше, чем {MAX_SIZE}.");
                    break;
                }
                
                case "2":
                {
                    OutputMessage(
                        $"{Environment.NewLine}Введите границы рандомизации через пробел.{Environment.NewLine}Не забывайте, что размеры зала должны быть больше, чем {MIN_SIZE} и меньше, чем {MAX_SIZE}.",
                        MessageType.Info);
                    OutputMessage("Границы: ", MessageType.Info, false);
                    if (!TryParseTwoNumbers(Console.ReadLine(), out int lower, out int higher) || lower < MIN_SIZE ||
                        higher < MIN_SIZE || lower > MAX_SIZE + 1 || higher > MAX_SIZE + 1 || lower > higher)
                        throw new ArgumentException("Некорректные границы рандома.");
                    var rnd = new Random();
                    rows = rnd.Next(lower, higher);
                    columns = rnd.Next(lower, higher);
                    break;
                }
                
                case "3":
                {
                    var commandLineArgs = Environment.GetCommandLineArgs();
                    if (commandLineArgs is null || !commandLineArgs.Any() || commandLineArgs.Length != 3 || commandLineArgs.Any(string.IsNullOrWhiteSpace))
                        throw new FormatException("Некорректные аргументы командной строки.");
                    if (!int.TryParse(commandLineArgs[1], out rows) || !int.TryParse(commandLineArgs[2], out columns) ||
                        columns < MIN_SIZE || rows < MIN_SIZE || columns > MAX_SIZE || rows > MAX_SIZE)
                        throw new FormatException(
                            $"Некорректный формат ввода размеров зала.{Environment.NewLine}Размеры зала должны быть больше, чем {MIN_SIZE} и меньше, чем {MAX_SIZE}.");
                    break;
                }

                case "4":
                {
                    OutputMessage($"{Environment.NewLine}Введите путь к файлу.{Environment.NewLine}В файле должны содержаться два числа на разных строках. Сначала количество рядов, затем количество мест в ряду.{Environment.NewLine}Путь: ", MessageType.Info, false);
                    var path = Console.ReadLine();

                    if (!File.Exists(path))
                        throw new ArgumentException("Такого файла не существует.");

                    using var sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.UTF8);

                    int count = 0;
                    var sizesPairLineBuilder = new StringBuilder(string.Empty);
                    while (!sr.EndOfStream && count <= 2)
                    {
                        var current = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            sizesPairLineBuilder.Append(current + " ");
                            count++;
                        }
                    }

                    if (count != 2)
                        throw new FormatException("Некорректный формат файла. Чисел должно быть ровно два.");

                    if (!TryParseTwoNumbers(sizesPairLineBuilder.ToString(), out rows, out columns) || columns < MIN_SIZE || rows < MIN_SIZE || columns > MAX_SIZE || rows > MAX_SIZE)
                        throw new FormatException($"Некорректный формат ввода размеров зала.{Environment.NewLine}Размеры зала должны быть больше, чем {MIN_SIZE} и меньше, чем {MAX_SIZE}.");

                     break;
                }
                
                default:
                    throw new ArgumentException("Такого варианта получения размеров зала нет.");
            }
        }

        /// <summary>
        /// Команда для выхода (вводится во время выбора команды).
        /// </summary>
        private const string EXIT_COMMAND_NUMBER = "99";

        /// <summary>
        /// Основной метод программы с циклом работы кинотеатра.
        /// </summary>
        private static void RepeatingBlock()
        {
            places = new Place[rows, columns];
            
            string command = default;
            bool check = true;
            do
            {
                try
                {
                    if (check)
                        check = places.Cast<Place>().Any(place => place is null);
                    
                    OutputMessage(Environment.NewLine + "Доступные команды:", MessageType.Title);
                    OutputMessage("1. Если Вы хотите новую задать матрицу цен.", MessageType.Info);
                    OutputMessage("2. Если Вы хотите вывести текущие цены.", MessageType.Info);
                    OutputMessage("3. Если Вы хотите изменить цену на месте (i, j), где i - ряд, а j - места.", MessageType.Info);
                    OutputMessage("4. Если Вы хотите впустить нового человека.", MessageType.Info);
                    OutputMessage("5. Если Вы хотите узнать текущую выручку.", MessageType.Info);
                    OutputMessage("6. Если Вы хотите вывести заданную сумму из кассы.", MessageType.Info);
                    OutputMessage("7. Если Вы хотите снять из кассы все имеющиеся деньги.", MessageType.Info);
                    OutputMessage("8. Если Вы хотите освободить место (i, j), где i - ряд, а j - места.", MessageType.Info);
                    OutputMessage("9. Если Вы хотите посмотреть, на каких местах уже сидят клиенты.", MessageType.Info);
                    OutputMessage("10. Если Вы хотите дополнительно пополнить количество денег в кассе на заданную сумму.", MessageType.Info);
                    OutputMessage("11. Если вы хотите увидеть количество обслуженных клиентов.", MessageType.Info);
                    OutputMessage($"{EXIT_COMMAND_NUMBER}. Если Вы хотите выйти из приложения.", MessageType.Info);
                    OutputMessage(Environment.NewLine + "Введите номер выбранной команды: ", MessageType.Info, false);

                    command = RemoveSpacesFromString(Console.ReadLine());
                    if (check && command != "1" && command != EXIT_COMMAND_NUMBER)
                    {
                        //Console.Clear();
                        OutputMessage($"{Environment.NewLine}Сначала необходимо полностью задать матрицу цен.{Environment.NewLine}Она пока что отсутствует или же не заданы какие-то конкретные места.", MessageType.Error);
                        OutputMessage("Автоматическое переключение на команду 1.", MessageType.Info);
                        command = "1";
                    }
                    Console.WriteLine();
                    
                    switch (command)
                    {
                        case "1":
                            places = MakeNewMatrixOfPrices();
                            break;

                        case "2":
                            OutputGeneralMatrixOfPrices();
                            break;
        
                        case "3":
                        {
                            OutputMessage(
                                "Введите \"координаты\" места через пробел (сначала ряд, затем место) (нумерация начинается с нуля): ",
                                MessageType.Info, false);
                            if (!TryParseTwoNumbers(Console.ReadLine(), out int row, out int column))
                                throw new FormatException("Некорректные символы.");
                            OutputMessage(Environment.NewLine + "Введите новую цену: ", MessageType.Info,
                                false);
                            if (!int.TryParse(RemoveSpacesFromString(Console.ReadLine()), out int newPrice))
                                throw new FormatException("Некорректные символы.");
                            Console.WriteLine();
                            ChangePrice(row, column, newPrice);
                            break;
                        }
                        
                        case "4":
                            OutputMessage("Обязательно поздоровайтесь с посетителем!", MessageType.Info);
                            InteractingWithClient();
                            break;

                        case "5":
                            OutputMessage($"Текущая выручка заведения: {totalProfit} руб.",
                                MessageType.Info);
                            break;
    
                        case "6":
                        {
                            OutputMessage("Введите сумму, которую Вы хотите снять: ", MessageType.Info,
                                false);
                            if (!int.TryParse(RemoveSpacesFromString(Console.ReadLine()), out int amount) ||
                                amount <= 0 || amount > totalProfit)
                                throw new ArgumentException("Суммая для снятия задана некорректно.");
                            OutputMessage(
                                $"Сумма размером {amount} руб снята из кассы. В кассе осталось {totalProfit -= amount} руб.",
                                MessageType.Success);
                            break;
                        }
                        
                        case "7":
                            OutputMessage(
                                $"Выручка размером {totalProfit} руб. полностью снята из кассы. Касса пуста.",
                                MessageType.Success);
                            totalProfit = 0;
                            break;

                        case "8":
                        {
                            OutputMatrixOfPlacesForAdmin();
                            OutputMessage("Введите через пробел \"координаты\" места (сначала ряд, затем место): ", MessageType.Info,
                                false);
                            if (!TryParseTwoNumbers(Console.ReadLine(), out int row, out int column) || row < 0 || column < 0 ||
                                row >= places.GetLength(0) || column >= places.GetLength(1))
                                throw new FormatException("Некорректные символы или такого места нет.");
                            OutputMessage(
                                $"{Environment.NewLine}Место ({row}; {column}) {(places[row, column].IsFree ? "итак свободно" : "освобождено")}.",
                                MessageType.Success);
                            places[row, column].MakeFree();
                            break;
                        }
                        
                        case "9":
                            OutputMatrixOfPlacesForAdmin();
                            break;
                        
                        case "10":
                        {
                            OutputMessage("Введите сумму, которую Вы хотите добавить: ", MessageType.Info,
                                false);
                            if (!int.TryParse(RemoveSpacesFromString(Console.ReadLine()), out int amount) || amount <= 0)
                                throw new ArgumentException("Суммая для пополнения задана некорректно.");
                            OutputMessage(
                                $"{Environment.NewLine}Сумма размером {amount} руб добавлена в кассу. В кассе теперь {totalProfit += amount} руб.",
                                MessageType.Success);
                            break;
                        }
                        
                        case "11":
                            OutputMessage($"Количество обслуженных клиентов: {peopleCount}.", MessageType.Info);
                            break;
                        
                        case EXIT_COMMAND_NUMBER:
                            break;
                        
                        default:
                            throw new ArgumentException("Извините, но команды с таким номером нет.");
                    }
                }
                catch (FormatException e)
                {
                    OutputMessage(Environment.NewLine + e.Message, MessageType.Error);
                }
                catch (ArgumentException e)
                {
                    OutputMessage(Environment.NewLine + e.Message, MessageType.Error);
                }
            } while (command != EXIT_COMMAND_NUMBER);
        }

        /// <summary>
        /// Метод задания новой матрицы цен.
        /// </summary>
        /// <returns> Матрица цен. </returns>
        private static Place[,] MakeNewMatrixOfPrices()
        {
            //Console.Clear();
            OutputMessage(Environment.NewLine + "Выберите, как будете создавать матрицу цен.", MessageType.Info);
            OutputMessage(Environment.NewLine + "1. Ввод в консоль вручную.", MessageType.Info);
            OutputMessage($"2. Рандомная генерация от минимальной цены: {Place.MIN_PRICE} до максимальной: {Place.MAX_PRICE} (не включительно).", MessageType.Info);
            OutputMessage("3. Рандомная генерация с заданными вручную границами.", MessageType.Info);
            OutputMessage(Environment.NewLine + "Введите свой выбор: ", MessageType.Info, false);
            
            var choice = RemoveSpacesFromString(Console.ReadLine());
            Console.WriteLine();
            
            if (choice == "1")
                return ReadMatrixOfPrices();

            if (choice == "2")
                return RandomGenerateMatrix(Place.MIN_PRICE, Place.MAX_PRICE + 1);

            if (choice == "3")
            {
                OutputMessage($"Введите через пробел границы рандомной генерации.{Environment.NewLine}Можете просто ввести два нуля, тогда границы будут заданы в программе.{Environment.NewLine}По умолчанию полуинтервал рандома [1; 1000).", MessageType.Info);
                
                if (!TryParseTwoNumbers(Console.ReadLine(), out int lowerBound, out int upperBound))
                    throw new FormatException("Некорректные символы.");

                if (lowerBound != 0 || upperBound != 0)
                    return RandomGenerateMatrix(lowerBound, upperBound);

                OutputMessage(Environment.NewLine + "Значения будут заданы по умолчанию.", MessageType.Info);
                return RandomGenerateMatrix();
            }
            
            throw new FormatException("Некорректный выбор.");
        }

        /// <summary>
        /// Поменять цену на заданное место.
        /// </summary>
        /// <param name="row"> Номер ряда. </param>
        /// <param name="column"> Номер места в ряду. </param>
        /// <param name="newPrice"> Новая цена. </param>
        private static void ChangePrice(int row, int column, int newPrice)
        {
            if (row < 0 || column < 0 || row >= places.GetLength(0) || column >= places.GetLength(1))
                throw new FormatException("Такого места нет.");

            places[row, column].SetNewPrice(newPrice);
        }

        /// <summary>
        /// Шаг юбилейных клиентов.
        /// </summary>
        private const int PRIZE_CLIENT_STEP = 100;

        /// <summary>
        /// Метод взаимодействия с клиентом.
        /// </summary>
        private static void InteractingWithClient()
        {
            //Console.Clear();
            OutputMessage($"{Environment.NewLine}Для начала введите бюджет клиента.{Environment.NewLine}Бюджет: ", MessageType.Info, false);

            if (!int.TryParse(RemoveSpacesFromString(Console.ReadLine()), out int budget) || budget <= 0)
                throw new FormatException("Некорректно заданный бюджет (необходимо положительное число).");

            Console.WriteLine();
            OutputMatrixOfPricesForCurrentUser(budget);
            
            if (places.Cast<Place>().All(place => !place.IsFree || place.Price > budget))
                throw new ArgumentException("Для данного клиента доступных мест нет.");

            OutputMessage(Environment.NewLine + "Выберите доступное место, которое клиент хочет купить(сначала ряд, затем, через пробел место (нумерация начинается с 0)).", MessageType.Info);
            if (!TryParseTwoNumbers(Console.ReadLine(), out int row, out int column) || row < 0 || column < 0 || row >= places.GetLength(0) || column >= places.GetLength(1)) 
                throw new ArgumentException("Такого места нет.");

            if (!places[row, column].IsFree || places[row, column].Price > budget)
                throw new ArgumentException("Это место взять нельзя.");

            places[row, column].MakeBusy();
            totalProfit += places[row, column].Price;
            peopleCount++;
            
            OutputMessage($"{Environment.NewLine}Место ({row}; {column}) выкуплено.{Environment.NewLine}В кассу поступило {places[row, column].Price} руб.", MessageType.Success);
            
            if (peopleCount % PRIZE_CLIENT_STEP == 0)
                OutputMessage($"УРА!!! ОБСЛУЖЕН ЮБИЛЕЙНЫЙ {peopleCount}-Й КЛИЕНТ!!!", MessageType.Success);
        }

        /// <summary>
        /// Метод считывания матрицы из консоли.
        /// </summary>
        /// <returns> Считанная матрица. </returns>
        private static Place[,] ReadMatrixOfPrices()
        {
            var pricesMatrix = new Place[rows, columns];
            //Console.Clear();
            OutputMessage(Environment.NewLine + "Начинайте ввод. После каждой строки нажимайте клавишу \"Enter\".", MessageType.Info);
            for (int i = 0; i < pricesMatrix.GetLength(0); i++)
            {
                var inputLine = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(inputLine))
                    throw new FormatException($"{i}-я строка матрицы цен пустая.");
                
                // Сборка i-й строки матрицы цен из введенных в консоль данных.
                var pricesLine = inputLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(priceStr =>
                {
                    if (!int.TryParse(priceStr, out int price) || price < Place.MIN_PRICE || price > Place.MAX_PRICE)
                        throw new FormatException("Некорректный(е) элемент(ы) матрицы.");

                    return price;
                }).ToArray();

                if (pricesLine.Length != pricesMatrix.GetLength(1))
                    throw new FormatException($"Матричная строка должна быть длины {pricesMatrix.GetLength(1)}. Длина {pricesLine.Length} недопустима.");

                for (int j = 0; j < pricesMatrix.GetLength(1); j++)
                    pricesMatrix[i, j] = new Place(pricesLine[j]);
            }
            return pricesMatrix;
        }

        /// <summary>
        /// Рандомная генерация матрицы мест.
        /// </summary>
        /// <param name="lowerBound"> Нижняя граница рандомизации цены. </param>
        /// <param name="upperBound"> Верхняя граница рандомизации цены. </param>
        /// <returns> Рандомно сгенерированная матрица мест. </returns>
        private static Place[,] RandomGenerateMatrix(int lowerBound = 1, int upperBound = 1000)
        {
            if (lowerBound > upperBound)
                throw new ArgumentException("Нижняя граница рандомной генерации цен не может быть больше верхней.");

            if (lowerBound < Place.MIN_PRICE || upperBound < Place.MIN_PRICE)
                throw new ArgumentException($"Минимальная возможная цена на место: {Place.MIN_PRICE}руб.");

            if (upperBound > Place.MAX_PRICE || lowerBound > Place.MAX_PRICE)
                throw new ArgumentException($"Максимальная возможная цена на место: {Place.MAX_PRICE}руб.");

            var placesMatrix = new Place[rows, columns];

            var rnd = new Random();

            for (int i = 0; i < placesMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < placesMatrix.GetLength(1); j++)
                    placesMatrix[i, j] = new Place(rnd.Next(lowerBound, upperBound));
            }

            OutputMessage(Environment.NewLine + "Матрица сгенерированна.", MessageType.Success);

            return placesMatrix;
        }

        /// <summary>
        /// Метод вывода матрицы цен в консоль.
        /// </summary>
        private static void OutputGeneralMatrixOfPrices()
        {
            var format = "{0, " + Place.MAX_PRICE.ToString().Length + "}\t";
            
            for (int i = 0; i < places.GetLength(0); i++)
            {
                for (int j = 0; j < places.GetLength(1); j++)
                    OutputMessage(string.Format(format, places[i, j]), MessageType.None, false);
                
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Знак свободного места в выводе матрицы мест.
        /// </summary>
        private const char AVAILABLE_PLACE_SIGN = '0';

        /// <summary>
        /// Знак недоступного места в выводе матрицы мест.
        /// </summary>
        private const char UNAVAILABLE_PLACE_SIGN = 'x';

        /// <summary>
        /// Метод вывода в консоль матрицы доступных мест для клиента.
        /// </summary>
        /// <param name="budget"> Бюджет клиента. </param>
        private static void OutputMatrixOfPricesForCurrentUser(int budget)
        {
            //Console.Clear();
            OutputMessage($"{Environment.NewLine}Доступные места ({AVAILABLE_PLACE_SIGN} - место свободно, {UNAVAILABLE_PLACE_SIGN} - место выкуплено или недоступно).", MessageType.Info);
            
            for (int i = 0; i < places.GetLength(0); i++)
            {
                for (int j = 0; j < places.GetLength(1); j++)
                    OutputMessage((places[i, j].IsFree && places[i, j].Price <= budget ? AVAILABLE_PLACE_SIGN.ToString() : UNAVAILABLE_PLACE_SIGN.ToString()) + "\t", MessageType.None, false);
                
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Метод вывода в консоль матрицы свободных мест для администратора.
        /// </summary>
        private static void OutputMatrixOfPlacesForAdmin()
        {
            //Console.Clear();
            OutputMessage($"{Environment.NewLine}Место занято клиентом - '{UNAVAILABLE_PLACE_SIGN}', место свободно - '{AVAILABLE_PLACE_SIGN}'.", MessageType.Info);

            for (int i = 0; i < places.GetLength(0); i++)
            {
                for (int j = 0; j < places.GetLength(1); j++)
                    OutputMessage((places[i, j].IsFree ? AVAILABLE_PLACE_SIGN.ToString() : UNAVAILABLE_PLACE_SIGN.ToString()) + "\t", MessageType.None, false);

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Вспомогательный вывод сообщений с разного рода информацией.
        /// </summary>
        /// <param name="message"> Сообщение, которое надо вывести. </param>
        /// <param name="informationType"> Тип информации для вывода (влияет на цвет вывода). </param>
        /// <param name="doNewLine"> Необходимо ли после вывода делать перенос строки. </param>>
        private static void OutputMessage(string message, MessageType informationType = MessageType.None, bool doNewLine = true)
        {
            switch (informationType)
            {
                case MessageType.Title:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;

                case MessageType.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;

                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case MessageType.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case MessageType.None:

                default:
                    Console.ResetColor();
                    break;
            }

            Console.Write((informationType == MessageType.Error ? "Произошла ошибка! " : string.Empty)
                          + message + (doNewLine ? Environment.NewLine : string.Empty));

            Console.ResetColor();
        }

        /// <summary>
        /// Вспомогательный метод на основе библиотечного метода TryParse, для преобразования строки в два числа.
        /// </summary>
        /// <param name="s"> Заданная строка (для преобразования необходимо два числа, записанные через пробел). </param>
        /// <param name="result1"> Первое число. </param>
        /// <param name="result2"> Второе число. </param>
        /// <returns> Является ли преобразование возможным и корректным. </returns>
        private static bool TryParseTwoNumbers(string s, out int result1, out int result2)
        {
            result1 = result2 = 0;

            if (string.IsNullOrWhiteSpace(s))  
                return false;

            var strPair = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return strPair.Length == 2 && int.TryParse(strPair[0], out result1) && int.TryParse(strPair[1], out result2);
        }

        /// <summary>
        /// Имя файла с паролем.
        /// </summary>
        private const string PASSWORD_FILE_NAME = "password.txt";

        /// <summary>
        /// Метод извлечения пароля из файла.
        /// </summary>
        /// <returns> Извлеченный из файла пароль. </returns>
        private static string GetPassword()
        {
            using var sr = new StreamReader(new FileStream(PASSWORD_FILE_NAME, FileMode.Open, FileAccess.Read), Encoding.UTF8);

            return sr.ReadLine();
        }

        /// <summary>
        /// Метод записи нового пароля в файл.
        /// </summary>
        /// <param name="password"> Новый пароль, который необходимо записать в файл. </param>
        private static void ChangePassword(out string password)
        {
            OutputMessage($"{Environment.NewLine}Выберите способ задать пароль.", MessageType.Title);
            OutputMessage("1. Ввести пароль вручную.", MessageType.Info);
            OutputMessage("2. Сгенерировать пароль.", MessageType.Info);
            OutputMessage("Введите свой выбор: ", MessageType.Info, false);
            var choose = RemoveSpacesFromString(Console.ReadLine());
            Console.WriteLine();
            
            switch (choose)
            {
                case "1":
                    OutputMessage($"{Environment.NewLine}Введите новый пароль: ", MessageType.Info, false);
                    password = Console.ReadLine();
                    break;
                case "2":
                    OutputMessage($"Пароль успешно сгенерирован.{Environment.NewLine}Новый пароль: {password = GenerateRandomPassword()}.", MessageType.Success);
                    break;
                default:
                    throw new ArgumentException("Некорректный выбор способа задать пароль.");
            }

            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(password) || password == COMMAND_FOR_FORGOTTEN_PASSWORD)
                throw new ArgumentException($"Новый пароль задан некорректно.{Environment.NewLine}Пароль не может быть пустой/пробельной строкой или зарезервированной командой \"{COMMAND_FOR_FORGOTTEN_PASSWORD}\".");

            using var sw = new StreamWriter(new FileStream(PASSWORD_FILE_NAME, FileMode.Create, FileAccess.Write), Encoding.UTF8);

            sw.WriteLine(password);
        }

        /// <summary>
        /// Вспомогательный метод, убирающий лишние пробелы из заданной строки.
        /// </summary>
        /// <param name="s"> Заданная строка. </param>
        /// <returns> Заданная строка, с удаленными пробелами. </returns>
        private static string RemoveSpacesFromString(string s) =>
            new string((from ch in s
                        where ch != ' '
                        select ch).ToArray());

        private static string GenerateRandomPassword()
        {
            var rand = new Random();
            StringBuilder passwordBuilder;
            do
            {
                passwordBuilder = new StringBuilder(string.Empty);
                var passwordLength = rand.Next(3, 10);
                for (int i = 0; i < passwordLength; i++)
                {
                    passwordBuilder.Append(rand.Next(0, 3) switch
                    {
                        0 => (char) rand.Next('a', 'z' + 1),
                        1 => (char) rand.Next('A', 'Z' + 1),
                        2 => rand.Next(0, 10),
                        _ => string.Empty
                    });
                }
            } while (passwordBuilder.ToString() == COMMAND_FOR_FORGOTTEN_PASSWORD);

            return passwordBuilder.ToString();
        }
    }
}
