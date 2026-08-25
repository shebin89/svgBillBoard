using System;
using System.Collections.Generic;
using System.Text;


namespace SvgBillBoard.Infrastructure.Authentication;

public class JwtTokenResult
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}