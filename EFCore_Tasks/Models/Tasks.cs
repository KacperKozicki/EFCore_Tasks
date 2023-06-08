using System;
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
        [Key, Column("id")]
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; }

        public ICollection<Users> Users { get; set; }
        public ICollection<TaskProgress> TaskProgress { get; set; }
    }
}
