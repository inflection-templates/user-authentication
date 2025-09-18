import Tenant from '../../models/tenant/tenant.model';
import { TenantDto } from "../../../../../domain.types/tenant/tenant.types";

///////////////////////////////////////////////////////////////////////////////////

export class TenantMapper {

    static toDto = (tenant: Tenant): TenantDto => {
        if (tenant == null){
            return null;
        }
        const dto: TenantDto = {
            id          : tenant.id,
            Name        : tenant.Name,
            Description : tenant.Description,
            TenantCode  : tenant.TenantCode,
            PhoneCode   : tenant.PhoneCode,
            PhoneNumber : tenant.PhoneNumber,
            Email       : tenant.Email,
        };
        return dto;
    };

}
