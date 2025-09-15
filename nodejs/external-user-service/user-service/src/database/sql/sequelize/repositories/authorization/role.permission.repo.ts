import { IRolePermissionRepo } from '../../../../repository.interfaces/authorization/role.permission.repo.interface';
import RolePermission from '../../models/authorization/role.permission.model';
import {
    RolePermissionDto,
    RolePermissionCreateModel,
    RolePermissionSearchFilters,
    RolePermissionSearchResults,
} from '../../../../../domain.types/authorization/role.permission.types';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import Role from '../../models/authorization/role.model';
import { Op } from 'sequelize';

///////////////////////////////////////////////////////////////////////

export class RolePermissionRepo implements IRolePermissionRepo {

    create = async (object: RolePermissionCreateModel): Promise<RolePermissionDto> => {
        try {
            const entity = {
                RoleId       : object.RoleId,
                PermissionId : object.PermissionId,
                Scope        : object.Scope,
                Enabled      : object.Enabled,
            };
            const rp = await RolePermission.create(entity);
            const dto: RolePermissionDto = {
                id           : rp.id,
                RoleId       : rp.RoleId,
                PermissionId : rp.PermissionId,
                Scope        : rp.Scope,
                Enabled      : rp.Enabled,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: uuid): Promise<RolePermissionDto> => {
        try {
            const rp = await RolePermission.findByPk(id);
            const dto: RolePermissionDto = {
                id           : rp.id,
                RoleId       : rp.RoleId,
                PermissionId : rp.PermissionId,
                Scope        : rp.Scope,
                Enabled      : rp.Enabled,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    enable = async (id: uuid, enable: boolean): Promise<RolePermissionDto> => {
        try {
            const rp = await RolePermission.findByPk(id);
            rp.Enabled = enable;
            await rp.save();
            const dto: RolePermissionDto = {
                id           : rp.id,
                RoleId       : rp.RoleId,
                PermissionId : rp.PermissionId,
                Scope        : rp.Scope,
                Enabled      : rp.Enabled,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: RolePermissionSearchFilters): Promise<RolePermissionSearchResults> => {
        try {
            const search: any = { where: {} };

            if (filters.RoleId) {
                search.where['RoleId'] = filters.RoleId;
            }
            if (filters.Scope) {
                search.where['Scope'] = filters.Scope;
            }
            if (filters.Enabled) {
                search.where['Enabled'] = filters.Enabled;
            }
            const orderByColum = 'CreatedAt';
            let order = 'ASC';
            if (filters.Order === 'descending') {
                order = 'DESC';
            }
            search['order'] = [[orderByColum, order]];
            if (filters.OrderBy) {
                search['order'] = [[filters.OrderBy, order]];
            }
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

            const foundResults = await RolePermission.findAndCountAll(search);
            const dtos = foundResults.rows.map(x => {
                return {
                    id           : x.id,
                    RoleId       : x.RoleId,
                    PermissionId : x.PermissionId,
                    Scope        : x.Scope,
                    Enabled      : x.Enabled,
                };
            });
            const searchResults: RolePermissionSearchResults = {
                TotalCount     : foundResults.count,
                RetrievedCount : dtos.length,
                PageIndex      : pageIndex,
                ItemsPerPage   : limit,
                Order          : order === 'DESC' ? 'descending' : 'ascending',
                OrderedBy      : orderByColum,
                Items          : dtos,
            };
            return searchResults;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getPermissionsForRole = async (roleId: uuid): Promise<RolePermissionDto[]> => {
        try {
            const rolePermissions = await RolePermission.findAll({
                where : {
                    RoleId : roleId,
                },
            });
            const dtos: RolePermissionDto[] = [];
            for (const rp of rolePermissions) {
                const dto: RolePermissionDto = {
                    id           : rp.id,
                    RoleId       : rp.RoleId,
                    PermissionId : rp.PermissionId,
                    Scope        : rp.Scope,
                    Enabled      : rp.Enabled,
                };
                dtos.push(dto);
            }
            return dtos;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    hasPermissionForRole = async (roleId: uuid, permissionId: uuid): Promise<boolean> => {
        try {
            const rolePermissions = await RolePermission.findAll({
                where : {
                    RoleId       : roleId,
                    PermissionId : permissionId,
                },
            });
            if (rolePermissions.length > 0) {
                const rp = rolePermissions[0];
                return rp.Enabled;
            }
            return false;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getRolePermission = async (roleId: uuid, permissionId: uuid): Promise<RolePermissionDto|null> => {
        try {
            const rp = await RolePermission.findOne({
                where : {
                    RoleId       : roleId,
                    PermissionId : permissionId,
                },
            });
            if (rp == null) {
                return null;
            }
            const dto: RolePermissionDto = {
                id           : rp.id,
                RoleId       : rp.RoleId,
                PermissionId : rp.PermissionId,
                Scope        : rp.Scope,
                Enabled      : rp.Enabled,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (id: string): Promise<boolean> => {
        try {
            await RolePermission.destroy({ where: { id: id } });
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getForTenant = async (tenantId: uuid): Promise<RolePermissionDto[]> => {
        try {
            const rolePermissions = await RolePermission.findAll({
                include: [{
                    model: Role,
                    where: {
                        TenantId: tenantId
                    }
                }]
            });
            const dtos: RolePermissionDto[] = [];
            for (const rp of rolePermissions) {
                const dto: RolePermissionDto = {
                    id           : rp.id,
                    RoleId       : rp.RoleId,
                    PermissionId : rp.PermissionId,
                    Scope        : rp.Scope,
                    Enabled      : rp.Enabled,
                };
                dtos.push(dto);
            }
            return dtos;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getSystemRolePermissions = async (): Promise<RolePermissionDto[]> => {
        try {
            const systemRoles = await Role.findAll({
                where : {
                    IsSystemRole : true,
                },
            });
            const roleIds = systemRoles.map(x => x.id);
            const rolePermissions = await RolePermission.findAll({
                where : {
                    RoleId : {
                        [Op.in] : roleIds,
                    },
                },
            });
            const dtos: RolePermissionDto[] = [];
            for (const rp of rolePermissions) {
                const dto: RolePermissionDto = {
                    id           : rp.id,
                    RoleId       : rp.RoleId,
                    PermissionId : rp.PermissionId,
                    Scope        : rp.Scope,
                    Enabled      : rp.Enabled,
                };
                dtos.push(dto);
            }
            return dtos;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
