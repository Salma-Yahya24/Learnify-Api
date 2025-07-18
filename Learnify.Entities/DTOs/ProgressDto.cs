﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs;

public class ProgressDto
{
    public int ProgressId { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public int EnrollmentId { get; set; }
    public double CompletionPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}