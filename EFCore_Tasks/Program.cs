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

        static async Task Main(string[] args)
        {
            using (TaskContext taskContext = new())
            {
                //if (!taskContext.Database.CanConnect())
                //{
                //    bool deleted = await taskContext.Database.EnsureDeletedAsync();
                //    Console.WriteLine($"Baza skasowana: {deleted}");

                //    bool created = await taskContext.Database.EnsureCreatedAsync();
                //    Console.WriteLine($"Baza utworzona: {created}");

                //    string sqlScript = taskContext.Database.GenerateCreateScript();
                //    Console.WriteLine(sqlScript);
                //}
                //else
                //{
                //    Console.WriteLine("Baza już istnieje.");
                //}
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

                            currentUser = await taskContext.Users.FirstOrDefaultAsync(u => u.FirstName == login);
                            if (currentUser != null && PasswordHasher.VerifyPassword(password, currentUser.Password))
                            {
                                Console.WriteLine($"Zalogowano jako {currentUser.FirstName} {currentUser.LastName}.");
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
                        Console.WriteLine("3. Wyświetl postępy zadań");
                        Console.WriteLine("4. Wyświetl historię zadań");
                        Console.WriteLine("5. Dodaj nowe zadanie");

                        string option = Console.ReadLine();
                        if (option == "1")
                        {
                            currentUser = null;
                            Console.WriteLine("Wylogowano.");
                        }
                        else if (option == "2")
                        {
                            Console.WriteLine("Zadania:");
                            List<Tasks> tasks = await taskContext.Tasks.ToListAsync();
                            foreach (Tasks task in tasks)
                            {
                                Console.WriteLine($"ID: {task.Id}, Tytuł: {task.Title}");
                            }
                        }
                        else if (option == "3")
                        {
                            Console.WriteLine("Postępy zadań:");
                            List<TaskProgress> taskProgresses = await taskContext.TaskProgress.Include(tp => tp.Task).Include(tp => tp.User).ToListAsync();
                            foreach (TaskProgress taskProgress in taskProgresses)
                            {
                                Console.WriteLine($"ID: {taskProgress.Id}, Użytkownik: {taskProgress.User.FirstName} {taskProgress.User.LastName}, " +
                                    $"Zadanie: {taskProgress.Task.Title}, Data: {taskProgress.Date}, Postęp: {taskProgress.Progress}");
                            }
                        }
                        else if (option == "4")
                        {
                            Console.WriteLine("Historia zadań:");
                            List<TaskProgress> completedTasks = await taskContext.TaskProgress.Where(tp => tp.Progress == 100).ToListAsync();
                            foreach (TaskProgress completedTask in completedTasks)
                            {
                                Console.WriteLine($"ID: {completedTask.Id}, Użytkownik: {completedTask.User.FirstName} {completedTask.User.LastName}, " +
                                    $"Zadanie: {completedTask.Task.Title}, Data: {completedTask.Date}");
                            }
                        }
                        else if (option == "5")
                        {
                            Console.Write("Podaj tytuł nowego zadania: ");
                            string title = Console.ReadLine();

                            Tasks newTask = new Tasks { Title = title };
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
