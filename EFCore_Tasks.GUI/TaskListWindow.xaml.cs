using EFCore_Tasks.DataAccess;
using EFCore_Tasks.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EFCore_Tasks.GUI
{
    public partial class TaskListWindow : Window, INotifyPropertyChanged
    {
        public Users CurrentUser { get; set; }
        private readonly TaskContext taskContext;

        private ObservableCollection<Tasks> taskList;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Tasks> TaskList
        {
            get { return taskList; }
            set
            {
                taskList = value;
                OnPropertyChanged(nameof(TaskList));
            }
        }

        private ObservableCollection<Tasks> taskHistory;

        public ObservableCollection<Tasks> TaskHistory
        {
            get { return taskHistory; }
            set
            {
                taskHistory = value;
                OnPropertyChanged(nameof(TaskHistory));
            }
        }

        private int progress;

        public int Progress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        private ObservableCollection<TaskProgress> taskProgresses;

        public ObservableCollection<TaskProgress> TaskProgresses
        {
            get { return taskProgresses; }
            set
            {
                taskProgresses = value;
                OnPropertyChanged(nameof(TaskProgresses));
            }
        }

        public TaskListWindow(Users user)
        {
            InitializeComponent();
            CurrentUser = user;
            DataContext = this;
            TaskList = new ObservableCollection<Tasks>();
            taskContext = new TaskContext();
            TaskHistory = new ObservableCollection<Tasks>();
            TaskProgresses = new ObservableCollection<TaskProgress>();
            LoadTasks();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LoadTasks()
        {
            IQueryable<Tasks> tasksQuery;

            if (CurrentUser.RoleId == 1 || CurrentUser.RoleId == 3)
            {
                // Użytkownicy z RoleId 1 i 3 widzą wszystkie zadania
                tasksQuery = taskContext.Tasks;
            }
            else if (CurrentUser.RoleId == 2)
            {
                // Użytkownicy z RoleId 2 widzą tylko zadania przypisane do siebie
                tasksQuery = taskContext.Tasks
                    .Where(t => t.Users.Any(u => u.Id == CurrentUser.Id));
            }
            else
            {
                // Pozostałe role nie mają dostępu do żadnych zadań
                tasksQuery = taskContext.Tasks.Where(t => false);
            }

            var tasks = await tasksQuery
                .Include(t => t.TaskPriority)
                .Include(t => t.TaskStage)
                .Include(t => t.TaskProgresses) // Dodaj to, aby załadować związane obiekty TaskProgress
                .ToListAsync();

            TaskList.Clear();
            TaskHistory.Clear();
            //Progress = 0;

            foreach (var task in tasks)
            {
                TaskList.Add(task);
                task.PropertyChanged += Task_PropertyChanged;

                if (task.TaskStageId == 4 || (task.TaskProgresses != null && task.TaskProgresses.Sum(tp => tp.Progress) == 100))
                {
                    TaskHistory.Add(task);
                }

                // Przypisanie wartości postępu bez obliczeń
                if (task.TaskProgresses != null && task.TaskProgresses.Any())
                {
                    task.Progress = task.TaskProgresses.OrderByDescending(tp => tp.Date).FirstOrDefault()?.Progress ?? 0;
                }
                else
                {
                    task.Progress = 0;
                }

            }

        }
        private void OpenAddTaskDialog_Click(object sender, RoutedEventArgs e)
        {
            if (UserHasPermissionToCreateTask())
            {
                AddTaskDialog dialog = new AddTaskDialog(this);
                dialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Nie masz uprawnień do dodawania nowych zadań.");
            }
        }

        

        private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TaskList));
        }

        public void AddNewTask(Tasks newTask)
        {
            TaskList.Add(newTask);
            MessageBox.Show("Nowe zadanie zostało dodane do listy.");
            OnPropertyChanged(nameof(TaskList));
        }

        private void TaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var selectedTask = (Tasks)listBox.SelectedItem;

            if (selectedTask != null)
            {
                var taskDetailsWindow = new TaskDetailsWindow(selectedTask);
                taskDetailsWindow.ShowDialog();

                // Clear the selection after opening the task details window
                listBox.SelectedItem = null;
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private async void DeleteTask(Tasks task)
        {
            var confirmResult = MessageBox.Show("Czy na pewno chcesz usunąć to zadanie?", "Potwierdzenie usunięcia", MessageBoxButton.YesNo);
            if (confirmResult == MessageBoxResult.Yes)
            {
                taskContext.Tasks.Remove(task);
                await taskContext.SaveChangesAsync();
                TaskList.Remove(task);
                MessageBox.Show("Zadanie zostało usunięte.");
            }
        }

        // Dodaj obsługę zdarzenia dla przycisku usuwania zadania
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.RoleId == 1 || CurrentUser.RoleId == 3)
            {
                var button = (Button)sender;
                var task = (Tasks)button.DataContext;
                DeleteTask(task);
            }
            else
            {
                MessageBox.Show("Nie masz uprawnień do usuwania zadań.");
            }
        }

        //private void OpenAddTaskDialog_Click(object sender, RoutedEventArgs e)
        //{
        //    if (UserHasPermissionToCreateTask())
        //    {
        //        AddTaskDialog dialog = new AddTaskDialog(this);
        //        dialog.ShowDialog();
        //    }
        //    else
        //    {
        //        MessageBox.Show("Nie masz uprawnień do dodawania nowych zadań.");
        //    }
        //}

        private bool UserHasPermissionToCreateTask()
        {
            return CurrentUser.RoleId == 1 || CurrentUser.RoleId == 3;
        }

        private void DeleteButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var task = (Tasks)button.DataContext;

            if (CurrentUser.RoleId == 1 || CurrentUser.RoleId == 3)
            {
                button.Visibility = Visibility.Visible;
            }
            else
            {
                button.Visibility = Visibility.Collapsed;
            }
        }










        private void OpenTaskDetailsWindow(Tasks selectedTask)
        {
            var taskDetailsWindow = new TaskDetailsWindow(selectedTask);
            taskDetailsWindow.TaskPointChecked += TaskDetailsWindow_TaskPointChecked;
            taskDetailsWindow.TaskPointUnchecked += TaskDetailsWindow_TaskPointUnchecked;
            taskDetailsWindow.ShowDialog();

            // Aktualizuj ProgressBar po zamknięciu okna TaskDetailsWindow
            UpdateProgressBar();
        }

        private void TaskDetailsWindow_TaskPointChecked(object sender, TaskPoint taskPoint)
        {
            // Aktualizuj TaskProgress na podstawie zaznaczonego TaskPoint
            var taskProgress = TaskProgresses.FirstOrDefault(tp => tp.TaskPointId == taskPoint.Id);
            if (taskProgress != null)
            {
                taskProgress.Progress = 100;
                // Zapisz zmiany w bazie danych
                SaveTaskProgress(taskProgress);
            }
            UpdateProgressBar();
        }

        private void TaskDetailsWindow_TaskPointUnchecked(object sender, TaskPoint taskPoint)
        {
            // Aktualizuj TaskProgress na podstawie odznaczonego TaskPoint
            var taskProgress = TaskProgresses.FirstOrDefault(tp => tp.TaskPointId == taskPoint.Id);
            if (taskProgress != null)
            {
                taskProgress.Progress = 0;
                // Zapisz zmiany w bazie danych
                SaveTaskProgress(taskProgress);
            }
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            // Oblicz wartość postępu na podstawie zadań w kolekcji TaskProgresses
            int totalProgress = TaskProgresses.Sum(tp => tp.Progress);

            // Oblicz średnią wartość postępu (uwzględniając liczbę zadań)
            int averageProgress = TaskProgresses.Count > 0 ? totalProgress / TaskProgresses.Count : 0;

            // Aktualizuj wartość Progress w klasie TaskListWindow
            Progress = averageProgress;
        }


        private void SaveTaskProgress(TaskProgress taskProgress)
        {
            using (var context = new TaskContext())
            {
                context.TaskProgress.Update(taskProgress);
                context.SaveChanges();
            }
        }
    }
}
