import { IPermissionRepo } from '../../../../repository.interfaces/authorization/permission.repo.interface';
import Permission from '../../models/authorization/permission.model';
import { Op } from 'sequelize';
import {
    PermissionDto,
    PermissionCreateModel,
    PermissionUpdateModel,
    PermissionSearchFilters,
    PermissionSearchResults
} from '../../../../../domain.types/authorization/permission.types';
import { PermissionMapper } from '../../mappers/authorization/permission.mapper';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import RolePermission from '../../models/authorization/role.permission.model';
import UserPermission from '../../models/authorization/user.permission.model';

///////////////////////////////////////////////////////////////////////

export class PermissionRepo implements IPermissionRepo {

    create = async (model: PermissionCreateModel): Promise<PermissionDto> => {
        try {
            const entity = {
                Name        : model.Name,
                Description : model.Description ?? null,
                TenantId    : model.TenantId ?? null,
                Module      : model.Module ?? null,
            };
            const permission = await Permission.create(entity);
            const dto = PermissionMapper.toDto(permission);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: uuid): Promise<PermissionDto> => {
        try {
            const permission = await Permission.findByPk(id);
            const dto = await PermissionMapper.toDto(permission);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getByName = async (name: string): Promise<PermissionDto> => {
        try {
            const permission = await Permission.findOne({ where: { Name: name } });
            if (!permission) {
                // logger.info(`Permission not found: ${name}`);
                return null;
            }
            const dto = await PermissionMapper.toDto(permission);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (id: uuid): Promise<boolean> => {
        try {
            const permission = await Permission.findByPk(id);
            if (!permission) {
                logger.info(`Permission not found: ${id}`);
                return false;
            }
            await UserPermission.destroy({ where: { PermissionId: id } });
            await RolePermission.destroy({ where: { PermissionId: id } });
            await Permission.destroy({ where: { id: id } });
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    searchByName = async (name?: string): Promise<PermissionDto[]> => {
        try {
            let filter = { where: {} };
            if (name != null && name !== 'undefined') {
                filter = {
                    where : {
                        Name : { [Op.like]: '%' + name + '%' },
                    },
                };
            }
            const permissions = await Permission.findAll(filter);
            const dtos = permissions.map((x) => {
                return PermissionMapper.toDto(x);
            });
            return dtos;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: PermissionSearchFilters): Promise<PermissionSearchResults> => {
        try {
            const search = { where: {} };
            if (filters.Name != null) {
                search.where['Name'] = { [Op.like]: '%' + filters.Name + '%' };
            }

            if (filters.TenantId != null) {
                search.where['TenantId'] = filters.TenantId;
            }
            if (filters.Module != null) {
                search.where['Module'] = filters.Module;
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

            const foundResults = await Permission.findAndCountAll(search);
            const dtos = foundResults.rows.map(x => PermissionMapper.toDto(x));
            const searchResult: PermissionSearchResults = {
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

    update = async (id: uuid, model: PermissionUpdateModel): Promise<PermissionDto> => {
        try {
            const permission = await Permission.findOne({ where: { id: id } });

            if (model.Name != null) {
                permission.Name = model.Name;
            }
            if (model.Description != null) {
                permission.Description = model.Description;
            }
            if (model.TenantId != null) {
                permission.TenantId = model.TenantId;
            }
            if (model.Module != null) {
                permission.Module = model.Module;
            }

            await permission.save();
            const dto = await PermissionMapper.toDto(permission);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
