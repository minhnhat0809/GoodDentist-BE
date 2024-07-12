﻿namespace BusinessObject.DTO.ViewDTO;

public class PrescriptionMedicineDTO
{
    public int MedicinePrescriptionId { get; set; }

    public int? MedicineId { get; set; }

    public int? PrescriptionId { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public bool? Status { get; set; }

    public virtual MedicineDTO? Medicine { get; set; }

    public virtual PrescriptionDTO? Prescription { get; set; }
    
}