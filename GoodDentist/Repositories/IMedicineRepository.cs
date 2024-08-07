﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Entity;

namespace Repositories
{
    public interface IMedicineRepository : IRepositoryBase<Medicine>
    {
        Task<List<Medicine>?> GetAllMedicine(int pageNumber, int pageSize);

        Task<Medicine?> GetMedicineByID(int Id);

        Task<List<Medicine>> GetAllMedicines();
    }
}
