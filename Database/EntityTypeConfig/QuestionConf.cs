using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Database.EntityTypeConfig
{
    internal class QuestionConf : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> entity)
        {
            entity.Property(e => e.Question1)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnName("Question");

            entity.Property(e => e.Comment)
                .HasMaxLength(128);
        }
    }
}
