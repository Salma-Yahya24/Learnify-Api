﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.Models;

    
    
public class JwtSettings
{
            public string Secret { get; set; } = string.Empty;
            public string Issuer { get; set; }= string.Empty;
            public string Audience { get; set; }=string.Empty;
            public int ExpiryMinutes { get; set; }
}
    

