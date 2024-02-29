using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Database.Database.Model;

namespace Database.Database.EntityTypeConfig
{
    public class UserConf : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasIndex(e => e.TgId, "IX_User_Id")
                    .IsUnique();

            entity.Property(e => e.Fio)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnName("FIO");

            entity.Property(e => e.TgId)
                .IsRequired();

            entity.HasIndex(e => e.TgId, "IX_Tg_Id")
                    .IsUnique();
        }
    }
}