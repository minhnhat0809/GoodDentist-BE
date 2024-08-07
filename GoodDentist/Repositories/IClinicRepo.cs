﻿using BusinessObject.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IClinicRepo : IRepositoryBase<Clinic>
    {
        Task<Clinic?> getClinicById(string id);
        Task<List<Clinic>> GetClinicByUserId(Guid userId);
    }
}
