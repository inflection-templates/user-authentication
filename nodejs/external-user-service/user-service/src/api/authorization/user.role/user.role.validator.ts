import express from 'express';
import { BaseValidator, Where } from '../../base.validator';

///////////////////////////////////////////////////////////////////////////////////////

export class UserRoleValidator extends BaseValidator {

    constructor() {
        super();
    }

    addUserRole = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'UserId', Where.Param, true, false);
        await this.validateUuid(request, 'RoleId', Where.Param, true, false);
        this.validateRequest(request);
    };

    removeUserRole = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'UserId', Where.Param, true, false);
        await this.validateUuid(request, 'RoleId', Where.Param, true, false);
        this.validateRequest(request);
    };

    getUserRoles = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'UserId', Where.Param, true, false);
        this.validateRequest(request);
    };

    removeAllUserRoles = async (request: express.Request): Promise<void> => {
        await this.validateUuid(request, 'UserId', Where.Param, true, false);
        this.validateRequest(request);
    };

}
