using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ZooTicket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ticket> cart = new List<Ticket>();
        private List<Ticket> allTickets = new List<Ticket>();
        private List<User> users = new List<User>();
        private User currentUser;
        private string ticketsFile = @"D:\TicketData\tickets.json"; // Убраны лишние кавычки
        private string usersFile = @"D:\TicketData\users.json";

        // Цены вынесли в поля, админ может изменить
        private decimal adultPrice = 500m;
        private decimal childPrice = 250m;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user ?? throw new ArgumentNullException(nameof(user));
            UserInfoText.Text = $"{currentUser.Username} ({currentUser.Role})";

            LoadTickets();
            LoadUsers();
            LoadPrices();
            ApplyRolePermissions();
            UpdateTotal();
            RefreshUsersList();
        }

        #region Загрузка/сохранение данных
        private void LoadTickets()
        {
            try
            {
                if (File.Exists(ticketsFile))
                {
                    string json = File.ReadAllText(ticketsFile);
                    allTickets = JsonSerializer.Deserialize<List<Ticket>>(json) ?? new List<Ticket>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки билетов: {ex.Message}");
                allTickets = new List<Ticket>();
            }
        }

        private void SaveTickets()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ticketsFile) ?? @"D:\TicketData");
                string json = JsonSerializer.Serialize(allTickets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ticketsFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения билетов: {ex.Message}");
            }
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

            // Если файл пуст или удалён — создать базовых пользователей (только при пустом списке)
            if (users.Count == 0)
            {
                users = new List<User>
                {
                    new User { Username = "админ", Password = "123", Role = "Администратор" },
                    new User { Username = "кассир", Password = "123", Role = "Кассир" },
                    new User { Username = "контролер", Password = "123", Role = "Контролер" }
                };
                SaveUsers();
            }
        }

        private void SaveUsers()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(usersFile) ?? @"D:\TicketData");
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(usersFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения пользователей: {ex.Message}");
            }
        }

        private void LoadPrices()
        {
            // можно хранить в small settings файл, но пока если usersFile есть, оставить дефолт
            // Если нужно — добавить отдельный файл с настройками.
            AdultPriceTextBox.Text = adultPrice.ToString();
            ChildPriceTextBox.Text = childPrice.ToString();
        }

        private void SavePricesToFields()
        {
            if (decimal.TryParse(AdultPriceTextBox.Text, out var a)) adultPrice = a;
            if (decimal.TryParse(ChildPriceTextBox.Text, out var c)) childPrice = c;
        }
        #endregion

        #region Роли и разрешения
        private void ApplyRolePermissions()
        {
            // По умолчанию все вкладки доступны — потом выключим лишние
            AdminTab.IsEnabled = false;
            SalesTab.IsEnabled = false;
            CheckTab.IsEnabled = false;
            ReportsTab.IsEnabled = false;

            switch (currentUser.Role)
            {
                case "Администратор":
                    AdminTab.IsEnabled = true;
                    SalesTab.IsEnabled = true;
                    CheckTab.IsEnabled = true;
                    ReportsTab.IsEnabled = true;
                    break;
                case "Кассир":
                    SalesTab.IsEnabled = true;
                    ReportsTab.IsEnabled = true;
                    break;
                case "Контролер":
                    CheckTab.IsEnabled = true;
                    break;
                default:
                    // ничего
                    break;
            }
        }

        private bool CanSell()
        {
            return currentUser.Role == "Кассир" || currentUser.Role == "Администратор";
        }
        private bool CanCheck()
        {
            return currentUser.Role == "Контролер" || currentUser.Role == "Администратор";
        }
        private bool IsAdmin() => currentUser.Role == "Администратор";
        #endregion

        #region Продажа билетов
        private void AddAdultTicket_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSell())
            {
                MessageBox.Show("У вас нет прав на продажу билетов.");
                return;
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Type = "Взрослый",
                Price = adultPrice,
                SaleDate = DateTime.Now,
                Status = "Действителен",
                SoldBy = currentUser.Username
            };

            cart.Add(ticket);
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = cart;
            UpdateTotal();
        }

        private void AddChildTicket_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSell())
            {
                MessageBox.Show("У вас нет прав на продажу билетов.");
                return;
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Type = "Детский",
                Price = childPrice,
                SaleDate = DateTime.Now,
                Status = "Действителен",
                SoldBy = currentUser.Username
            };

            cart.Add(ticket);
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = cart;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var ticket in cart) total += ticket.Price;
            TotalText.Text = $"Итого: {total} руб";
        }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var ticket in cart) total += ticket.Price;
            return total;
        }

        private void ProcessSale_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSell())
            {
                MessageBox.Show("У вас нет прав на оформление продажи.");
                return;
            }

            if (cart.Count == 0)
            {
                MessageBox.Show("Добавьте билеты в корзину!");
                return;
            }

            allTickets.AddRange(cart);
            SaveTickets();

            MessageBox.Show($"Продажа оформлена! Продано билетов: {cart.Count}\nСумма: {CalculateTotal()} руб");

            cart.Clear();
            CartListBox.ItemsSource = null;
            UpdateTotal();
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            cart.Clear();
            CartListBox.ItemsSource = null;
            UpdateTotal();
        }
        #endregion

        #region Проверка билета
        private void CheckTicket_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCheck())
            {
                MessageBox.Show("У вас нет прав на проверку билетов.");
                return;
            }

            string ticketId = TicketIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(ticketId))
            {
                CheckResultText.Text = "Введите ID билета";
                CheckResultText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var ticket = allTickets.Find(t => string.Equals(t.Id, ticketId, StringComparison.OrdinalIgnoreCase));

            if (ticket == null)
            {
                CheckResultText.Text = "Билет не найден";
                CheckResultText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (ticket.Status == "Использован")
            {
                CheckResultText.Text = "Билет уже использован";
                CheckResultText.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                ticket.Status = "Использован";
                SaveTickets();
                CheckResultText.Text = "Билет действителен. Проход разрешен!";
                CheckResultText.Foreground = System.Windows.Media.Brushes.Green;
            }
        }
        #endregion

        #region Отчеты
        private void ShowTodaySales_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser.Role != "Администратор" && currentUser.Role != "Кассир")
            {
                MessageBox.Show("У вас нет прав для просмотра отчётов.");
                return;
            }

            var todayTickets = allTickets.FindAll(t => t.SaleDate.Date == DateTime.Today);

            ReportListBox.ItemsSource = null;
            ReportListBox.ItemsSource = todayTickets;

            decimal total = 0;
            foreach (var ticket in todayTickets) total += ticket.Price;

            TotalSalesText.Text = $"Продано сегодня: {todayTickets.Count} билетов\nВыручка: {total} руб";
        }
        #endregion

        #region Админ: управление пользователями и ценами
        private void RefreshUsersList()
        {
            UsersListBox.ItemsSource = null;
            UsersListBox.ItemsSource = users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                MessageBox.Show("Только администратор может добавлять пользователей.");
                return;
            }

            var name = NewUserName.Text.Trim();
            var pass = NewUserPassword.Password;
            var roleItem = NewUserRole.SelectedItem as System.Windows.Controls.ComboBoxItem;
            var role = roleItem?.Content.ToString() ?? "Кассир";

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            if (users.Exists(u => u.Username.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                return;
            }

            users.Add(new User { Username = name, Password = pass, Role = role });
            SaveUsers();
            RefreshUsersList();

            NewUserName.Clear();
            NewUserPassword.Clear();
        }

        private void RemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                MessageBox.Show("Только администратор может удалять пользователей.");
                return;
            }

            var sel = UsersListBox.SelectedItem as User;
            if (sel == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.");
                return;
            }

            if (sel.Username.Equals(currentUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Нельзя удалить себя.");
                return;
            }

            users.Remove(sel);
            SaveUsers();
            RefreshUsersList();
        }

        private void ResetTestPasswords_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin()) return;

            foreach (var u in users)
            {
                if (u.Username == "админ" || u.Username == "кассир" || u.Username == "контролер")
                    u.Password = "123";
            }

            SaveUsers();
            MessageBox.Show("Тестовые пароли сброшены.");
        }

        private void SavePrices_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                MessageBox.Show("Только администратор может менять цены.");
                return;
            }

            SavePricesToFields();
            MessageBox.Show("Цены сохранены.");
        }
        #endregion

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void TicketIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TicketIdPlaceholder.Visibility = string.IsNullOrEmpty(TicketIdTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        private void TicketIdPlaceholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TicketIdTextBox.Focus();
        }

        // --- NewUserName placeholder ---
        private void NewUserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            NewUserNamePlaceholder.Visibility = string.IsNullOrEmpty(NewUserName.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        private void NewUserNamePlaceholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewUserName.Focus();
        }

        // --- NewUserPassword placeholder ---
        private void NewUserPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            NewUserPasswordPlaceholder.Visibility = string.IsNullOrEmpty(NewUserPassword.Password)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        private void NewUserPasswordPlaceholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewUserPassword.Focus();
        }
    }

}