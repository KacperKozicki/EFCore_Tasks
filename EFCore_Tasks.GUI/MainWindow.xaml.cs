using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EFCore_Tasks.Models;
using EFCore_Tasks;
using EFCore_Tasks.DataAccess;
using Microsoft.EntityFrameworkCore;
using EFCore_Tasks.GUI;

namespace EFCore_Tasks.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskContext taskContext;
        private Users currentUser;

        public MainWindow()
        {
            InitializeComponent();
            taskContext = new TaskContext();

        }


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            Users currentUser = await taskContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.FirstName == login);

            if (currentUser != null && PasswordHasher.VerifyPassword(password, currentUser.Password))
            {
                //MessageBox.Show($"Zalogowano jako {currentUser.FirstName} {currentUser.LastName}.");
                OpenTaskListWindow(currentUser);
            }
            else
            {
                MessageBox.Show("Niepoprawny login lub hasło.");
            }
        }

        private async Task OpenTaskListWindow(Users user)
        {
            //bool deleted = await taskContext.Database.EnsureDeletedAsync();
            //MessageBox.Show($"Baza skasowana. {deleted}");

            //bool created = await taskContext.Database.EnsureCreatedAsync();
            //MessageBox.Show($"Baza utworzona.{created}");


            currentUser = user;

            TaskListWindow taskListWindow = new TaskListWindow(user);
            taskListWindow.Show();
            this.Close();

            //TaskListWindow taskListWindow = new TaskListWindow(currentUser);
            //taskListWindow.Show();
            //this.Close();
        }

    }
}
