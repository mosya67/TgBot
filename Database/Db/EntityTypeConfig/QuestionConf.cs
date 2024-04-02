using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Model;

namespace Database.Db.EntityTypeConfig
{
    internal class QuestionConf : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> entity)
        {
            entity.Property(e => e.question)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnName("Question");

            entity.Property(e => e.Comment)
                .HasMaxLength(128);
        }
    }
}
