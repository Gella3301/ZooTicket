using System.Windows;
using System.Windows.Controls;

namespace ZooTicket
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private List<User> users = new List<User>();

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            // Создаем тестовых пользователей
            users = new List<User>
            {
                new User { Username = "админ", Password = "123", Role = "Администратор" },
                new User { Username = "кассир", Password = "123", Role = "Кассир" },
                new User { Username = "контролер", Password = "123", Role = "Контролер" }
            };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim().ToLower();
            string password = PasswordBox.Password;

            var user = users.Find(u => u.Username.ToLower() == username && u.Password == password);

            if (user != null)
            {
                // Правильно передаем пользователя в конструктор
                var mainWindow = new MainWindow(user); // ← вот здесь передается user
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorText.Text = "Неверный логин или пароль!";
            }
        }
    }
}
