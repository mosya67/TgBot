using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Model;

namespace Database.Db.EntityTypeConfig
{
    internal class DeviceConf : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            throw new System.NotImplementedException();
        }
    }
}
