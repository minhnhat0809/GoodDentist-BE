﻿using BusinessObject;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IClinicUserRepo : IRepositoryBase<ClinicUser>
    {
        Task<ClinicUser?> GetClinicUserByUserAndClinic(string userId, string clinicId);
    }
}
