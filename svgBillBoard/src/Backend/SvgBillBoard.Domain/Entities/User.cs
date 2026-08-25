using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public byte Status { get; set; } = 1;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Organization? Organization { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
