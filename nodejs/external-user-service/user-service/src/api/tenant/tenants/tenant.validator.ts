import express from 'express';
import {
    TenantSearchFilters,
    TenantCreateModel,
    TenantUpdateModel
} from "../../../domain.types/tenant/tenant.types";
import { BaseValidator, Where } from '../../base.validator';

///////////////////////////////////////////////////////////////////////////////////////

export class TenantValidator extends BaseValidator {

    constructor() {
        super();
    }

    create = async (request: express.Request): Promise<TenantCreateModel> => {

        await this.validateString(request, 'Name', Where.Body, true, false);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateString(request, 'TenantCode', Where.Body, false, true);
        await this.validateString(request, 'PhoneNumber', Where.Body, false, false);
        await this.validateString(request, 'Email', Where.Body, false, false);
        await this.validateString(request, 'UserName', Where.Body, false, false);
        await this.validateString(request, 'Password', Where.Body, false, false);

        this.validateRequest(request);

        const body = request.body;

        const model: TenantCreateModel = {
            Name        : body.Name ?? null,
            Description : body.Description ?? null,
            TenantCode  : body.TenantCode ?? null,
            PhoneCode   : body.PhoneCode ?? null,
            PhoneNumber : body.PhoneNumber ?? null,
            Email       : body.Email ?? null,
            UserName    : body.UserName ?? null,
            Password    : body.Password ?? null,
        };
        return model;
    };

    update = async (request: express.Request): Promise<TenantUpdateModel> => {
        await this.validateString(request, 'Name', Where.Body, false, true);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateString(request, 'TenantCode', Where.Body, false, true);
        await this.validateString(request, 'PhoneNumber', Where.Body, false, false);
        await this.validateString(request, 'Email', Where.Body, false, false);
        this.validateRequest(request);

        const body = request.body;

        const model: TenantUpdateModel = {
            id          : await this.getParamUuid(request, 'id'),
            Name        : body.Name ?? null,
            Description : body.Description ?? null,
            TenantCode  : body.TenantCode ?? null,
            PhoneCode   : body.PhoneCode ?? null,
            PhoneNumber : body.PhoneNumber ?? null,
            Email       : body.Email ?? null,
        };
        return model;
    };

    search = async (request: express.Request): Promise<TenantSearchFilters> => {

        await this.validateString(request, 'name', Where.Query, false, false);
        await this.validateString(request, 'code', Where.Query, false, false);
        await this.validateString(request, 'phone', Where.Query, false, false);
        await this.validateString(request, 'email', Where.Query, false, false);
        await this.validateBaseSearchFilters(request);

        this.validateRequest(request);

        const filters: TenantSearchFilters = {
            Name        : request.query.name as string ?? null,
            TenantCode  : request.query.code as string  ?? null,
            PhoneNumber : request.query.phone as string  ?? null,
            Email       : request.query.email as string  ?? null,
        };
        return this.updateBaseSearchFilters(request, filters);
    };

}
