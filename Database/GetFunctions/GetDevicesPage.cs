using Database.Db;
using Domain;
using Domain.Dto;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetDevicesPage : IGetCommand<IEnumerable<Device>, PageDto>
    {
        readonly Context context;

        public GetDevicesPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Device> Get(PageDto dto)
        {
            return context.Devices.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage);
        }
    }
}
