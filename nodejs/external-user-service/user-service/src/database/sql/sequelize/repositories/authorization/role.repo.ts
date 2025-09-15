import { IRoleRepo } from '../../../../repository.interfaces/authorization/role.repo.interface';
import Role from '../../models/authorization/role.model';
import { Op } from 'sequelize';
import {
    RoleDto,
    RoleCreateModel,
    RoleUpdateModel,
    RoleSearchFilters,
    RoleSearchResults
} from '../../../../../domain.types/authorization/role.types';
import { RoleMapper } from '../../mappers/authorization/role.mapper';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import UserRole from '../../models/authorization/user.role.model';
import RolePermission from '../../models/authorization/role.permission.model';

///////////////////////////////////////////////////////////////////////

export class RoleRepo implements IRoleRepo {

    create = async (model: RoleCreateModel): Promise<RoleDto> => {
        try {
            const entity = {
                Name         : model.Name,
                Description  : model.Description ?? null,
                TenantId     : model.TenantId ?? null,
                ParentRoleId : model.ParentRoleId ?? null,
                IsSystemRole : model.IsSystemRole ?? false,
            };
            const role = await Role.create(entity);
            const dto = RoleMapper.toDto(role);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: uuid): Promise<RoleDto> => {
        try {
            const role = await Role.findByPk(id);
            const dto = await RoleMapper.toDto(role);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getByName = async (name: string): Promise<RoleDto> => {
        try {
            const role = await Role.findOne({ where: { Name: name } });
            if (!role) {
                // logger.info(`Role not found: ${name}`);
                return null;
            }
            const dto = await RoleMapper.toDto(role);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (id: uuid): Promise<boolean> => {
        try {
            const role = await Role.findByPk(id);
            if (!role) {
                logger.info(`Role not found: ${id}`);
                return false;
            }
            await UserRole.destroy({ where: { RoleId: id } });
            await RolePermission.destroy({ where: { RoleId: id } });
            await Role.destroy({ where: { id: id } });
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    searchByName = async (name?: string): Promise<RoleDto[]> => {
        try {
            let filter = { where: {} };
            if (name != null && name !== 'undefined') {
                filter = {
                    where : {
                        Name : { [Op.like]: '%' + name + '%' },
                    },
                };
            }
            const roles = await Role.findAll(filter);
            const dtos = roles.map((x) => {
                return RoleMapper.toDto(x);
            });
            return dtos;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: RoleSearchFilters): Promise<RoleSearchResults> => {
        try {
            const search = { where: {} };
            if (filters.Name != null) {
                search.where['Name'] = { [Op.like]: '%' + filters.Name + '%' };
            }

            if (filters.TenantId != null) {
                search.where['TenantId'] = filters.TenantId;
            }
            if (filters.ParentRoleId) {
                search.where['ParentRoleId'] = filters.ParentRoleId;
            }
            if (filters.IsSystemRole) {
                search.where['IsSystemRole'] = filters.IsSystemRole;
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

            const foundResults = await Role.findAndCountAll(search);
            const dtos = foundResults.rows.map(x => RoleMapper.toDto(x));
            const searchResult: RoleSearchResults = {
                TotalCount     : foundResults.count,
                RetrievedCount : dtos.length,
                PageIndex      : pageIndex,
                ItemsPerPage   : limit,
                Order          : order === 'DESC' ? 'descending' : 'ascending',
                OrderedBy      : orderByColum,
                Items          : dtos,
            };

            return searchResult;
        }
        catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    update = async (id: uuid, model: RoleUpdateModel): Promise<RoleDto> => {
        try {
            const role = await Role.findOne({ where: { id: id } });

            if (model.Name != null) {
                role.Name = model.Name;
            }
            if (model.Description != null) {
                role.Description = model.Description;
            }
            if (model.TenantId != null) {
                role.TenantId = model.TenantId;
            }
            if (model.ParentRoleId != null) {
                role.ParentRoleId = model.ParentRoleId;
            }
            if (model.IsSystemRole != null) {
                role.IsSystemRole = model.IsSystemRole;
            }

            await role.save();
            const dto = await RoleMapper.toDto(role);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
