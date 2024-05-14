using Database.Db.EntityTypeConfig;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;

#nullable disable

namespace Database.Db
{
    public partial class Context : DbContext
    {
        public Context()
        {
            Database.EnsureCreated();
        }

        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<TestVersion> TestVersions { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning перед выдачей бота создать config файл со строкой подключения
                optionsBuilder.UseSqlite("Data Source=db.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AnswerConf());
            modelBuilder.ApplyConfiguration(new QuestionConf());
            modelBuilder.ApplyConfiguration(new TestConf());
            modelBuilder.ApplyConfiguration(new TestResultConf());
            modelBuilder.ApplyConfiguration(new UserConf());
            modelBuilder.ApplyConfiguration(new ProjectConf());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
