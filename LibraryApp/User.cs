using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp
{
    //класс пользователя
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // Студент, Аспирант, Преподаватель
        public int MaxBooks { get; set; }
        public string Password { get; set; }
    
    }
}
