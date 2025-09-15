import {
    PermissionCreateModel,
    PermissionDto,
    PermissionSearchFilters,
    PermissionSearchResults,
    PermissionUpdateModel
} from '../../../domain.types/authorization/permission.types';
import { uuid } from '../../../domain.types/miscellaneous/system.types';

////////////////////////////////////////////////////////////////////////////////////

export interface IPermissionRepo {

    create(model: PermissionCreateModel): Promise<PermissionDto>;

    getById(id: uuid): Promise<PermissionDto>;

    getByName(name: string): Promise<PermissionDto>;

    delete(id: uuid): Promise<boolean>;

    search(filters: PermissionSearchFilters): Promise<PermissionSearchResults>;

    searchByName(name?: string): Promise<PermissionDto[]>;

    update(id: uuid, model: PermissionUpdateModel): Promise<PermissionDto>;

}
