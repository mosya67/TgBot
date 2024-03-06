﻿using Database.Db;
using Domain;
using Domain.Dto;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.AddFunctions
{
#warning класс ResetName переделать на SetName
    public class ResetName : IWriteCommand<bool, ResetNameDto>
    {
        readonly Context context;
        readonly IGetCommand<User, long> getUser;

        public ResetName(Context context, IGetCommand<User, long> getUser)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.getUser = getUser ?? throw new ArgumentNullException(nameof(getUser));
        }

        public bool Write(ResetNameDto parameter)
        {
            var user = getUser.Get(parameter.Id);
            if (user != null)
            {
                user.Fio = parameter.Name;
                context.Update(user);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
