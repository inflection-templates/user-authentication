import {
    RoleCreateModel,
    RoleDto,
    RoleSearchFilters,
    RoleSearchResults,
    RoleUpdateModel
} from '../../../domain.types/authorization/role.types';
import { uuid } from '../../../domain.types/miscellaneous/system.types';

////////////////////////////////////////////////////////////////////////////////////

export interface IRoleRepo {

    create(model: RoleCreateModel): Promise<RoleDto>;

    getById(id: uuid): Promise<RoleDto>;

    getByName(name: string): Promise<RoleDto>;

    delete(id: uuid): Promise<boolean>;

    search(filters: RoleSearchFilters): Promise<RoleSearchResults>;

    searchByName(name?: string): Promise<RoleDto[]>;

    update(id: uuid, model: RoleUpdateModel): Promise<RoleDto>;

}
