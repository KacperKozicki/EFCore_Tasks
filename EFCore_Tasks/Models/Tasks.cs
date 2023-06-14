using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.Models
{
    public class Tasks : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TaskPriorityId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public int StatusId { get; set; }
        public ICollection<Users> Users { get; set; }
        public TaskPriority TaskPriority { get; set; }
        public ICollection<TaskProgress> TaskProgresses { get; set; }

        public int TaskStageId { get; set; }
        public TaskStage TaskStage { get; set; }
        public ICollection<TaskPoint> TaskPoints { get; set; }

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

        public Tasks()
        {
            TaskProgresses = new List<TaskProgress>();
            TaskPoints = new ObservableCollection<TaskPoint>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
