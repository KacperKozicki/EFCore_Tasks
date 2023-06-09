using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.Models
{
    public class TaskPoint
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string PointTitle { get; set; }
        public bool IsCompleted { get; set; }

        public Tasks Task { get; set; }
    }
}
