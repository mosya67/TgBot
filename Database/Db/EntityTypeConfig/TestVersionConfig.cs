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
    public class TestVersionConfig : IEntityTypeConfiguration<TestVersion>
    {
        public void Configure(EntityTypeBuilder<TestVersion> e)
        {
            e.HasMany(e => e.Questions).WithOne();
        }
    }
}
