using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Db.EntityTypeConfig
{
    internal class ProjectConf : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> entity)
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(e => e.Id, "IX_TestGroup_Id")
                    .IsUnique();

            entity.HasIndex(e => e.Name, "IX_TestGroup_Name")
                .IsUnique();

            entity.HasMany(e => e.Tests)
                .WithOne(e => e.Project);
        }
    }
}
