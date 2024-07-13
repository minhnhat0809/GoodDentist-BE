﻿using BusinessObject.DTO.CustomerDTOs.View;
using BusinessObject.DTO.UserDTOs.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.ExaminationProfileDTOs.View
{
    public class ExaminationProfileForCustomerDTO
    {
        public int ExaminationProfileId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? DentistId { get; set; }

        public DateOnly? Date { get; set; }

        public string? Diagnosis { get; set; }

        public bool? Status { get; set; }

        public CustomerDTOForPhuc? Customer { get; set; }

        public UserForExamDTO? Dentist { get; set; }
    }
}
