using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.Models
{
    public class TaskProgress
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int TaskPointId { get; set; } // Dodany identyfikator TaskPointId
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int Progress { get; set; }

        [ForeignKey("TaskId")]
        public Tasks Task { get; set; }
        [ForeignKey("UserId")]
        public Users User { get; set; }
        [ForeignKey("TaskPointId")] // Dodane mapowanie na TaskPointId
        public TaskPoint TaskPoint { get; set; }
    }
}
