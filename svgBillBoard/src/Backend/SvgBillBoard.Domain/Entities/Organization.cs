using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Domain.Entities
{
    public class Organization
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public byte Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; }
    = new List<User>();

        public ICollection<Role> Roles { get; set; }
            = new List<Role>();

        public ICollection<Location> Locations { get; set; }
    = new List<Location>();
    }
}
