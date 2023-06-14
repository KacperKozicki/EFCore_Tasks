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
        /// <summary>
        /// Aktualnie zalogowany użytkownik.
        /// </summary>
        public Users CurrentUser { get; set; }
        private readonly TaskContext taskContext;

        private ObservableCollection<Tasks> taskList;

        /// <summary>
        /// Lista zadań.
        /// </summary>
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

        /// <summary>
        /// Historia zakończonych zadań.
        /// </summary>
        public ObservableCollection<Tasks> TaskHistory
        {
            get { return taskHistory; }
            set
            {
                taskHistory = value;
                OnPropertyChanged(nameof(TaskHistory));
            }
        }

        /// <summary>
        /// Postęp aktualnie wyświetlanego zadania.
        /// </summary>
        public int Progress { get; set; }

        private ObservableCollection<TaskProgress> taskProgresses;

        /// <summary>
        /// Lista postępów zadań.
        /// </summary>
        public ObservableCollection<TaskProgress> TaskProgresses
        {
            get { return taskProgresses; }
            set
            {
                taskProgresses = value;
                OnPropertyChanged(nameof(TaskProgresses));
            }
        }

        /// <summary>
        /// Zdarzenie wywoływane po zmianie wartości właściwości.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Konstruktor klasy TaskListWindow.
        /// </summary>
        /// <param name="user">Zalogowany użytkownik</param>
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

        /// <summary>
        /// Metoda wywoływana po zmianie wartości właściwości.
        /// </summary>
        /// <param name="propertyName">Nazwa zmienionej właściwości</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Asynchronicznie wczytuje zadania z bazy danych i aktualizuje listy zadań.
        /// </summary>
        public async void LoadTasks()
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
                    task.Progress = task.TaskProgresses.Sum(tp => tp.Progress) / task.TaskProgresses.Count;
                }
                else
                {
                    task.Progress = 0;
                }
            }
        }

        /// <summary>
        /// Obsługa zdarzenia kliknięcia przycisku dodawania zadania.
        /// Otwiera okno dialogowe AddTaskDialog.
        /// </summary>
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

        /// <summary>
        /// Obsługa zdarzenia zmiany właściwości zadania.
        /// Wywołuje zdarzenie PropertyChanged dla właściwości TaskList.
        /// </summary>
        private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TaskList));
        }

        /// <summary>
        /// Dodaje nowe zadanie do listy zadań.
        /// </summary>
        /// <param name="newTask">Nowe zadanie</param>
        public void AddNewTask(Tasks newTask)
        {
            TaskList.Add(newTask);
            MessageBox.Show("Nowe zadanie zostało dodane do listy.");
            OnPropertyChanged(nameof(TaskList));
        }

        /// <summary>
        /// Obsługa zdarzenia kliknięcia przycisku wylogowania.
        /// Wyświetla okno MainWindow i zamyka bieżące okno.
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        /// <summary>
        /// Asynchronicznie usuwa zadanie z bazy danych i z listy zadań.
        /// </summary>
        /// <param name="task">Zadanie do usunięcia</param>
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

        /// <summary>
        /// Obsługa zdarzenia kliknięcia przycisku usuwania zadania.
        /// Usuwa zadanie z listy zadań i z bazy danych, jeśli użytkownik ma odpowiednie uprawnienia.
        /// </summary>
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

        /// <summary>
        /// Sprawdza, czy użytkownik ma uprawnienia do tworzenia nowego zadania.
        /// </summary>
        /// <returns>Prawda, jeśli użytkownik ma uprawnienia do tworzenia zadania; w przeciwnym razie fałsz</returns>
        private bool UserHasPermissionToCreateTask()
        {
            return CurrentUser.RoleId == 1 || CurrentUser.RoleId == 3;
        }

        /// <summary>
        /// Obsługa zdarzenia ładowania przycisku usuwania.
        /// Ustawia widoczność przycisku w zależności od roli użytkownika.
        /// </summary>
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

        /// <summary>
        /// Obsługa zdarzenia zmiany zaznaczenia w ListBoxie z zadaniami.
        /// Otwiera okno TaskDetailsWindow, wyświetlające szczegóły wybranego zadania.
        /// </summary>
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
    }
}
