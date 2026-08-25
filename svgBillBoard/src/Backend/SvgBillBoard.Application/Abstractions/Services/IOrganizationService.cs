using SvgBillBoard.Application.DTOs.Organization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Application.Abstractions.Services
{
    public interface IOrganizationService
    {
        Task<OrganizationResponse> CreateAsync(
    CreateOrganizationRequest request);

        Task<List<OrganizationResponse>> GetAllAsync();

        Task<OrganizationResponse?> GetByIdAsync(Guid id);

        Task<bool> UpdateAsync(
            Guid id,
            UpdateOrganizationRequest request);

        Task<bool> DeactivateAsync(Guid id);
    }
}
