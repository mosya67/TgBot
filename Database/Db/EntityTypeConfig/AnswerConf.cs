using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Model;

namespace Database.Db.EntityTypeConfig
{
    internal class AnswerConf : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> entity)
        {
            entity.Property(e => e.Result).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(128);
        }
    }
}
