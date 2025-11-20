import { ITenantRepo } from '../../database/repository.interfaces/tenant/tenant.repo.interface';
import { injectable, inject } from 'tsyringe';
import {
    TenantCreateModel,
    TenantDto,
    TenantSearchFilters,
    TenantSearchResults,
    TenantUpdateModel
} from "../../domain.types/tenant/tenant.types";
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { logger } from '../../logger/logger';

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class TenantService {

    constructor(
        @inject('ITenantRepo') private _tenantRepo: ITenantRepo,
    ) {}

    //#region Publics

    create = async (model: TenantCreateModel): Promise<TenantDto> => {
        return await this._tenantRepo.create(model);
    };

    public getById = async (id: uuid): Promise<TenantDto> => {
        return await this._tenantRepo.getById(id);
    };

    public getByCode = async (code: string): Promise<TenantDto> => {
        return await this._tenantRepo.getByCode(code);
    };

    public exists = async (id: uuid): Promise<boolean> => {
        return await this._tenantRepo.exists(id);
    };

    public search = async (filters: TenantSearchFilters): Promise<TenantSearchResults> => {
        var dtos = await this._tenantRepo.search(filters);
        return dtos;
    };

    public update = async (id: uuid, model: TenantUpdateModel): Promise<TenantDto> => {
        return await this._tenantRepo.update(id, model);
    };

    public delete = async (id: uuid, hardDelete: boolean = false): Promise<boolean> => {
        return await this._tenantRepo.delete(id, hardDelete);
    };

    public getTenantWithPhone = async (phone: string): Promise<TenantDto> => {
        return await this._tenantRepo.getTenantWithPhone(phone);
    };

    public getTenantWithCode = async (code: string): Promise<TenantDto> => {
        return await this._tenantRepo.getTenantWithCode(code);
    };

    public getTenantWithEmail = async (email: string): Promise<TenantDto> => {
        return await this._tenantRepo.getTenantWithEmail(email);
    };


    public seedDefaultTenant = async (): Promise<TenantDto> => {
        var defaultTenant = await this._tenantRepo.getTenantWithCode('default');
        if (defaultTenant == null) {

            const tenantEmail = process.env.PLATFORM_SUPPORT_EMAIL;
            if (!tenantEmail) {
                logger.info('PLATFORM_SUPPORT_EMAIL is not set in the environment variables');
            }

            var tenant: TenantCreateModel = {
                Name        : 'Default',
                Description : 'Default Tenant',
                TenantCode  : 'default',
                PhoneCode   : '+91',
                PhoneNumber : '0000000000',
                Email       : tenantEmail ?? 'support@gmail.com',
            };
            var defaultTenant = await this._tenantRepo.create(tenant);
            return defaultTenant;
        }
        else {
            var defaultTenant = await this._tenantRepo.getTenantWithCode('default');
            return defaultTenant;
        }
    };

    //#endregion

}
