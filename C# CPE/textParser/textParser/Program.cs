namespace textParser
{

    public class textParser
    {
        public static void Main(string[] args)
        {
            TextParser parser = new TextParser();
            Text text = parser.ParseText("HowBuildTheCar.txt");

            Console.WriteLine($"Количество предложений: {text.Sentences.Count}");

            //    for (int i = 0; i < text.Sentences.Count; i++) {
            //    Console.WriteLine("\n Предложение " + (i + 1) + ": ");
            //    Console.WriteLine(text.Sentences[i].GetSentence());
            //    Console.WriteLine("Слов: " + text.Sentences[i].CountOfWordsInSentense);
            //    Console.WriteLine("Токенов: " + text.Sentences[i].Tokens.Count);
            //}
            TextFunction functions = new TextFunction(text);
            //1
            Console.WriteLine("\n Сортировка по кол-ву слов");
            var sortedByWords = functions.SortByCountWord();
            foreach (var sentence in sortedByWords)
            {
                Console.WriteLine("Слов:" + sentence.CountOfWordsInSentense + "| " + sentence.GetSentence());
            }
            Console.WriteLine("\n Сортировка по длине предложения");
            //2
            var sortedByLength = functions.SortByLenghtSentence();
            foreach (var sentence in sortedByLength)
            {
                Console.WriteLine("Длина: " + sentence.GetSentence().Length + "| " + sentence.GetSentence());
            }
            // 8 Конкорданс
            functions.PrintConcordance();
            //3
            Console.WriteLine("\nCлова длинной (num) в предложениях с вопросом");
            int num = 0;
            do
            {
                Console.Write("Введите число: ");
                num = int.Parse(Console.ReadLine());
            }
            while (num <= 0);
            var questionWords = functions.FindLenghtWordsInQuestions(num);
            if (questionWords.Count > 0)
            {
                foreach (var word in questionWords)
                {
                    Console.WriteLine("Слово: " + word);
                }
            }
            else
            {
                Console.WriteLine("Вопросительных предложений не найдено");
            }
            //4
            Console.WriteLine("\nУдалить согласную букву из слов длины (n) ");
            int n = 0;
            do
            {
                Console.Write("Введите длину слов для удаления: ");
                n = int.Parse(Console.ReadLine());
            }
            while (n <= 0);
            Console.WriteLine("До удаления: " + text.Sentences[0].GetSentence());
            functions.RemoveWordsWithSoglasnye(n);
            Console.WriteLine("После удаления: " + text.Sentences[0].GetSentence());
            //5 
            Console.WriteLine("\nЗамена слов");
            Console.WriteLine("До замены: " + text.Sentences[1].GetSentence());
            functions.ReplaceWordsInSentence(1, 6, "WOOOW ");
            Console.WriteLine("После замены: " + text.Sentences[1].GetSentence());
            //6
            Console.WriteLine("\nУдаление стоп-слов");
            functions.RemoveStopWords("StopWords.txt");
            foreach (var sentence in text.Sentences)
            {
                Console.Write(sentence.GetSentence());
            }
            //7
            Console.WriteLine("\n экспорт в XML");
            functions.ToXml("text.xml");
            Console.WriteLine("готово");

        }
    }
}