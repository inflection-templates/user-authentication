import Permission from '../../models/authorization/permission.model';
import { PermissionDto } from "../../../../../domain.types/authorization/permission.types";

///////////////////////////////////////////////////////////////////////////////////

export class PermissionMapper {

    static toDto = (permission: Permission): PermissionDto => {
        if (permission == null){
            return null;
        }
        const dto: PermissionDto = {
            id           : permission.id,
            Name         : permission.Name,
            Description  : permission.Description,
            TenantId     : permission.TenantId,
        };
        return dto;
    };

}
