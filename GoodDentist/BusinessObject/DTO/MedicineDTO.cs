﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO
{
    public class MedicineDTO
    {

        public string MedicineName { get; set; } = null!;

        public string? Type { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public bool? Status { get; set; }
    }
}
