import { uuid } from '../miscellaneous/system.types';
import { RoleDto } from './role.types';

////////////////////////////////////////////////////////////////////////////////////

export interface UserRolesDto {
    TenantId: uuid;
    UserId  : uuid;
    Roles   : RoleDto[];
}
