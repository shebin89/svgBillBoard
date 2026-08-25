using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Organization;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;

        public OrganizationService(
            IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrganizationResponse> CreateAsync(
            CreateOrganizationRequest request)
        {
            var codeExists =
                await _repository.ExistsByCodeAsync(request.Code);

            if (codeExists)
            {
                throw new InvalidOperationException(
                    "Organization code already exists.");
            }

            var organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Email = request.Email,
                Phone = request.Phone,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(organization);
            await _repository.SaveChangesAsync();

            return MapToResponse(organization);
        }

        public async Task<List<OrganizationResponse>> GetAllAsync()
        {
            var organizations =
                await _repository.GetAllAsync();

            return organizations
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<OrganizationResponse?> GetByIdAsync(Guid id)
        {
            var organization =
                await _repository.GetByIdAsync(id);

            return organization == null
                ? null
                : MapToResponse(organization);
        }

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateOrganizationRequest request)
        {
            var organization =
                await _repository.GetByIdAsync(id);

            if (organization == null)
                return false;

            organization.Name = request.Name;
            organization.Email = request.Email;
            organization.Phone = request.Phone;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(organization);
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var organization =
                await _repository.GetByIdAsync(id);

            if (organization == null)
                return false;

            organization.Status = 0;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(organization);
            await _repository.SaveChangesAsync();

            return true;
        }

        private static OrganizationResponse MapToResponse(
            Organization organization)
        {
            return new OrganizationResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                Code = organization.Code,
                Email = organization.Email,
                Phone = organization.Phone,
                Status = organization.Status,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }
    }
}
