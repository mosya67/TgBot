using Database.Database.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Database.EntityTypeConfig
{
    internal class TestResultConf : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> entity)
        {
            entity.HasIndex(e => e.Id, "IX_TestResults_Id")
                    .IsUnique();

            entity.HasIndex(e => e.Date, "IX_TestResult_Date");
            entity.Property(e => e.Date).HasColumnType("timestamp").IsRequired();

            entity.Property(e => e.Comment).HasMaxLength(128);
        }
    }
}
