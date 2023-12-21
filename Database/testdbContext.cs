using Database.EntityTypeConfig;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Database
{
    public partial class testdbContext : DbContext
    {
        public testdbContext(){}

        public testdbContext(DbContextOptions<testdbContext> options)
            : base(options){}

        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<TestResult> TestResults { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=docs\\Database\\db.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AnswerConf());
            modelBuilder.ApplyConfiguration(new QuestionConf());
            modelBuilder.ApplyConfiguration(new TestConf());
            modelBuilder.ApplyConfiguration(new TestResultConf());
            modelBuilder.ApplyConfiguration(new UserConf());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
