import Tenant from '../../models/tenant/tenant.model';
import {
    TenantSearchFilters,
    TenantSearchResults,
    TenantCreateModel,
    TenantUpdateModel,
    TenantDto
} from "../../../../../domain.types/tenant/tenant.types";
import { ITenantRepo } from '../../../../../database/repository.interfaces/tenant/tenant.repo.interface';
import { TenantMapper } from '../../mappers/tenant/tenant.mapper';
import { Op } from 'sequelize';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import User from '../../models/users/user.model';
import Role from '../../models/authorization/role.model';
import UserRole from '../../models/authorization/user.role.model';
import { DefaultRoleTypes } from '../../../../../domain.types/authorization/enums';
import { logger } from '../../../../../logger/logger';

///////////////////////////////////////////////////////////////////////////////////

export class TenantRepo implements ITenantRepo {

    create = async (model: TenantCreateModel): Promise<TenantDto> => {
        try {
            const entity = {
                Name        : model.Name,
                Description : model.Description,
                TenantCode  : model.TenantCode,
                PhoneCode   : model.PhoneCode,
                PhoneNumber : model.PhoneNumber,
                Email       : model.Email
            };
            const tenant = await Tenant.create(entity);
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            const msg = error.message + '. ' + error?.original?.message;
            throw new Error(`Failed to create tenant: ${msg}`);
        }
    };

