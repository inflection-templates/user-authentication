import * as fs from 'fs';
import * as path from 'path';
import { inject, injectable } from "tsyringe";
import { IRolePermissionRepo } from "../../database/repository.interfaces/authorization/role.permission.repo.interface";
import { IRoleRepo } from "../../database/repository.interfaces/authorization/role.repo.interface";
import { logger } from "../../logger/logger";
import { DefaultRoles } from "../../domain.types/authorization/role.types";
import { uuid } from "../../domain.types/miscellaneous/system.types";
import { RolePermissionCreateModel, RolePermissionDto } from "../../domain.types/authorization/role.permission.types";
import { PermissionScope } from '../../domain.types/authorization/enums';
import { IPermissionRepo } from '../../database/repository.interfaces/authorization/permission.repo.interface';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class RolePermissionService {

    constructor(
        @inject('IRolePermissionRepo') private _rolePermissionRepo: IRolePermissionRepo,
        @inject('IRoleRepo') private _roleRepo: IRoleRepo,
        @inject('IPermissionRepo') private _permissionRepo: IPermissionRepo,
    ) {}

    create = async (entity: RolePermissionCreateModel): Promise<RolePermissionDto> => {
        return await this._rolePermissionRepo.create(entity);
    };

    getById = async (id: uuid): Promise<RolePermissionDto> => {
        return await this._rolePermissionRepo.getById(id);
    };

    getPermissionsForRole = async (roleId: uuid): Promise<RolePermissionDto[]> => {
        return await this._rolePermissionRepo.getPermissionsForRole(roleId);
    };

    hasPermissionForRole = async (roleId: uuid, permissionId: uuid): Promise<boolean> => {
        return await this._rolePermissionRepo.hasPermissionForRole(roleId, permissionId);
    };

    delete = async (id: uuid): Promise<boolean> => {
        return await this._rolePermissionRepo.delete(id);
    };

    addPermission = async (roleId: uuid, permissionId: uuid, scope: string): Promise<RolePermissionDto> => {
        return await this._rolePermissionRepo.create({
            RoleId       : roleId,
            PermissionId : permissionId,
            Scope        : scope || PermissionScope.System,
            Enabled      : true,
        });
    };

    removePermission = async (roleId: uuid, permissionId: uuid): Promise<boolean> => {
        const rolePermission = await this._rolePermissionRepo.getRolePermission(roleId, permissionId);
        if (rolePermission) {
            return await this._rolePermissionRepo.delete(rolePermission.id);
        }
        return false;
    };

    enablePermission = async (roleId: uuid, permissionId: uuid): Promise<RolePermissionDto> => {
        const rolePermission = await this._rolePermissionRepo.getRolePermission(roleId, permissionId);
        if (rolePermission) {
            return await this._rolePermissionRepo.enable(rolePermission.id, true);
        }
        return null;
    };

    disablePermission = async (roleId: uuid, permissionId: uuid): Promise<RolePermissionDto> => {
        const rolePermission = await this._rolePermissionRepo.getRolePermission(roleId, permissionId);
        if (rolePermission) {
            return await this._rolePermissionRepo.enable(rolePermission.id, false);
        }
        return null;
    };

    updatePermissionScope = async (roleId: uuid, permissionId: uuid, scope: string): Promise<RolePermissionDto> => {
        const rolePermission = await this._rolePermissionRepo.getRolePermission(roleId, permissionId);
        if (rolePermission) {
            // Delete the old one and create a new one with updated scope
            await this._rolePermissionRepo.delete(rolePermission.id);
            return await this._rolePermissionRepo.create({
                RoleId       : roleId,
                PermissionId : permissionId,
                Scope        : scope || PermissionScope.System,
                Enabled      : rolePermission.Enabled,
            });
        }
        return null;
    };

    seedRolePermissions = async () => {
        try {
            var roles = DefaultRoles;
            for await (const r of roles) {
                const role = await this._roleRepo.getByName(r.Role);
                if (role == null) {
                    continue;
                }
                const seederFile = r.SeederFile;
                if (seederFile == null) {
                    continue;
                }
                var filepath = path.join(process.cwd(), 'seed.data', 'authorizations', seederFile);
                var fileBuffer = fs.readFileSync(filepath, 'utf8');
                const permissions = JSON.parse(fileBuffer);

                for (const p of permissions) {
                    var permissionName = p.name;
                    var enabled = p.enabled;
                    var scope = p.scope;

                    const permission = await this._permissionRepo.getByName(permissionName);
                    if (permission == null) {
                        continue;
                    }

                    const rp = await this._rolePermissionRepo.getRolePermission(role.id, permission.id);
                    if (rp == null) {
                        await this._rolePermissionRepo.create({
                            RoleId       : role.id,
                            PermissionId : permission.id,
                            Scope        : scope ?? PermissionScope.System,
                            Enabled      : enabled,
                        });
                    } else {
                        if (rp.Enabled !== enabled) {
                            await this._rolePermissionRepo.enable(rp.id, enabled);
                        }
                    }
                }
            }
            logger.info('Role-permissions seeded successfully!');
        } catch (error) {
            logger.info('Error occurred while seeding role-permissions!');
        }
    };

    getForTenant = async (tenantId: uuid): Promise<RolePermissionDto[]> => {
        return await this._rolePermissionRepo.getForTenant(tenantId);
    };

    getSystemRolePermissions = async (): Promise<RolePermissionDto[]> => {
        return await this._rolePermissionRepo.getSystemRolePermissions();
    };

}
