using EFCore_Tasks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EFCore_Tasks.DataAccess
{
    public class TaskContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<TaskProgress> TaskProgress { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<TaskPriority> TaskPriorities { get; set; }
        public DbSet<TaskStage> TaskStages { get; set; }
        public DbSet<TaskPoint> TaskPoints { get; set; } // Dodaj to

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Tasks.db");
            optionsBuilder.UseSqlite($"Filename={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.TaskStage)
                .WithMany(ts => ts.Tasks)
                .HasForeignKey(t => t.TaskStageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.TaskPriority)
                .WithMany(r => r.Tasks)
                .HasForeignKey(t => t.TaskPriorityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tasks>()
                .HasMany(t => t.TaskPoints)
                .WithOne(tp => tp.Task)
                .HasForeignKey(tp => tp.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.Task)
                .WithMany(t => t.TaskProgresses)
                .HasForeignKey(tp => tp.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(t => t.Id);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.User)
                .WithMany(u => u.TaskProgress)
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.TaskPoint) // Dodaj to
                .WithMany() // Dodaj to
                .HasForeignKey(tp => tp.TaskPointId) // Dodaj to
                .OnDelete(DeleteBehavior.Cascade); // Dodaj to

            modelBuilder.Entity<TaskProgress>()
                .HasIndex(tp => tp.Progress)
                .HasFilter("[Progress] = 100");

            modelBuilder.Entity<Tasks>()
                .HasMany(c => c.Users)
                .WithMany(s => s.Tasks)
                .UsingEntity(cs => cs
                    .HasData(
                        new { TasksId = 1, UsersId = 1 },
                        new { TasksId = 1, UsersId = 2 },
                        new { TasksId = 2, UsersId = 3 }
                    )
                );

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Pracownik" },
                new Role { Id = 3, Name = "Kierownik" }
            );

            modelBuilder.Entity<TaskPriority>().HasData(
                new TaskPriority { Id = 1, Name = "Niski" },
                new TaskPriority { Id = 2, Name = "Średni" },
                new TaskPriority { Id = 3, Name = "Wysoki" }
            );

            modelBuilder.Entity<Users>().HasData(
                new Users { Id = 1, FirstName = "admin", LastName = "admin", Password = PasswordHasher.HashPassword("a"), RoleId = 1 },
                new Users { Id = 2, FirstName = "kierownik", LastName = "ssss", Password = PasswordHasher.HashPassword("k"), RoleId = 3 },
                new Users { Id = 3, FirstName = "pracownik", LastName = "www", Password = PasswordHasher.HashPassword("p"), RoleId = 2 },
                new Users { Id = 4, FirstName = "gosc", LastName = "www", Password = PasswordHasher.HashPassword("g"), RoleId = 2 }
            );

            modelBuilder.Entity<Tasks>().HasData(
                new Tasks { Id = 1, Title = "Zadanie1", Description = "Opis1", StatusId = 1, DueDate = new DateTime(2024, 6, 8, 9, 10, 2), CreatedDate = new DateTime(2023, 6, 8, 9, 10, 2), TaskPriorityId = 1, TaskStageId = 1 },
                new Tasks { Id = 2, Title = "Zadanie2", Description = "Opis2", StatusId = 1, DueDate = new DateTime(2024, 1, 8, 10, 40, 30), CreatedDate = new DateTime(2023, 6, 8, 9, 10, 2), TaskPriorityId = 2, TaskStageId = 4 },
                new Tasks { Id = 3, Title = "Zadanie3", Description = "Opis3", StatusId = 1, DueDate = new DateTime(2024, 10, 2, 2, 30, 3), CreatedDate = new DateTime(2023, 6, 8, 9, 10, 2), TaskPriorityId = 3, TaskStageId = 1 }
            );

            modelBuilder.Entity<TaskProgress>().HasData(
                new TaskProgress { Id = 1, TaskId = 1, UserId = 1, TaskPointId = 1, Date = DateTime.Now, Progress = 0 },
                new TaskProgress { Id = 2, TaskId = 2, UserId = 1, TaskPointId = 3, Date = DateTime.Now, Progress = 100 }
                //new TaskProgress { Id = 3, TaskId = 3, UserId = 2, TaskPointId = 4, Date = DateTime.Now, Progress = 0 }
            );

            modelBuilder.Entity<TaskStage>().HasData(
                new TaskStage { Id = 1, Name = "Nowe" },
                new TaskStage { Id = 2, Name = "W trakcie" },
                new TaskStage { Id = 3, Name = "Do zatwierdzenia" },
                new TaskStage { Id = 4, Name = "Zakończone" }
            );

            modelBuilder.Entity<TaskPoint>().HasData(
                new TaskPoint() { Id = 1, TaskId = 1, Content = "Punkt 1", IsCompleted = false },
                new TaskPoint() { Id = 2, TaskId = 1, Content = "Punkt 2", IsCompleted = false },
                new TaskPoint() { Id = 3, TaskId = 2, Content = "Punkt 1", IsCompleted = false },
                new TaskPoint() { Id = 4, TaskId = 3, Content = "Punkt 1", IsCompleted = false }
            );
        }

        public override int SaveChanges()
        {
            var completedTasks = Tasks.Include(t => t.TaskProgresses)
                                      .Where(t => t.TaskProgresses.Any(tp => tp.Progress == 100))
                                      .ToList();

            foreach (var task in completedTasks)
            {
                task.TaskStageId = 4; // Ustawienie etapu zadania na "Zakończone"
            }

            return base.SaveChanges();
        }
    }

    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashedInput, hashedPassword) == 0;
        }
    }
}
