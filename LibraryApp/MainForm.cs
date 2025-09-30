using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryApp
{
    public partial class MainForm : Form
    {
        public User _currentUser;
        public MainForm(User currentUser)
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = true; // чтобы колонки создавались автоматически
            dataGridView1.ReadOnly = true;            // запрет редактирования напрямую
            _currentUser = currentUser;
            lblWelcome.Text = $"Добро пожаловать, {_currentUser.Name} ({_currentUser.Category})";
            this.Load += MainForm_Load;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadBooks();
        }
        private void LoadBooks()
        {
            var books = LibraryDB.GetAllBooks();
            dataGridView1.DataSource = books;
        }
        private void btnIssueBook_Click(object sender, EventArgs e)
        {
            try
            {
                string userIdStr = Prompt.ShowDialog("Введите ID пользователя:", "Выдача книги");
                string bookIdStr = Prompt.ShowDialog("Введите ID книги:", "Выдача книги");

                if (int.TryParse(userIdStr, out int userId) && int.TryParse(bookIdStr, out int bookId))
                {
                    LibraryDB.IssueBook(userId, bookId); // метод ниже
                    MessageBox.Show("Книга успешно выдана!");
                    LoadBooks(); // обновление DataGridView
                }
                else
                {
                    MessageBox.Show("Ошибка: неверные данные!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выдачи книги: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var books = LibraryDB.GetAllBooks();
            dataGridView1.DataSource = books;
        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            string title = Prompt.ShowDialog("Название книги:", "Добавить книгу");
            string author = Prompt.ShowDialog("Автор:", "Добавить книгу");
            int year = int.Parse(Prompt.ShowDialog("Год издания:", "Добавить книгу"));

            LibraryDB.AddBook(new Book
            {
                Title = title,
                Author = author,
                Year = year,
                Category = "Общее",
                Status = "Свободна"
            });

            LoadBooks(); // обновляем DataGridView
        }

        private void btnDeleteBook_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int bookId = (int)dataGridView1.SelectedRows[0].Cells["Id"].Value;
                LibraryDB.DeleteBook(bookId);
                LoadBooks();
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.");
            }
            LoadBooks();
        }

        private void btnReturnBook_Click(object sender, EventArgs e)
        {
            using (var conn = new SQLiteConnection())
            {
                conn.Open();

                int bookId = 0;
                int userId = 0;
                string category = "";
                DateTime issueDate = DateTime.Now;

                // Получаем запись выдачи
                string sql = "SELECT BookId, UserId, ReturnDate, IssueDate FROM Issues WHERE Id=@Id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    //cmd.Parameters.AddWithValue("@Id", IssueId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Запись выдачи не найдена!");

                        bookId = Convert.ToInt32(reader["BookId"]);
                        userId = Convert.ToInt32(reader["UserId"]);
                        issueDate = Convert.ToDateTime(reader["IssueDate"]);

                        if (reader["ReturnDate"] != DBNull.Value)
                            throw new Exception("Книга уже возвращена!");
                    }
                }

                // Получаем категорию пользователя
                string catSql = "SELECT Category FROM Users WHERE Id=@UserId";
                using (var cmd = new SQLiteCommand(catSql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    category = cmd.ExecuteScalar()?.ToString();
                }

                // Расчёт штрафа
                
                DateTime expectedReturnDate = issueDate.AddDays(14); // 14 дней срок возврата
               

                // Обновляем запись: книга свободна, дата возврата и штраф
                string updateSql = "UPDATE Books SET Status='Свободна' WHERE Id=@BookId;" +
                                   "UPDATE Issues SET ReturnDate=@ReturnDate, Penalty=@Penalty WHERE Id=@Id";
                using (var cmd = new SQLiteCommand(updateSql, conn))
                {
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                    //cmd.Parameters.AddWithValue("@Penalty", penalty);
                    //cmd.Parameters.AddWithValue("@Id", issueId);
                    cmd.ExecuteNonQuery();
                }

                //MessageBox.Show(penalty > 0 ? $"Книга возвращена с просрочкой. Штраф: {penalty} руб." : "Книга возвращена вовремя.");
            }
        }
    }
}




