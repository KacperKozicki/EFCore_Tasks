using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.Models
{
    public class TaskStage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Tasks> Tasks { get; set; }
    }
}
