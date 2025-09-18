import {
    UserDeviceDetailsDomainModel,
    UserDeviceDetailsDto,
    UserDeviceDetailsSearchFilters,
    UserDeviceDetailsSearchResults
} from "../../../domain.types/users/user.device.details.types";
import { uuid } from "../../../domain.types/miscellaneous/system.types";

////////////////////////////////////////////////////////////////////////////////////

export interface IUserDeviceDetailsRepo {

    create(model: UserDeviceDetailsDomainModel): Promise<UserDeviceDetailsDto>;

    getById(id: uuid): Promise<UserDeviceDetailsDto>;

    getByUserId(userId: uuid): Promise<UserDeviceDetailsDto[]>;

    getExistingRecord(deviceDetails: any): Promise<UserDeviceDetailsDto>;

    search(filters: UserDeviceDetailsSearchFilters): Promise<UserDeviceDetailsSearchResults>;

    update(id: uuid, model: UserDeviceDetailsDomainModel): Promise<UserDeviceDetailsDto>;

    delete(id: uuid): Promise<boolean>;

}
