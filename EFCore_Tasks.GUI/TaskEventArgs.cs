using EFCore_Tasks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore_Tasks.GUI
{
    public class TaskEventArgs : EventArgs
    {
        public Tasks Task { get; }

        public TaskEventArgs(Tasks task)
        {
            Task = task;
        }
    }

}
