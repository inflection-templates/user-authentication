import express from 'express';
import { RoleCreateModel } from "../../../domain.types/authorization/role.types";
import { BaseValidator, Where } from '../../base.validator';
import { RoleSearchFilters } from "../../../domain.types/authorization/role.types";

///////////////////////////////////////////////////////////////////////////////////////

export class RoleValidator extends BaseValidator {

    constructor() {
        super();
    }

    create = async (request: express.Request): Promise<RoleCreateModel> => {

        await this.validateString(request, 'Name', Where.Body, true, false);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateUuid(request, 'TenantId', Where.Body, false, true);
        await this.validateUuid(request, 'ParentRoleId', Where.Body, false, true);
        await this.validateBoolean(request, 'IsSystemRole', Where.Body, false, true);

        this.validateRequest(request);

        const body = request.body;

        const model: RoleCreateModel = {
            Name         : body.Name ?? null,
            Description  : body.Description ?? null,
            TenantId     : body.TenantId ?? null,
            ParentRoleId : body.ParentRoleId ?? null,
            IsSystemRole : body.IsSystemRole ?? false,
        };
        return model;
    };

    search = async (request: express.Request): Promise<RoleSearchFilters> => {

        await this.validateString(request, 'name', Where.Query, false, false);
        await this.validateUuid(request, 'tenantId', Where.Query, false, false);
        await this.validateInt(request, 'parentRoleId', Where.Query, false, false);
        await this.validateBoolean(request, 'isSystemRole', Where.Query, false, false);
        await this.validateBaseSearchFilters(request);
        this.validateRequest(request);

        const model: RoleSearchFilters = {
            Name         : request.query.name as string ?? null,
            ParentRoleId : request.query.parentRoleId as string ?? null,
            IsSystemRole : request.query.isSystemRole ? request.query.isSystemRole as string === 'true' : null,
            TenantId     : request.query.tenantId as string ?? null,
        };
        return this.updateBaseSearchFilters(request, model);
    };

    update = async (request: express.Request): Promise<RoleCreateModel> => {

        await this.validateString(request, 'Name', Where.Body, true, false);
        await this.validateString(request, 'Description', Where.Body, false, true);
        await this.validateUuid(request, 'TenantId', Where.Body, false, true);
        await this.validateUuid(request, 'ParentRoleId', Where.Body, false, true);
        await this.validateBoolean(request, 'IsSystemRole', Where.Body, false, true);

        this.validateRequest(request);

        const body = request.body;

        const model: RoleCreateModel = {
            Name         : body.Name ?? null,
            Description  : body.Description ?? null,
            TenantId     : body.TenantId ?? null,
            ParentRoleId : body.ParentRoleId ?? null,
            IsSystemRole : body.IsSystemRole ?? false,
        };
        return model;
    };

}
