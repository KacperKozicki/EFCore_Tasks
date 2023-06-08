using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EFCore_Tasks.Models;
using Microsoft.Extensions.Logging;

namespace EFCore_Tasks.DataAccess
{
    public class TaskContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<TaskProgress> TaskProgress { get; set; } // Dodana definicja DbSet dla TaskProgress


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Tasks.db");
            optionsBuilder.UseSqlite($"Filename={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguracja relacji wiele-do-wielu między Tasks i Users za pomocą klasy pośredniczącej TaskProgress
            modelBuilder.Entity<TaskProgress>()
                .HasKey(tp => new { tp.TaskId, tp.UserId });

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.Task)
                .WithMany(t => t.TaskProgress)
                .HasForeignKey(tp => tp.TaskId);

            modelBuilder.Entity<TaskProgress>()
                .HasOne(tp => tp.User)
                .WithMany(u => u.TaskProgress)
                .HasForeignKey(tp => tp.UserId);

            // Inicjalne dane dla tabel
            Users s1 = new Users { Id = 1, FirstName = "aaaa", LastName = "aaaaa", Password = PasswordHasher.HashPassword("password1") };
            Users s2 = new Users { Id = 2, FirstName = "ssss", LastName = "ssss", Password = PasswordHasher.HashPassword("password2") };
            modelBuilder.Entity<Users>().HasData(s1, s2, new Users { Id = 3, FirstName = "dupa", LastName = "www", Password = PasswordHasher.HashPassword("password3") });

            Tasks c1 = new Tasks { Id = 1, Title = "Zadanie1" };
            Tasks c2 = new Tasks { Id = 2, Title = "Zadanie2" };
            modelBuilder.Entity<Tasks>().HasData(c1, c2, new Tasks { Id = 3, Title = "Zadanie3" });

            modelBuilder.Entity<TaskProgress>()
                .HasData(
                    new TaskProgress { TaskId = c1.Id, UserId = s1.Id },
                    new TaskProgress { TaskId = c1.Id, UserId = s2.Id },
                    new TaskProgress { TaskId = c2.Id, UserId = s2.Id }
                );
        }
        private void InitDatabase(ModelBuilder modelBuilder)
        {
            Users s1 = new() { Id = 1, FirstName = "aaaa", LastName = "aaaaa", Password = PasswordHasher.HashPassword("password1") };
            Users s2 = new() { Id = 2, FirstName = "ssss", LastName = "ssss", Password = PasswordHasher.HashPassword("password2") };
            modelBuilder.Entity<Users>().HasData(s1, s2, new() { Id = 3, FirstName = "dupa", LastName = "www", Password = PasswordHasher.HashPassword("password3") });

            Tasks c1 = new Tasks() { Id = 1, Title = "Zadanie1" };
            Tasks c2 = new Tasks() { Id = 2, Title = "Zadanie2" };
            modelBuilder.Entity<Tasks>().HasData(c1, c2, new() { Id = 3, Title = "Zadanie3" });

            modelBuilder.Entity<Tasks>()
                .HasMany(c => c.Users)
                .WithMany(s => s.Tasks)
                .UsingEntity(cs => cs
                .HasData(
                    new { TasksId = c1.Id, UsersId = s1.Id },
                    new { TasksId = c1.Id, UsersId = s2.Id },
                    new { TasksId = c2.Id, UsersId = s2.Id }));
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