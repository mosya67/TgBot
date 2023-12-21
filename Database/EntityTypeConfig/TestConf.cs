using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Database.EntityTypeConfig
{
    internal class TestConf : IEntityTypeConfiguration<Test>
    {
        public void Configure(EntityTypeBuilder<Test> entity)
        {
            entity.HasIndex(e => e.Id, "IX_Tests_Id")
                    .IsUnique();

            entity.Property(e => e.Date).HasColumnType("date").IsRequired();

            entity.Property(e => e.Name).HasMaxLength(64).IsRequired();

            entity.Property(e => e.Comment).HasMaxLength(128);
        }
    }
}
