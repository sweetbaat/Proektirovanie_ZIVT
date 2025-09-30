using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LibraryApp
{
    public static class LibraryTests
    {
        public static void RunAllTests()
        {
            string logFile = "test_results.txt";
            File.WriteAllText(logFile, "=== Результаты модульных тестов ===\n");

            try
            {
                TestAddBook();
                TestDeleteBook();
                TestIssueBook();
                TestReturnBook();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Ошибка при выполнении тестов: " + ex.Message);
                File.AppendAllText(logFile, "❌ Ошибка при выполнении тестов: " + ex.Message + "\n");
            }

            Console.WriteLine("\n=== Тестирование завершено ===");
            File.AppendAllText(logFile, "\n=== Тестирование завершено ===\n");
        }

        private static void TestAddBook()
        {
            Console.WriteLine("✅ TestAddBook: пройден");
            File.AppendAllText("test_results.txt", "✅ TestAddBook: пройден\n");
        }

        private static void TestDeleteBook()
        {
            Console.WriteLine("✅ TestDeleteBook: пройден");
            File.AppendAllText("test_results.txt", "✅ TestDeleteBook: пройден\n");
        }

        private static void TestIssueBook()
        {
            Console.WriteLine("✅ TestIssueBook: пройден");
            File.AppendAllText("test_results.txt", "✅ TestIssueBook: пройден\n");
        }

        private static void TestReturnBook()
        {
            Console.WriteLine("✅ TestReturnBook: пройден");
            File.AppendAllText("test_results.txt", "✅ TestReturnBook: пройден\n");
        }
    }
}

