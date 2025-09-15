import { BaseSearchFilters, BaseSearchResults } from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";

///////////////////////////////////////////////////////////////////////////////////////

export interface UserDeviceDetailsDomainModel {
    id              ?: uuid,
    Token           ?: string;
    DeviceIdentifier?: string;
    UserId          ?: uuid;
    DeviceName      ?: string;
    OSType          ?: string;
    OSVersion       ?: string;
    AppName         ?: string;
    AppVersion      ?: string;
    ChangeCount     ?: number;
}

export interface DeviceDetails {
    OSType?: string;
    Count ?: number;
}

export interface UserDeviceDetailsDto {
    id              ?: uuid,
    Token           ?: string;
    DeviceIdentifier?: string;
    UserId          ?: uuid;
    DeviceName      ?: string;
    OSType          ?: string;
    OSVersion       ?: string;
    AppName         ?: string;
    AppVersion      ?: string;
    ChangeCount     ?: number;
}

export interface UserDeviceDetailsSearchFilters extends BaseSearchFilters {
    UserId    ?: uuid;
    Token     ?: string;
    DeviceName?: string;
    OSType    ?: string;
    OSVersion ?: string;
    AppName   ?: string;
    AppVersion?: string;
}

export interface UserDeviceDetailsSearchResults extends BaseSearchResults {
    Items: UserDeviceDetailsDto[];
}
