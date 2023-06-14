using EFCore_Tasks.DataAccess;
using EFCore_Tasks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EFCore_Tasks.GUI
{
    public partial class AddTaskDialog : Window
    {

        private readonly TaskContext taskContext;
        private readonly TaskListWindow taskListWindow;

        public AddTaskDialog(TaskListWindow taskListWindow)
        {
            InitializeComponent();
            taskContext = new TaskContext();
            this.taskListWindow = taskListWindow;
            LoadUsers();
        }

        private async Task<bool> SaveTask()
        {
            string title = TaskTitleTextBox.Text;
            string description = TaskDescriptionTextBox.Text;
            DateTime dueDate = DueDatePicker.SelectedDate ?? DateTime.MinValue;
            int taskPriorityId;

            switch (TaskPriorityComboBox.SelectedIndex)
            {
                case 0:
                    taskPriorityId = 1; // Niski
                    break;
                case 1:
                    taskPriorityId = 2; // Średni
                    break;
                case 2:
                    taskPriorityId = 3; // Wysoki
                    break;
                default:
                    MessageBox.Show("Nieprawidłowy priorytet zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
            }

            // Sprawdź, czy podany taskPriorityId istnieje w tabeli TaskPriority
            if (!taskContext.TaskPriorities.Any(tp => tp.Id == taskPriorityId))
            {
                MessageBox.Show("Nieprawidłowy priorytet zadania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                // Tworzenie nowego zadania
                Tasks newTask = new Tasks
                {
                    Title = title,
                    Description = description,
                    DueDate = dueDate,
                    TaskPriorityId = taskPriorityId,
                    TaskStageId = 1, // ID dla etapu "Nowe"
                    CreatedDate = DateTime.Now
                };

                foreach (var checkBox in UsersCheckBoxList.Children.OfType<CheckBox>())
                {
                    if (checkBox.IsChecked == true)
                    {
                        int userId = (int)checkBox.Tag;
                        var user = taskContext.Users.FirstOrDefault(u => u.Id == userId);
                        if (user != null)
                        {
                            if (newTask.Users == null)
                            {
                                newTask.Users = new List<Users>(); // Inicjalizacja listy użytkowników, jeśli jest null
                            }
                            newTask.Users.Add(user);
                        }
                    }
                }


                // Zapisz nowe zadanie do bazy danych lub wykonaj odpowiednie operacje
                // w zależności od logiki aplikacji
                taskContext.Tasks.Add(newTask);
                await taskContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas dodawania zadania do bazy danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LoadUsers()
        {
            var users = taskContext.Users.Where(u => u.RoleId == 2).ToList();

            foreach (var user in users)
            {
                var checkBox = new CheckBox
                {
                    Content = user.FirstName, // Poprawka: odwołanie do właściwości "Name"
                    Tag = user.Id,
                    Margin = new Thickness(5)
                };
                
                UsersCheckBoxList.Children.Add(checkBox);
            }
        }

       
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveTask())
            {
                MessageBox.Show("Zadanie zostało dodane do bazy danych.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                var newTask = new Tasks
                {
                    Title = TaskTitleTextBox.Text,
                    Description = TaskDescriptionTextBox.Text,
                    DueDate = DueDatePicker.SelectedDate ?? DateTime.MinValue,
                    TaskPriorityId = TaskPriorityComboBox.SelectedIndex + 1,
                    TaskStageId = 1, // ID dla etapu "Nowe"
                    CreatedDate = DateTime.Now
                };

                taskListWindow.AddNewTask(newTask);

                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
