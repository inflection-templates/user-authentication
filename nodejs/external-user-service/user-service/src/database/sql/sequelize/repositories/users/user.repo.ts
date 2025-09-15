import { Op } from 'sequelize';
import { ApiError } from "../../../../../common/api.error";
import { logger } from "../../../../../logger/logger";
import { IUserRepo } from "../../../../repository.interfaces/users/user.repo.interface";
import { UserMapper } from "../../mappers/users/user.mapper";
import User from '../../models/users/user.model';
import Tenant from '../../models/tenant/tenant.model';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import {
    UserSearchFilters,
    UserSearchResults,
    UserDto,
    UserCreateModel,
    UserUpdateModel,
} from '../../../../../domain.types/users/user.types';
import UserMetadata from '../../models/users/user.metadata.model';
import UserRole from '../../models/authorization/user.role.model';
import Role from '../../models/authorization/role.model';
import UserPassword from '../../models/users/user.password.model';
import UserOtp from '../../models/users/user.otp.model';
import UserSession from '../../models/users/user.session.model';
import UserDeviceDetails from '../../models/users/user.device.details.model';

///////////////////////////////////////////////////////////////////////////////////

export class UserRepo implements IUserRepo {

    getByPhone = async (
        tenantId: uuid,
        phoneCode: string,
        phoneNumber: string)
        : Promise<UserDto> => {
        const user = await User.findOne({
            where : {
                TenantId    : tenantId,
                PhoneCode   : phoneCode,
                PhoneNumber : phoneNumber
            }
        });
        return await UserMapper.toDto(user);
    };

    getByEmail = async (tenantId: uuid, email: string): Promise<UserDto> => {
        const user = await User.findOne({
            where : {
                TenantId : tenantId,
                Email    : email
            }
        });
        return await UserMapper.toDto(user);
    };

    getByUserName = async (userName: string): Promise<UserDto> => {
        const user = await User.findOne({
            where : {
                UserName : userName
            }
        });
        return await UserMapper.toDto(user);
    };

