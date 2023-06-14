using EFCore_Tasks.DataAccess;
using EFCore_Tasks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EFCore_Tasks.GUI
{
    public partial class TaskDetailsWindow : Window
    {
        public event EventHandler<TaskPoint> TaskPointChecked;
        public event EventHandler<TaskPoint> TaskPointUnchecked;



        public Tasks SelectedTask { get; set; }
        public ObservableCollection<TaskPoint> TaskPoints { get; set; }

        public TaskDetailsWindow(Tasks selectedTask)
        {
            InitializeComponent();
            
            SelectedTask = selectedTask;
            TaskPoints = new ObservableCollection<TaskPoint>(SelectedTask.TaskPoints);
            DataContext = this;
            LoadTaskPoints();
            Closing += TaskDetailsWindow_Closing;
        
        }
        private void LoadTaskPoints()
        {
            try
            {
                using (var context = new TaskContext())
                {
                    var task = context.Tasks.Include(t => t.TaskPoints).FirstOrDefault(t => t.Id == SelectedTask.Id);

                    if (task != null)
                    {
                        TaskPoints = new ObservableCollection<TaskPoint>(task.TaskPoints);
                        DataContext = this;
                    }
                    else
                    {
                        MessageBox.Show("Nie znaleziono zadania w bazie danych.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas pobierania danych z bazy danych: {ex.Message}");
            }
        }

        private void SaveTaskPoints(IEnumerable<TaskPoint> taskPoints)
        {
            using (var context = new TaskContext())
            {
                context.TaskPoints.UpdateRange(taskPoints);
                context.SaveChanges();
            }
        }

        private void TaskDetailsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTaskPoints(TaskPoints);
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var taskPoint = (TaskPoint)checkBox.DataContext;

            TaskPointChecked?.Invoke(this, taskPoint);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var taskPoint = (TaskPoint)checkBox.DataContext;

            TaskPointUnchecked?.Invoke(this, taskPoint);
        }

    }
}
