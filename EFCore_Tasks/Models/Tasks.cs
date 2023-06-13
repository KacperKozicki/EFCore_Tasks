﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.Models
{
    public class Tasks
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
        public ICollection<TaskPoint> TaskPoints { get; set; } // Nowa lista punktów zadania

        public int TaskStageId { get; set; }
        public TaskStage TaskStage { get; set; }

        public Tasks()
        {
            TaskProgresses = new List<TaskProgress>();
            TaskPoints = new List<TaskPoint>(); // Inicjalizacja listy punktów zadania
        }
    }
}