    create = async (model: UserCreateModel): Promise<UserDto> => {
        try {
            const entity = {
                TenantId        : model.TenantId ?? null,
                UserName        : model.UserName,
                Prefix          : model.Prefix ?? '',
                FirstName       : model.FirstName ?? null,
                MiddleName      : model.MiddleName ?? null,
                LastName        : model.LastName ?? null,
                PhoneCode       : model.PhoneCode ?? null,
                PhoneNumber     : model.PhoneNumber ?? null,
                Email           : model.Email ?? null,
                Gender          : model.Gender ?? null,
                DateOfBirth     : model.DateOfBirth ?? null,
                ProfileImageUrl : model.ProfileImageUrl ?? null,
            };
            const user = await User.create(entity);

            // Add user to tenant
            var tenant = null;
            if (model.TenantId != null) {
                tenant = await Tenant.findByPk(model.TenantId);
            }
            return UserMapper.toDto(user, null, tenant);
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: string): Promise<UserDto> => {
        try {
            const user = await User.findByPk(id);
            if (!user) {
                logger.info(`User not found: ${id}`);
                return null;
            }
            const metadata = await UserMetadata.findOne({
                where : {
                    UserId : id
                }
            });
            const tenant = await Tenant.findByPk(user.TenantId);
            const userRoles = await UserRole.findAll({
                where : {
                    UserId : id
                },
                include : [
                    {
                        model : Role,
                        as    : 'Role'
                    }
                ]
            });
            const roles = userRoles.map(x => x.Role);

            const dto = await UserMapper.toDto(user, metadata, tenant, roles);
            // logger.info(`User mapper DTO: ${JSON.stringify(dto)}`);
            return dto;

        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    update = async (id: string, model: UserUpdateModel): Promise<UserDto> => {
        try {
            var user = await User.findByPk(id);

            if (model.Prefix !== undefined) {
                user.Prefix = model.Prefix;
            }
            if (model.FirstName !== undefined) {
                user.FirstName = model.FirstName;
            }
            if (model.MiddleName !== undefined) {
                user.MiddleName = model.MiddleName;
            }
            if (model.LastName !== undefined) {
                user.LastName = model.LastName;
            }
            if (model.PhoneNumber !== undefined) {
                user.PhoneNumber = model.PhoneNumber;
            }
            if (model.Email !== undefined) {
                user.Email = model.Email;
            }
            if (model.Gender !== undefined) {
                user.Gender = model.Gender;
            }
            if (model.DateOfBirth !== undefined) {
                user.DateOfBirth = model.DateOfBirth;
            }
            if (model.ProfileImageUrl !== undefined) {
                user.ProfileImageUrl = model.ProfileImageUrl;
            }
            if (model.UserName !== undefined &&
                model.UserName !== null &&
                model.UserName.length > 0) {
                user.UserName = model.UserName;
            }

            await user.save();
            const dto = await UserMapper.toDto(user);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: UserSearchFilters): Promise<UserSearchResults> => {
        try {
            const search: any = { where: {} };

            if (filters.TenantId) {
                search.where['TenantId'] = filters.TenantId;
            }
            if (filters.UserName) {
                search.where['UserName'] = { [Op.like]: '%' + filters.UserName + '%' };
            }
            if (filters.UserId) {
                search.where['id'] = filters.UserId;
            }
            if (filters.PhoneNumber) {
                search.where['PhoneNumber'] = { [Op.like]: '%' + filters.PhoneNumber + '%' };
            }
            if (filters.Email) {
                search.where['Email'] = { [Op.like]: '%' + filters.Email + '%' };
            }
            if (filters.Name != null) {
                search.where[Op.or] = [
                    {
                        FirstName : { [Op.like]: '%' + filters.Name + '%' },
                    },
                    {
                        LastName : { [Op.like]: '%' + filters.Name + '%' },
                    },
                ];
            }
            if (filters.CreatedDateFrom != null && filters.CreatedDateTo != null) {
                search.where['CreatedAt'] = {
                    [Op.gte] : filters.CreatedDateFrom,
                    [Op.lte] : filters.CreatedDateTo,
                };
            }
            else if (filters.CreatedDateFrom == null && filters.CreatedDateTo != null) {
                search.where['CreatedAt'] = {
                    [Op.lte] : filters.CreatedDateTo,
                };
            }
            else if (filters.CreatedDateFrom != null && filters.CreatedDateTo == null) {
                search.where['CreatedAt'] = {
                    [Op.gte] : filters.CreatedDateFrom,
                };
            }

            const orderByColum = 'CreatedAt';
            let order = 'ASC';
            if (filters.Order === 'descending') {
                order = 'DESC';
            }
            search['order'] = [[orderByColum, order]];
            if (filters.OrderBy) {
                search['order'] = [[ filters.OrderBy, order]];
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

            const foundResults = await User.findAndCountAll(search);
            const dtos = foundResults.rows.map(x => UserMapper.toDto(x));

            const searchResults: UserSearchResults = {
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

    delete = async (id: string): Promise<boolean> => {
        try {
            const user = await User.findByPk(id);
            if (!user) {
                logger.info(`User not found: ${id}`);
                return false;
            }
            await UserMetadata.destroy({
                where : {
                    UserId : id
                }
            });
            await UserRole.destroy({
                where : {
                    UserId : id
                }
            });
            await UserPassword.destroy({
                where : {
                    UserId : id
                }
            });
            await UserOtp.destroy({
                where : {
                    UserId : id
                }
            });
            await UserSession.destroy({
                where : {
                    UserId : id
                }
            });
            await UserDeviceDetails.destroy({
                where : {
                    UserId : id
                }
            });
            const count = await User.destroy({
                where : {
                    id : id
                }
            });

            return count === 1;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    deleteProfileImageUrl = async (id: string): Promise<UserDto> => {
        try {
            const user = await User.findByPk(id);
            user.ProfileImageUrl = null;
            await user.save();
            const dto = await UserMapper.toDto(user);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
