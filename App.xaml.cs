using System.Configuration;
using System.Data;
using System.Windows;

namespace ZooTicket
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Запускаем с окна входа
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }

}
