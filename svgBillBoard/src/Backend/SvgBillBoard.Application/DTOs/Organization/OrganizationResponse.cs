using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Application.DTOs.Organization
{
    public class OrganizationResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public byte Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
