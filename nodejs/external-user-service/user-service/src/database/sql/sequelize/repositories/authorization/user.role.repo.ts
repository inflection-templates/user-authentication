import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import { ApiError } from '../../../../../common/api.error';
import { UserRolesDto } from '../../../../../domain.types/authorization/user.role.types';
import { IUserRoleRepo } from '../../../../repository.interfaces/authorization/user.role.repo.interface';
import UserRole from '../../models/authorization/user.role.model';
import Role from '../../models/authorization/role.model';
import { RoleMapper } from '../../mappers/authorization/role.mapper';

///////////////////////////////////////////////////////////////////////////////////

export class UserRoleRepo implements IUserRoleRepo {

    getUserRoles = async (userId: uuid): Promise<UserRolesDto|null> => {
        try {
            const userRoles = await UserRole.findAll({
                where : {
                    UserId : userId
                },
                include : [
                    {
                        model : Role,
                        as    : 'Role'
                    }
                ]
            });
            if (userRoles.length === 0) {
                return null;
            }
            const roles = userRoles.map(x => x.Role);
            const roleDtos = await Promise.all(roles.map(x => RoleMapper.toDto(x)));
            const dto: UserRolesDto = {
                TenantId : userRoles[0].TenantId,
                UserId   : userId,
                Roles    : roleDtos
            };
            return dto;
        } catch (error) {
            throw new ApiError(500, 'Error fetching user roles');
        }
    };

    hasUserRole = async (userId: uuid, roleId: uuid): Promise<boolean> => {
        try {
            const userRole = await UserRole.findOne({
                where : {
                    UserId : userId,
                    RoleId : roleId
                }
            });
            return userRole != null;
        } catch (error) {
            throw new ApiError(500, 'Error fetching user role');
        }
    };

    addUserRole = async (userId: uuid, roleId: uuid, tenantId: uuid): Promise<boolean> => {
        try {

            //Check if user already has role
            const existingUserRole = await UserRole.findOne({
                where : {
                    UserId : userId,
                    RoleId : roleId
                }
            });
            if (existingUserRole != null) {
                throw new ApiError(400, 'User already has the givenrole');
            }
            const record = await UserRole.create({
                UserId   : userId,
                RoleId   : roleId,
                TenantId : tenantId
            });
            if (record == null) {
                throw new ApiError(500, 'Error adding user role');
            }
            return true;
        } catch (error) {
            throw new ApiError(500, 'Error adding user role');
        }
    };

    removeUserRole = async (userId: uuid, roleId: uuid): Promise<boolean> => {
        try {
            await UserRole.destroy({
                where : {
                    UserId : userId,
                    RoleId : roleId
                }
            });
            return true;
        } catch (error) {
            throw new ApiError(500, 'Error removing user role');
        }
    };

    removeAllUserRoles = async (userId: uuid): Promise<boolean> => {
        try {
            await UserRole.destroy({
                where : {
                    UserId : userId
                }
            });
            return true;
        } catch (error) {
            throw new ApiError(500, 'Error removing user roles');
        }
    };

}
