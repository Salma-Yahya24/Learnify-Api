using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnify.Entities.DTOs;

public class ForgotPassword
{

    public string Email { get; set; } = string.Empty;
}
public class VerifyCodeRequest
{
    public string ResetCode { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string ResetCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

