import express from 'express';
import { BaseValidator, Where } from '../../base.validator';

///////////////////////////////////////////////////////////////////////////////////////

export class UserPermissionValidator extends BaseValidator {

    constructor() {
        super();
    }

    addPermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        await this.validateString(request, 'Scope', Where.Body, false, false);
        this.validateRequest(request);
    };

    updatePermissionScope = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        await this.validateString(request, 'Scope', Where.Body, false, false);
        this.validateRequest(request);
    };

    getPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        this.validateRequest(request);
    };

    removePermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

    grantPermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

    revokePermission = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        await this.validateUuid(request, 'permissionId', Where.Param, true, false);
        this.validateRequest(request);
    };

    getGrantedPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        this.validateRequest(request);
    };

    getRevokedPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        this.validateRequest(request);
    };

    getRoleBasedPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        this.validateRequest(request);
    };

    getAllPermissions = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'userId', Where.Param, true, false);
        this.validateRequest(request);
    };

}
