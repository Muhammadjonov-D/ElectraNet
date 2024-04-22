﻿using AutoMapper;
using ElectraNet.DataAccess.UnitOfWorks;
using ElectraNet.Service.Configurations;
using ElectraNet.Service.DTOs.Organizations;
using ElectraNet.Service.Exceptions;
using ElectraNet.Service.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ElectraNet.Service.Services.Organizations;

public class OrganizationService(IMapper mapper, IUnitOfWork unitOfWork) : IOrganizationService
{
    public async ValueTask<OrganizationViewModel> CreateAsync(OrganizationCreateModel createModel)
    {
        var existOrganization = await unitOfWork.Organizations.SelectAsync(p =>
            p.Name.ToLower() == createModel.Name.ToLower() &&
        p.Address.ToLower() == createModel.Address.ToLower());

        if (existOrganization is not null)
            throw new AlreadyExistException($"This organization is already exists | OrganizationName = {createModel.Name} Address Name = {createModel.Address}");

        existOrganization.Create();
        var createdOrganization = await unitOfWork.Organizations.InsertAsync(existOrganization);
        await unitOfWork.SaveAsync();

        return mapper.Map<OrganizationViewModel>(createdOrganization);

    }
    public async ValueTask<OrganizationViewModel> UpdateAsync(long id, OrganizationUpdateModel updateModel)
    {
        var existOrganization = await unitOfWork.Organizations.SelectAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Organization is not found with this ID = {id}");

        var alreadyExistOrganization = await unitOfWork.Organizations.SelectAsync(p =>
             p.Name.ToLower() == updateModel.Name.ToLower() &&
             p.Address.ToLower() == updateModel.Address.ToLower());
        if (alreadyExistOrganization is not null)
            throw new AlreadyExistException($"This organization is already exists | Name = {updateModel.Name} Address Name = {updateModel.Address}");

        existOrganization.Name = updateModel.Name;
        existOrganization.Address = updateModel.Address;
        existOrganization.PhoneNumber = updateModel.PhoneNumber;

        existOrganization.Create();
        await unitOfWork.Organizations.UpdateAsync(existOrganization);
        await unitOfWork.SaveAsync();

        return mapper.Map<OrganizationViewModel>(existOrganization);
    }

    public async ValueTask<bool> DeleteAsync(long id)
    {
        var existOrganization = await unitOfWork.Users.SelectAsync(p => p.Id == id)
             ?? throw new NotFoundException($"Organization is not found with this ID = {id}");

        await unitOfWork.Organizations.DropAsync(existOrganization);
        await unitOfWork.SaveAsync();

        return true;
    }

    public async ValueTask<IEnumerable<OrganizationViewModel>> GetAllAsync(PaginationParams @params, Filter filter, string search = null)
    {
        var organizations = unitOfWork.Organizations.SelectAsQueryable().OrderBy(filter);

        if (!string.IsNullOrEmpty(search))
            organizations = organizations.Where(p =>
             p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
             p.Address.Contains(search, StringComparison.OrdinalIgnoreCase) ||
             p.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase));

        return mapper.Map<IEnumerable<OrganizationViewModel>>(await organizations.ToPaginateAsQueryable(@params).ToListAsync());
    }

    public async ValueTask<OrganizationViewModel> GetByIdAsync(long id)
    {
        var existOrganization = await unitOfWork.Organizations.SelectAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Organization is not found with this ID = {id}");
        return mapper.Map<OrganizationViewModel>(existOrganization);
    }
}