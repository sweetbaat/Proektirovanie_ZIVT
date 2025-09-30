using LibraryApp;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Windows.Forms;


public static class LibraryDB
{
    private static string connectionString = "Data Source=library.db;Version=3;";

    //создание калькулятора на штраф
    public static double CalculatePenalty(DateTime expectedReturnDate, string userCategory)
    {
        if (userCategory == "Преподаватель") return 0;

        DateTime today = DateTime.Now;
        if (today <= expectedReturnDate) return 0;

        int daysLate = (today - expectedReturnDate).Days;
        double penaltyAmount = daysLate * 10; // 10 руб./день
        return penaltyAmount;
    }


    // Получение пользователя по логину, паролю и категории
    public static User GetUser(string name, string password, string category)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var cmd = new SQLiteCommand(
                "SELECT * FROM Users WHERE Name=@name AND Password=@password AND Category=@category", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@category", category);

            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Category = reader["Category"].ToString(),
                    MaxBooks = Convert.ToInt32(reader["MaxBooks"]),
                    Password = reader["Password"].ToString()
                };
            }
            return null;
        }
    }

    // Регистрация нового пользователя
    public static void AddUser(User user)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var cmd = new SQLiteCommand(
                "INSERT INTO Users (Name, Category, MaxBooks, Password) VALUES (@name, @category, @maxBooks, @password)", conn);
            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@category", user.Category);
            cmd.Parameters.AddWithValue("@maxBooks", user.MaxBooks);
            cmd.Parameters.AddWithValue("@password", user.Password);
            cmd.ExecuteNonQuery();
        }
    }

    // Получение книги по ID
    public static Book GetBook(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT * FROM Books WHERE Id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Book
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Title = reader["Title"].ToString(),
                            Author = reader["Author"].ToString(),
                            Year = Convert.ToInt32(reader["Year"]),
                            Status = reader["Status"].ToString()
                        };
                    }
                }
            }
        }
        return null;
    }




    // Выдача книги пользователю
    //public static void IssueBook(Issue issue)
    //{
    //    using (var conn = new SQLiteConnection(connectionString))
    //    {
    //        conn.Open();
    //        var cmd = new SQLiteCommand(
    //            "INSERT INTO Issues (UserId, BookId, IssueDate, ReturnDate) VALUES (@userId, @bookId, @issueDate, @returnDate)", conn);
    //        cmd.Parameters.AddWithValue("@userId", issue.UserId);
    //        cmd.Parameters.AddWithValue("@bookId", issue.BookId);
    //        cmd.Parameters.AddWithValue("@issueDate", issue.IssueDate.ToString("yyyy-MM-dd"));
    //        cmd.Parameters.AddWithValue("@returnDate", issue.ReturnDate.ToString("yyyy-MM-dd"));
    //        cmd.ExecuteNonQuery();
    //    }
    //}

    // Обновление статуса книги
    public static void UpdateBook(Book book)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var cmd = new SQLiteCommand(
                "UPDATE Books SET Status=@status WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@status", book.Status);
            cmd.Parameters.AddWithValue("@id", book.Id);
            cmd.ExecuteNonQuery();
        }
    }

    //Возврат книги

    public static string GetUserCategory(int userId)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT Category FROM Users WHERE Id=@Id";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", userId);
                return cmd.ExecuteScalar()?.ToString();
            }
        }
    }

    public static DateTime GetExpectedReturnDate(int issueId)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT IssueDate FROM Issues WHERE Id=@Id";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", issueId);
                var issueDate = Convert.ToDateTime(cmd.ExecuteScalar());
                return issueDate.AddDays(14); // например, срок возврата 14 дней для всех
            }
        }
    }
    public static double ReturnBook(int userId, int bookId)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();

            // Находим активную выдачу
            string sql = "SELECT Id, IssueDate FROM Issues WHERE UserId=@UserId AND BookId=@BookId AND ReturnDate IS NULL";
            int issueId = 0;
            DateTime issueDate = DateTime.Now;

            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        throw new Exception("Активная запись выдачи не найдена для этой книги и пользователя!");

                    issueId = Convert.ToInt32(reader["Id"]);
                    issueDate = Convert.ToDateTime(reader["IssueDate"]);
                }
            }

            // Рассчитываем штраф
            string category = GetUserCategory(userId); // метод для получения категории пользователя
            double penalty = 0;
            if (category != "Преподаватель")
            {
                int daysLate = (DateTime.Now - issueDate).Days;
                penalty = daysLate > 0 ? daysLate * 10 : 0;
            }

            // Обновляем запись выдачи
            string updateIssue = "UPDATE Issues SET ReturnDate=@ReturnDate WHERE Id=@Id";
            using (var cmd = new SQLiteCommand(updateIssue, conn))
            {
                cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", issueId);
                cmd.ExecuteNonQuery();
            }

            // Обновляем статус книги
            string updateBook = "UPDATE Books SET Status='Свободна' WHERE Id=@BookId";
            using (var cmd = new SQLiteCommand(updateBook, conn))
            {
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.ExecuteNonQuery();
            }

            // Добавляем штраф
            if (penalty > 0)
            {
                string insertPenalty = "INSERT INTO Penalties (IssueId, Amount) VALUES (@IssueId, @Amount)";
                using (var cmd = new SQLiteCommand(insertPenalty, conn))
                {
                    cmd.Parameters.AddWithValue("@IssueId", issueId);
                    cmd.Parameters.AddWithValue("@Amount", penalty);
                    cmd.ExecuteNonQuery();
                }
            }

            return penalty;
        }
    }

    // Расчет штрафа
    public static double CalculatePenalty(Issue issue, string category)
    {
        DateTime today = DateTime.Now;
        DateTime returnDate = DateTime.MinValue;

        if (category == "Преподаватель") return 0;
        if (today <= returnDate) return 0;

        int daysLate = (today - returnDate).Days;
        double penaltyAmount = daysLate * 10; // 10 руб./день
        return penaltyAmount;
    }

    // Добавление штрафа
    public static void AddPenalty(Penalty penalty)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            var cmd = new SQLiteCommand(
                "INSERT INTO Penalties (IssueId, Amount) VALUES (@issueId, @amount)", conn);
            cmd.Parameters.AddWithValue("@issueId", penalty.IssueId);
            cmd.Parameters.AddWithValue("@amount", penalty.Amount);
            cmd.ExecuteNonQuery();
        }
    }
    public static List<Book> GetAllBooks()
    //метод просмотр каталога книг
    {
        var list = new List<Book>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT * FROM Books", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Book
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                        Author = reader["Author"].ToString(),
                        Year = Convert.ToInt32(reader["Year"]),
                        Category = reader["Category"].ToString(),
                        Status = reader["Status"].ToString()
                    });
                }
            }
        }
        return list;
    }

    //метод на добавление книги
    public static void AddBook(Book book)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "INSERT INTO Books (Title, Author, Year, Category, Status) " +
                         "VALUES (@Title, @Author, @Year, @Category, @Status)";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Title", book.Title ?? "Без названия");
                cmd.Parameters.AddWithValue("@Author", book.Author ?? "Неизвестен");
                cmd.Parameters.AddWithValue("@Year", book.Year);
                cmd.Parameters.AddWithValue("@Category", book.Category ?? "Общее");
                cmd.Parameters.AddWithValue("@Status", "Свободна");

                cmd.ExecuteNonQuery();
            } // cmd.Dispose() вызывается автоматически
        } // conn.Dispose() закрывает соединение
    }

    //метод удаления книги
    public static void DeleteBook(int bookId)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            // Проверка, что книга не выдана
            string checkSql = "SELECT Status FROM Books WHERE Id=@Id";
            using (var checkCmd = new SQLiteCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("@Id", bookId);
                var status = checkCmd.ExecuteScalar()?.ToString();
                if (status == "Выдана")
                    throw new Exception("Нельзя удалить книгу, которая выдана!");
            }

            string sql = "DELETE FROM Books WHERE Id=@Id";
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", bookId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public static void IssueBook(int userId, int bookId)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();

            // Проверяем статус книги
            string checkSql = "SELECT Status FROM Books WHERE Id=@Id";
            using (var checkCmd = new SQLiteCommand(checkSql, conn))
            {
                checkCmd.Parameters.AddWithValue("@Id", bookId);
                var status = checkCmd.ExecuteScalar()?.ToString();
                if (!string.Equals(status, "Свободна", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Книга уже выдана!");
            }

            // Создаем запись выдачи
            string insertSql = "INSERT INTO Issues (UserId, BookId, IssueDate, ReturnDate) VALUES (@UserId, @BookId, @IssueDate, NULL)";
            using (var cmd = new SQLiteCommand(insertSql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookId", bookId);
                cmd.Parameters.AddWithValue("@IssueDate", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            // Обновляем статус книги
            string updateSql = "UPDATE Books SET Status='Выдана' WHERE Id=@Id";
            using (var cmd = new SQLiteCommand(updateSql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", bookId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

