using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Db.EntityTypeConfig
{
    internal class TestResultConf : IEntityTypeConfiguration<TestResult>
    {
        public void Configure(EntityTypeBuilder<TestResult> entity)
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(e => e.Id, "IX_TestResults_Id")
                    .IsUnique();

            entity.HasIndex(e => e.Version, "IX_TestResults_Version");

            entity.HasIndex(e => e.Date, "IX_TestResult_Date");

            entity.HasIndex(e => e.IsPaused, "IX_TestResult_IsPaused");
            entity.Property(e => e.Date).HasColumnType("timestamp").IsRequired();

            entity.Property(e => e.Comment).HasMaxLength(128);

#warning временная мера до изменения базы данных
            entity.Property(e => e.Version).HasColumnName("Release");
        }
    }
}
