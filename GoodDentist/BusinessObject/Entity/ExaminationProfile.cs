﻿using System;
using System.Collections.Generic;

namespace BusinessObject.Entity;

public partial class ExaminationProfile
{
    public int ExaminationProfileId { get; set; }

    public Guid? CustomerId { get; set; }

    public DateOnly? Date { get; set; }

    public string? Diagnosis { get; set; }

    public bool? Status { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Examination> Examinations { get; set; } = new List<Examination>();
}
