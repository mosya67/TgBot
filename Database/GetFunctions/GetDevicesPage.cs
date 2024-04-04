using Database.Db;
using Domain;
using Domain.Dto;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.GetFunctions
{
    public class GetDevicesPage : IGetCommand<Task<IEnumerable<Device>>, PageDto>
    {
        readonly Context context;

        public GetDevicesPage(Context context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Device>> Get(PageDto dto)
        {
            return await context.Devices.Skip(dto.startPage * dto.countElementsInPage).Take(dto.countElementsInPage + 1).ToListAsync();
        }
    }
}
