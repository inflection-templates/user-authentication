import express from 'express';
import { PermissionCreateModel } from "../../../domain.types/authorization/permission.types";
import { BaseValidator, Where } from '../../base.validator';
import { PermissionSearchFilters } from "../../../domain.types/authorization/permission.types";

///////////////////////////////////////////////////////////////////////////////////////

export class PermissionValidator extends BaseValidator {

    constructor() {
        super();
    }

    create = async (request: express.Request): Promise<PermissionCreateModel> => {

        await this.validateString(request, 'Name', Where.Body, true, false);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateUuid(request, 'TenantId', Where.Body, false, true);
        await this.validateString(request, 'Module', Where.Body, false, true);

        this.validateRequest(request);

        const body = request.body;

        const model: PermissionCreateModel = {
            Name        : body.Name ?? null,
            Description : body.Description ?? null,
            TenantId    : body.TenantId ?? null,
            Module      : body.Module ?? null,
        };
        return model;
    };

    search = async (request: express.Request): Promise<PermissionSearchFilters> => {

        await this.validateString(request, 'name', Where.Query, false, false);
        await this.validateUuid(request, 'tenantId', Where.Query, false, false);
        await this.validateString(request, 'module', Where.Query, false, false);
        await this.validateBaseSearchFilters(request);
        this.validateRequest(request);

        const model: PermissionSearchFilters = {
            Name     : request.query.name as string ?? null,
            TenantId : request.query.tenantId as string ?? null,
            Module   : request.query.module as string ?? null,
        };
        return this.updateBaseSearchFilters(request, model);
    };

    update = async (request: express.Request): Promise<PermissionCreateModel> => {

        await this.validateString(request, 'Name', Where.Body, true, false);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateUuid(request, 'TenantId', Where.Body, false, true);
        await this.validateString(request, 'Module', Where.Body, false, true);

        this.validateRequest(request);

        const body = request.body;

        const model: PermissionCreateModel = {
            Name        : body.Name ?? null,
            Description : body.Description ?? null,
            TenantId    : body.TenantId ?? null,
            Module      : body.Module ?? null,
        };
        return model;
    };

}
