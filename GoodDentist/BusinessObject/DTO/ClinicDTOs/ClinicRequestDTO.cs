﻿using BusinessObject.DTO.ServiceDTOs.View;
using BusinessObject.Entity;

namespace BusinessObject.DTO.ClinicDTOs;

public class ClinicUpdateDTO
{
    public Guid ClinicId { get; set; }

    public string ClinicName { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool? Status { get; set; }
    public ICollection<ServiceDTO> Services { get; set; } = new List<ServiceDTO>();
}