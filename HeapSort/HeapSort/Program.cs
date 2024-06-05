using System;
using System.Xml;


namespace Лаб2_Сортировка_чисел
{
    public class Swapper
    {
        public int branchesCount;
        private int swapCount;

        public int GetSwapCount()
        {
            return swapCount;
        }

        public void Swap<T>(ref T first, ref T second)
        {
            var temp = first;
            first = second;
            second = temp;
            swapCount++;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Тестирование, если хоть один тест провален, программа завершается
            if (!TestHeapSort())
                return;


            //Выполнение эксперимента
            Console.Write("Выполнить эксперимент (да/нет)?");
            var answer = Console.ReadLine();
            if (answer == "да")
            {
                GetResultsFromXML();
                Console.WriteLine("Запись в файл закончена");
            }
        }

        //Метод сортировки
        public static int HeapSort(int[] array)
        {
            //Создаём объект класса Swapper, для получения доступа к методу Swap() и подсчёту кол-во swap
            var swapper = new Swapper();

            var heapSize = array.Length;
            //Тут происъодит начальное построение "дерева"
            for (var i = heapSize / 2 - 1; i >= 0; i--)
                Heapify(array, heapSize, i, swapper);
            for (var i = heapSize - 1; i >= 0; i--)
            {
                //выносим макс элемент в ту часть массива,
                //куда складываем результирующий массив(отсортированная часть)
                swapper.Swap(ref array[0], ref array[i]);
                //После того, как перенесли макс элемент, стабилизируем дерево вновь
                Heapify(array, i, 0, swapper);
            }

            //в конце возвращаем кол-во swap
            return swapper.GetSwapCount();
        }

        //Метод перестройки "дерева"
        public static void Heapify(int[] arr, int length, int i, Swapper swapper)
        {
            //инициализируем largest - корень, left, right - потомки
            var largest = i;
            var left = 2 * i + 1;
            var right = 2 * i + 2;
            // Если левый потомок больше корня, то он - должен стать корнем
            if (left < length && arr[left] > arr[largest])
                largest = left;
            // Если правый потомок больше корня, то он - должен стать корнем
            if (right < length && arr[right] > arr[largest])
                largest = right;
            //Если корень поменялся
            if (largest != i)
            {
                //То меняем его положение в дереве
                swapper.Swap(ref arr[i], ref arr[largest]);
                //Повторяем процедуру перестройки дерева, пока не построим
                Heapify(arr, length, largest, swapper);
            }
            swapper.branchesCount += 3;
        }

