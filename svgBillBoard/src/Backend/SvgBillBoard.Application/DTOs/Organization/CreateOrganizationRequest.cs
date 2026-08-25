using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Application.DTOs.Organization
{
    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }
    }
}
