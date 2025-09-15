import express from 'express';
import { BaseValidator, Where } from '../../base.validator';

///////////////////////////////////////////////////////////////////////////////////////

export class RolePermissionValidator extends BaseValidator {

    constructor() {
        super();
    }

    addPermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        await this.validateString(request, 'Scope', Where.Body, false, false);
        this.validateRequest(request);
    };

    updatePermissionScope = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        await this.validateString(request, 'Scope', Where.Body, false, false);
        this.validateRequest(request);
    };

    getPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        this.validateRequest(request);
    };

    removePermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

    enablePermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

    disablePermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'roleId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

}