        //Метод работы с XML и печати результатов в файл
        private static void GetResultsFromXML()
        {
            //создаем оюъкт класса xmlDocument
            var xmlDoc = new XmlDocument();
            //Загружаем в него наш xml
            xmlDoc.Load(@"C:\Users\maxxx\OneDrive\Рабочий стол\AlgoritmLab1\Experiments.xml");
            //Получаем из него корневой элемент experiment, в котором описана массовая задача
            var xmlRootNode = xmlDoc.DocumentElement;
            var experiment = xmlRootNode.SelectSingleNode("experiment");
            //Создаем writer,чтобы записывать результаты в файл, который будет находиться в projectDirectory
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var writer = new StreamWriter(Path.Combine(projectDirectory, "ExperimentResults.txt"));


            using (writer)
            {
                //проходимся по nodes
                foreach (XmlNode node in experiment.ChildNodes)
                {
                    //Парсим некоторые общие данные из nodes
                    var startLength = int.Parse(node.SelectSingleNode("@startLength").Value);
                    var repeat = int.Parse(node.SelectSingleNode("@repeat").Value);
                    var name = node.SelectSingleNode("@name").Value;
                    //Далее в зависимости от name nodes, получаем доп данные
                    if (name == "Arithmetic Progression")
                    {
                        var minElement = int.Parse(node.SelectSingleNode("@minElement").Value);
                        var maxElement = int.Parse(node.SelectSingleNode("@maxElement").Value);
                        var diff = int.Parse(node.SelectSingleNode("@diff").Value);
                        //var maxElement = startLength + repeat * diff;
                        var length = startLength;
                        for (int i = 0; i < repeat; i++)
                        {
                            length = startLength + i * diff;
                            //генерируем массивы и получаем из них swap count, сортируя их
                            var operationsCount = GenerateArrAndGetCount(minElement, maxElement, length);
                            //записываем, что нужно в файл
                            writer.WriteLine(length + "\t" + operationsCount);
                        }
                    }
                    //Тут всё то же самое, что и выше
                    if (name == "Bad Data")
                    {
                        writer.WriteLine("Bad Data");
                        for (int i = 0; i < repeat; i++)
                        {
                            var minElement = int.Parse(node.SelectSingleNode("@minElement").Value);
                            var length = startLength + i;
                            //только метод создания массивов и получения из них swapCount другой
                            var operationsCount = GenerateBadDataAndGetCount(minElement, length);
                            writer.WriteLine(length + "\t" + operationsCount);
                        }
                    }
                    //Тут всё то же самое, что и выше
                    if (name == "Good Data")
                    {
                        writer.WriteLine("Good Data");
                        for (int i = 0; i < repeat; i++)
                        {
                            var maxElement = int.Parse(node.SelectSingleNode("@maxElement").Value);
                            var length = startLength + i;
                            //только метод создания массивов и получения из них swapCount другой
                            var operationsCount = GenerateGoodDataAndGetCount(maxElement, length);
                            writer.WriteLine(length + "\t" + operationsCount);
                        }
                    }
                }
            }
        }
        //Метод генерации массивов и получения swapCount после их сортировки
        private static int GenerateArrAndGetCount(int minElement, int maxElement, int arrayLength)
        {
            //создаём массив
            var array = new int[arrayLength];
            //заполняем его рандомными элементам в промежутке от minElement до maxElement
            var random = new Random();
            for (var j = 0; j < array.Length; j++)
                array[j] = random.Next(minElement, maxElement);
            //сортируем массив и возвращаем swapCount
            return HeapSort(array);
        }
        //Метод генерации массивов и получения swapCount после их сортировки
        private static int GenerateBadDataAndGetCount(int minElement, int arrayLength)
        {
            //Получаем уже отсортированный массив
            var array = GenerateSortedArray(minElement, arrayLength);
            //сортируем массив и возвращаем swapCount
            return HeapSort(array);
        }
        //Метод генерации массивов и получения swapCount после их сортировки
        private static int GenerateGoodDataAndGetCount(int maxElement, int arrayLength)
        {
            //Получаем массив отсортированный в обратном порядке
            var array = GenerateReverseSortedArray(maxElement, arrayLength);
            //сортируем массив и возвращаем swapCount
            return HeapSort(array); ;
        }

        //Создаем массив
        private static int[] GenerateSortedArray(int minElement, int arrayLength)
        {
            var array = new int[arrayLength];
            for (var i = 0; i < arrayLength; i++)
            {
                //каждый следующий элемент > предыдущего
                array[i] = minElement + i;
            }
            //отдаем отсортированный массив
            return array;
        }

        //Создаем массив
        private static int[] GenerateReverseSortedArray(int maxElement, int arrayLength)
        {
            var array = new int[arrayLength];
            for (var i = 0; i < arrayLength; i++)
            {
                //каждый следующий элемент < предыдущего
                array[i] = maxElement - i;
            }
            //отдаем отсортированный в обратном порядке массив
            return array;
        }

        //метод тестирования
        private static bool TestHeapSort()
        {
            //создаем переменную, testFinished = true, если testFinished станет false,
            //один или несколько тестов провалено
            var testFinished = true;
            {
                //тест на пустом массиве
                var startArray = new int[0];
                var expectedArray = new int[0];
                HeapSort(startArray);
                if (!Enumerable.SequenceEqual(startArray, expectedArray))
                {
                    Console.WriteLine("Test:hollow arr wasn't passed");
                    testFinished = false;
                }
            }
            {
                //тест на массиве с одним элементом
                var startArray = new[] { 1 };
                var expectedArray = new[] { 1 };
                HeapSort(startArray);
                if (!Enumerable.SequenceEqual(startArray, expectedArray))
                {
                    Console.WriteLine("Test:one element arr wasn't passed");
                    testFinished = false;
                }
            }
            {
                //тест на массиве
                var startArray = new[] { 5, 3, 10, 8, 1, 7 };
                var expectedArray = new[] { 1, 3, 5, 7, 8, 10 };
                HeapSort(startArray);
                if (!Enumerable.SequenceEqual(startArray, expectedArray))
                {
                    Console.WriteLine("Test:common arr wasn't passed");
                    testFinished = false;
                }
            }
            {
                //тест на массиве, в котором есть повторяющиеся элементы
                var startArray = new[] { 1, 0, 0, 1 };
                var expectedArray = new[] { 0, 0, 1, 1 };
                HeapSort(startArray);
                if (!Enumerable.SequenceEqual(startArray, expectedArray))
                {
                    Console.WriteLine("Test:repeated elements arr wasn't passed");
                    testFinished = false;
                }
            }
            //возвращаем прошли ли все тесты
            return testFinished;
        }
    }
}