    getById = async (id: uuid): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findByPk(id);
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant by id: ${error.message}`);
        }
    };

    getByCode = async (code: string): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findOne({ where: { TenantCode: code } });
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant by code: ${error.message}`);
        }
    };

    getTenantWithPhone = async (phone: string): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findOne({ where: { PhoneNumber: phone } });
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant by phone: ${error.message}`);
        }
    };

    getTenantWithEmail = async (email: string): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findOne({ where: { Email: email } });
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant by email: ${error.message}`);
        }
    };

    getTenantWithCode = async (tenantCode: string): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findOne({ where: { TenantCode: tenantCode } });
            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant by code: ${error.message}`);
        }
    };

    tenantCount = async (): Promise<number> => {
        try {
            return await Tenant.count();
        }
        catch (error) {
            throw new Error(`Failed to fetch tenant count: ${error.message}`);
        }
    };

    exists = async (id: uuid): Promise<boolean> => {
        try {
            const tenant = await Tenant.findByPk(id);
            return tenant != null;
        }
        catch (error) {
            throw new Error(`Failed to check if tenant exists: ${error.message}`);
        }
    };

    existsWithCode = async (tenantCode: string): Promise<boolean> => {
        try {
            const tenant = await Tenant.findOne({ where: { TenantCode: tenantCode } });
            return tenant != null;
        }
        catch (error) {
            throw new Error(`Failed to check if tenant exists with code: ${error.message}`);
        }
    };

    search = async (filters: TenantSearchFilters): Promise<TenantSearchResults> => {
        try {
            const search = { where: {} };
            if (filters.Name != null) {
                search.where['Name'] = { [Op.like]: '%' + filters.Name + '%' };
            }
            if (filters.TenantCode != null) {
                search.where['TenantCode'] = { [Op.like]: '%' + filters.TenantCode + '%' };
            }
            if (filters.PhoneNumber != null) {
                search.where['PhoneNumber'] = { [Op.like]: '%' + filters.PhoneNumber + '%' };
            }
            if (filters.Email != null) {
                search.where['Email'] = { [Op.like]: '%' + filters.Email + '%' };
            }
            let orderByColum = 'CreatedAt';
            if (filters.OrderBy) {
                orderByColum = filters.OrderBy;
            }
            let order = 'ASC';
            if (filters.Order === 'descending') {
                order = 'DESC';
            }
            search['order'] = [[orderByColum, order]];

            let limit = 25;
            if (filters.ItemsPerPage) {
                limit = filters.ItemsPerPage;
            }
            let offset = 0;
            let pageIndex = 0;
            if (filters.PageIndex) {
                pageIndex = filters.PageIndex < 0 ? 0 : filters.PageIndex;
                offset = pageIndex * limit;
            }
            search['limit'] = limit;
            search['offset'] = offset;

            const foundResults = await Tenant.findAndCountAll(search);

            const dtos: TenantDto[] = foundResults.rows.map((tenant) => {
                return TenantMapper.toDto(tenant);
            });

            const searchResults: TenantSearchResults = {
                TotalCount     : foundResults.count,
                RetrievedCount : dtos.length,
                PageIndex      : pageIndex,
                ItemsPerPage   : limit,
                Order          : order === 'DESC' ? 'descending' : 'ascending',
                OrderedBy      : orderByColum,
                Items          : dtos,
            };

            return searchResults;
        }
        catch (error) {
            throw new Error(`Failed to search tenants: ${error.message}`);
        }
    };

    update = async (id: uuid, model: TenantUpdateModel): Promise<TenantDto> => {
        try {
            const tenant = await Tenant.findByPk(id);

            if (model.Name != null) {
                tenant.Name = model.Name;
            }
            if (model.Description != null) {
                tenant.Description = model.Description;
            }
            if (model.TenantCode != null) {
                tenant.TenantCode = model.TenantCode;
            }
            if (model.PhoneCode != null) {
                tenant.PhoneCode = model.PhoneCode;
            }
            if (model.PhoneNumber != null) {
                tenant.PhoneNumber = model.PhoneNumber;
            }
            if (model.Email != null) {
                tenant.Email = model.Email;
            }

            await tenant.save();

            return TenantMapper.toDto(tenant);
        }
        catch (error) {
            throw new Error(`Failed to update tenant: ${error.message}`);
        }
    };

    delete = async (id: string, hardDelete: boolean = false): Promise<boolean> => {
        try {
            const deletedCount = await Tenant.destroy({ where: { Id: id }, force: hardDelete });
            return deletedCount > 0;
        }
        catch (error) {
            throw new Error(`Failed to delete tenant: ${error.message}`);
        }
    };

    promoteTenantUserAsAdmin = async (tenantId: uuid, userId: uuid): Promise<boolean> => {
        try {
            const tenantUserRole = await Role.findOne({ where: { Name: DefaultRoleTypes.User } });
            if (tenantUserRole == null) {
                throw new Error(`Tenant user role not found!`);
            }
            const tenantAdminRole = await Role.findOne({ where: { Name: DefaultRoleTypes.SystemUser } });
            if (tenantAdminRole == null) {
                throw new Error(`System user role not found!`);
            }
            var tenantUser = await User.findOne(
                {
                    where : {
                        TenantId : tenantId,
                        id       : userId,
                    }
                }
            );
            if (tenantUser == null) {
                throw new Error(`User is not tenant user!`);
            }

            const userRole = await UserRole.findOne({
                where : {
                    UserId   : userId,
                    TenantId : tenantId,
                    RoleId   : tenantUserRole.id
                }
            });
            if (userRole == null) {
                logger.info(`User role not found!`);
            }
            else {
                //Delete the user role
                await UserRole.destroy({
                    where : {
                        UserId   : userId,
                        TenantId : tenantId,
                        RoleId   : tenantUserRole.id
                    }
                });
            }

            const existingAdmin = await UserRole.findOne({
                where : {
                    UserId   : userId,
                    TenantId : tenantId,
                    RoleId   : tenantAdminRole.id
                }
            });
            if (existingAdmin == null) {
                //Add the user role
                await UserRole.create({
                    UserId   : userId,
                    TenantId : tenantId,
                    RoleId   : tenantAdminRole.id
                });
            }

            return true;
        }
        catch (error) {
            throw new Error(`Failed to add user as admin to tenant: ${error.message}`);
        }
    };

    demoteAdmin = async (tenantId: uuid, userId: uuid): Promise<boolean> => {
        try {
            const tenantUserRole = await Role.findOne({ where: { Name: DefaultRoleTypes.User } });
            if (tenantUserRole == null) {
                throw new Error(`Tenant user role not found!`);
            }
            const tenantAdminRole = await Role.findOne({ where: { Name: DefaultRoleTypes.SystemUser } });
            if (tenantAdminRole == null) {
                throw new Error(`System user role not found!`);
            }
            var user = await User.findOne(
                {
                    where : {
                        TenantId : tenantId,
                        id       : userId,
                    }
                }
            );
            if (user == null) {
                throw new Error(`User is not tenant user!`);
            }
            const tenantAdminCount = await UserRole.count(
                {
                    where : {
                        RoleId   : tenantAdminRole.id,
                        UserId   : userId,
                        TenantId : tenantId,
                    }
                }
            );
            if (tenantAdminCount <= 1) {
                throw new Error(`Cannot demote the only admin user!`);
            }
            await UserRole.destroy({ where: { UserId: userId, RoleId: tenantAdminRole.id } });
            return true;
        }
        catch (error) {
            throw new Error(`Failed to remove user as admin from tenant: ${error.message}`);
        }
    };

    getTenantStats = async (id: uuid): Promise<any> => {
        try {
            const tenant = await Tenant.findByPk(id);
            if (tenant == null) {
                throw new Error(`Tenant not found!`);
            }
            const tenantUserRole = await Role.findOne({ where: { Name: DefaultRoleTypes.User } });
            if (tenantUserRole == null) {
                throw new Error(`Tenant user role not found!`);
            }
            const tenantAdminRole = await Role.findOne({ where: { Name: DefaultRoleTypes.SystemUser } });
            if (tenantAdminRole == null) {
                throw new Error(`System user role not found!`);
            }
            const usersCount = await User.count({ where: { TenantId: id } });
            const adminUsersCount = await User.count({ where: { TenantId: id, RoleId: tenantAdminRole.id } });
            const regularUsersCount = await User.count({ where: { TenantId: id, RoleId: tenantUserRole.id } });

            const stats = {
                TotalUsers    : usersCount,
                TotalAdmins   : adminUsersCount,
                TotalRegulars : regularUsersCount,
            };
            return stats;
        }
        catch (error) {
            throw new Error(`Failed to get tenant stats: ${error.message}`);
        }
    };

    getTenantAdmins = async (id: uuid): Promise<any[]> => {
        try {
            const role = await Role.findOne({ where: { Name: DefaultRoleTypes.SystemUser } });
            if (role == null) {
                throw new Error(`System user role not found!`);
            }
            const tenantAdmins = await UserRole.findAll({
                where : {
                    TenantId : id,
                    RoleId   : role.id
                },
                include : [
                    {
                        model    : User,
                        as       : 'User',
                        required : true,
                    }
                ]
            });
            const dtos = tenantAdmins.map((u) => {
                return {
                    id              : u.id,
                    UserName        : u.User.UserName,
                    FirstName       : u.User.FirstName,
                    LastName        : u.User.LastName,
                    PhoneNumber     : u.User.PhoneNumber,
                    Email           : u.User.Email,
                    ProfileImageUrl : u.User.ProfileImageUrl,
                    CreatedAt       : u.createdAt,
                    UpdatedAt       : u.updatedAt,
                };
            });
            return dtos;
        }
        catch (error) {
            throw new Error(`Failed to get tenant admins: ${error.message}`);
        }
    };

    getTenantRegularUsers = async (id: uuid): Promise<any[]> => {
        try {
            const role = await Role.findOne({ where: { Name: DefaultRoleTypes.User } });
            if (role == null) {
                throw new Error(`Tenant user role not found!`);
            }
            const tenantAdmins = await UserRole.findAll({
                where : {
                    TenantId : id,
                    RoleId   : role.id
                },
                include : [
                    {
                        model    : User,
                        as       : 'User',
                        required : true,
                    }
                ]
            });
            const dtos = tenantAdmins.map((u) => {
                return {
                    id              : u.id,
                    UserName        : u.User.UserName,
                    FirstName       : u.User.FirstName,
                    LastName        : u.User.LastName,
                    PhoneNumber     : u.User.PhoneNumber,
                    Email           : u.User.Email,
                    ProfileImageUrl : u.User.ProfileImageUrl,
                    CreatedAt       : u.createdAt,
                    UpdatedAt       : u.updatedAt,
                };
            });
            return dtos;
        }
        catch (error) {
            throw new Error(`Failed to get tenant moderators: ${error.message}`);
        }
    };

}
