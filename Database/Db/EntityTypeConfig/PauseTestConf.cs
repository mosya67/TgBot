using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Model;

namespace Database.Db.EntityTypeConfig
{
    internal class PauseTestConf : IEntityTypeConfiguration<PauseTest>
    {
        public void Configure(EntityTypeBuilder<PauseTest> entity)
        {
            
        }
    }
}
