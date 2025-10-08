using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ZooTicket
{
    public partial class LoginWindow : Window
    {
        private List<User> users = new List<User>();
        private string usersFile = @"D:\TicketData\users.json";

        public LoginWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(usersFile))
                {
                    var json = File.ReadAllText(usersFile);
                    users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch
            {
                users = new List<User>();
            }

            if (users.Count == 0)
            {
                users = new List<User>
                {
                    new User { Username = "админ", Password = "123", Role = "Администратор" },
                    new User { Username = "кассир", Password = "123", Role = "Кассир" },
                    new User { Username = "контролер", Password = "123", Role = "Контролер" }
                };
                File.WriteAllText(usersFile, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            var user = users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);

            if (user != null)
            {
                var mainWindow = new MainWindow(user);
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
