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
using System.Windows.Shapes;

namespace ZooTicket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ticket> cart = new List<Ticket>();
        private List<Ticket> allTickets = new List<Ticket>();
        private string dataFile = "\"D:\\TicketData\\tickets.json\"";

        public MainWindow(User user)
        {
            InitializeComponent();
            LoadTickets();
            UpdateTotal();
        }


        // Загрузка билетов из файла
        private void LoadTickets()
        {
            try
            {
                if (File.Exists(dataFile))
                {
                    string json = File.ReadAllText(dataFile);
                    allTickets = JsonSerializer.Deserialize<List<Ticket>>(json) ?? new List<Ticket>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        // Сохранение билетов в файл
        private void SaveTickets()
        {
            try
            {
                string json = JsonSerializer.Serialize(allTickets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        // Добавление взрослого билета
        private void AddAdultTicket_Click(object sender, RoutedEventArgs e)
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Type = "Взрослый",
                Price = 500,
                SaleDate = DateTime.Now,
                Status = "Действителен",
                SoldBy = "Кассир"
            };

            cart.Add(ticket);
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = cart;
            UpdateTotal();
        }

        // Добавление детского билета
        private void AddChildTicket_Click(object sender, RoutedEventArgs e)
        {
            var ticket = new Ticket
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Type = "Детский",
                Price = 250,
                SaleDate = DateTime.Now,
                Status = "Действителен",
                SoldBy = "Кассир"
            };

            cart.Add(ticket);
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = cart;
            UpdateTotal();
        }

        // Обновление общей суммы
        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var ticket in cart)
            {
                total += ticket.Price;
            }
            TotalText.Text = $"Итого: {total} руб";
        }

        // Оформление продажи
        private void ProcessSale_Click(object sender, RoutedEventArgs e)
        {
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

        // Очистка корзины
        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            cart.Clear();
            CartListBox.ItemsSource = null;
            UpdateTotal();
        }

        // Проверка билета
        private void CheckTicket_Click(object sender, RoutedEventArgs e)
        {
            string ticketId = TicketIdTextBox.Text.Trim();

            if (string.IsNullOrEmpty(ticketId))
            {
                CheckResultText.Text = "Введите ID билета";
                CheckResultText.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var ticket = allTickets.Find(t => t.Id == ticketId);

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

        // Показать сегодняшние продажи
        private void ShowTodaySales_Click(object sender, RoutedEventArgs e)
        {
            var todayTickets = allTickets.FindAll(t => t.SaleDate.Date == DateTime.Today);

            ReportListBox.ItemsSource = null;
            ReportListBox.ItemsSource = todayTickets;

            decimal total = 0;
            foreach (var ticket in todayTickets)
            {
                total += ticket.Price;
            }

            TotalSalesText.Text = $"Продано сегодня: {todayTickets.Count} билетов\nВыручка: {total} руб";
        }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var ticket in cart)
            {
                total += ticket.Price;
            }
            return total;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }

}