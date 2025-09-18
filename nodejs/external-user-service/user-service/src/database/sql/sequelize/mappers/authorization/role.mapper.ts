import Role from '../../models/authorization/role.model';
import { RoleDto } from "../../../../../domain.types/authorization/role.types";

///////////////////////////////////////////////////////////////////////////////////

export class RoleMapper {

    static toDto = (role: Role): RoleDto => {
        if (role == null){
            return null;
        }
        const dto: RoleDto = {
            id           : role.id,
            Name         : role.Name,
            Description  : role.Description,
            TenantId     : role.TenantId,
            ParentRoleId : role.ParentRoleId,
            IsSystemRole : role.IsSystemRole,
        };
        return dto;
    };

}
