using EFCore_Tasks.DataAccess;
using EFCore_Tasks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EFCore_Tasks.DataAccess.TaskContext;

namespace EFCore_Tasks
{
    internal class Program
    {
        private static Users currentUser;
        private static Role currentRole;

        static async Task Main(string[] args)
        {
            using (TaskContext taskContext = new TaskContext())
            {
                bool deleted = await taskContext.Database.EnsureDeletedAsync();
                Console.WriteLine($"Baza skasowana: {deleted}");

                bool created = await taskContext.Database.EnsureCreatedAsync();
                Console.WriteLine($"Baza utworzona: {created}");
                Console.WriteLine("Witaj w Task Managerze!");

                while (true)
                {
                    if (currentUser == null)
                    {
                        Console.WriteLine("\nWybierz opcję:");
                        Console.WriteLine("1. Zaloguj się");
                        Console.WriteLine("2. Wyjdź");

                        string option = Console.ReadLine();
                        if (option == "1")
                        {
                            Console.Write("Podaj login: ");
                            string login = Console.ReadLine();
                            Console.Write("Podaj hasło: ");
                            string password = Console.ReadLine();

                            currentUser = await taskContext.Users
                                .Include(u => u.Role)
                                .FirstOrDefaultAsync(u => u.FirstName == login);

                            if (currentUser != null && PasswordHasher.VerifyPassword(password, currentUser.Password))
                            {
                                Console.WriteLine($"Zalogowano jako {currentUser.FirstName} {currentUser.LastName}.");
                                currentRole = currentUser.Role;
                                Console.WriteLine($"Status: {currentUser.Role.Name}.");
                            }
                            else
                            {
                                Console.WriteLine("Niepoprawny login lub hasło.");
                                currentUser = null;
                            }
                        }
                        else if (option == "2")
                        {
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nWybierz opcję:");
                        Console.WriteLine("1. Wyloguj się");
                        Console.WriteLine("2. Wyświetl zadania");
                        if (currentRole.Name == "Pracownik")
                        {
                            Console.WriteLine("3. Wyświetl postępy swoich zadań");
                            Console.WriteLine("4. Wyświetl historię swoich zadań");
                        }
                        else
                        {
                            Console.WriteLine("3. Wyświetl postępy zadań");
                            Console.WriteLine("4. Wyświetl historię zadań");
                        }
                        Console.WriteLine("5. Dodaj nowe zadanie");

                        string option = Console.ReadLine();
                        if (option == "1")
                        {
                            currentUser = null;
                            currentRole = null;
                            Console.WriteLine("Wylogowano.");
                        }
                        else if (option == "2")
                        {
                            Console.WriteLine("Zadania:");
                            //List<Tasks> tasks = await taskContext.Tasks.ToListAsync();
                            List<Tasks> tasks = await taskContext.Tasks.Include(t => t.TaskPriority).Include(q => q.TaskStage).ToListAsync();

                            foreach (Tasks task in tasks)
                            {
                                Console.WriteLine($"ID: {task.Id}, Tytuł: {task.Title}, Data dodania: {task.CreatedDate}, Deadline: {task.DueDate}, Opis: {task.Description},Etap: {task.TaskStage.Name}, Priorytet: {task.TaskPriority.Name}");
                            }
                        }
                        else if (option == "3")
                        {
                            if (currentRole.Name == "Pracownik")
                            {
                                Console.WriteLine("Postępy Twoich zadań:");
                                List<TaskProgress> taskProgresses = await taskContext.TaskProgress
                                    .Include(tp => tp.Task)
                                    .Include(tp => tp.User)
                                    .Where(tp => tp.UserId == currentUser.Id)
                                    .ToListAsync();
                                foreach (TaskProgress taskProgress in taskProgresses)
                                {
                                    Console.WriteLine($"ID: {taskProgress.Id}, Użytkownik: {taskProgress.User.FirstName} {taskProgress.User.LastName}, " +
                                        $"Zadanie: {taskProgress.Task.Title}, Data: {taskProgress.Date}, Postęp: {taskProgress.Progress}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Postępy zadań:");
                                List<TaskProgress> taskProgresses = await taskContext.TaskProgress
                                    .Include(tp => tp.Task)
                                    .Include(tp => tp.User)
                                    .ToListAsync();
                                foreach (TaskProgress taskProgress in taskProgresses)
                                {
                                    Console.WriteLine($"ID: {taskProgress.Id}, Użytkownik: {taskProgress.User.FirstName} {taskProgress.User.LastName}, " +
                                        $"Zadanie: {taskProgress.Task.Title}, Data: {taskProgress.Date}, Postęp: {taskProgress.Progress}");
                                }
                            }
                        }
                        else if (option == "4")
                        {
                            if (currentRole.Name == "Pracownik")
                            {
                                Console.WriteLine("Historia Twoich zadań:");
                                List<TaskProgress> completedTasks = await taskContext.TaskProgress
                                    .Include(tp => tp.Task)
                                    .Include(tp => tp.User)
                                    .Where(tp => tp.Progress == 100 && tp.UserId == currentUser.Id)
                                    .ToListAsync();
                                foreach (TaskProgress completedTask in completedTasks)
                                {
                                    Console.WriteLine($"ID: {completedTask.Id}, Użytkownik: {completedTask.User.FirstName} {completedTask.User.LastName}, " +
                                        $"Zadanie: {completedTask.Task.Title}, Data: {completedTask.Date}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Historia zadań:");
                                List<TaskProgress> completedTasks = await taskContext.TaskProgress
                                    .Include(tp => tp.Task)
                                    .Include(tp => tp.User)
                                    .Where(tp => tp.Progress == 100)
                                    .ToListAsync();
                                foreach (TaskProgress completedTask in completedTasks)
                                {
                                    Console.WriteLine($"ID: {completedTask.Id}, Użytkownik: {completedTask.User.FirstName} {completedTask.User.LastName}, " +
                                        $"Zadanie: {completedTask.Task.Title}, Data: {completedTask.Date}");
                                }
                            }
                        }
                        else if (option == "5")
                        {
                            Console.Write("Podaj tytuł nowego zadania: ");
                            string title = Console.ReadLine();

                            Console.Write("Podaj opis zadania: ");
                            string desc = Console.ReadLine();

                            Tasks newTask = new Tasks { Title = title, Description = desc };
                            taskContext.Tasks.Add(newTask);

                            await taskContext.SaveChangesAsync();

                            Console.WriteLine($"Dodano nowe zadanie: ID: {newTask.Id}, Tytuł: {newTask.Title}");
                        }
                    }
                }
            }
        }
    }
}
