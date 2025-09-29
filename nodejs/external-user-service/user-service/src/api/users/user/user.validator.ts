import express from 'express';
import Joi from 'joi';
import { ErrorHandler } from '../../../common/handlers/error.handler';
import { UserCreateModel, UserSearchFilters, UserUpdateModel, UserUniqueIdentifiers } from '../../../domain.types/users/user.types';
import { Gender, uuid } from '../../../domain.types/miscellaneous/system.types';
import { InputValidationError } from '../../../common/input.validation.error';
import { SupportedLanguage } from '../../../domain.types/users/user.enums';

///////////////////////////////////////////////////////////////////////////////////////

export class UserValidator {

    static create = async (request: express.Request): Promise<UserCreateModel> => {
        try {
            //make sure either email or phone code + phone number is provided
            const canCreateUser = request.body.Email || (request.body.PhoneCode && request.body.PhoneNumber);
            if (!canCreateUser) {
                throw new InputValidationError(['Either email or phone code + phone number is required']);
            }

            const schema = Joi.object({
                TenantId        : Joi.string().uuid().optional(),
                Prefix          : Joi.string().optional().trim(),
                FirstName       : Joi.string().optional().trim(),
                MiddleName      : Joi.string().optional().trim(),
                LastName        : Joi.string().optional().trim(),
                PhoneNumber     : Joi.string().optional().trim(),
                PhoneCode       : Joi.string().optional().trim(),
                Email           : Joi.string().optional().trim(),
                UserName        : Joi.string().optional().trim(),
                ProfileImageUrl : Joi.string().uri().optional().trim(),
                Password        : Joi.string().optional().trim()
                    .min(8)
                    .regex(/[a-z]/) // At least one lowercase
                    .regex(/[A-Z]/) // At least one uppercase
                    .regex(/[0-9]/) // At least one number
                    .regex(/[^a-zA-Z0-9]/) // At least one symbol
                    .message('Password must contain at least 8 characters including one lowercase letter, one uppercase letter, one number, and one symbol'),
                DefaultTimeZone   : Joi.string().optional().trim(),
                CurrentTimeZone   : Joi.string().optional().trim(),
                IsTestUser        : Joi.boolean().optional(),
                Gender            : Joi.string().optional().valid(...Object.values(Gender)),
                DateOfBirth       : Joi.string().isoDate().optional(),
                PreferredLanguage : Joi.string().optional().valid(...Object.values(SupportedLanguage)),
                RoleIds           : Joi.array().items(Joi.string().uuid()).optional()
            });

            await schema.validateAsync(request.body);

            return UserValidator.getCreateModel(request);
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    private static getCreateModel(request): UserCreateModel {
        if (typeof request.body.RoleId === 'string') {
            request.body.RoleId = parseInt(request.body.RoleId);
        }
        const model: UserCreateModel = {
            TenantId          : request.body.TenantId ?? null,
            Prefix            : request.body.Prefix ?? null,
            FirstName         : request.body.FirstName,
            MiddleName        : request.body.MiddleName ?? null,
            LastName          : request.body.LastName,
            PhoneCode         : request.body.PhoneCode ?? null,
            PhoneNumber       : request.body.PhoneNumber ?? null,
            Email             : request.body.Email ?? null,
            ProfileImageUrl   : request.body.ProfileImageUrl ?? null,
            UserName          : request.body.UserName ?? null,
            Password          : request.body.Password,
            CurrentTimeZone   : request.body.CurrentTimeZone ?? '+05:30',
            DefaultTimeZone   : request.body.DefaultTimeZone ?? '+05:30',
            IsTestUser        : request.body.IsTestUser ?? false,
            Gender            : request.body.Gender ?? null,
            DateOfBirth       : request.body.DateOfBirth ? new Date(request.body.DateOfBirth) : null,
            PreferredLanguage : request.body.PreferredLanguage ?? SupportedLanguage.English,
            UserSettings      : request.body.UserSettings ?? '{}',
        };

        return model;
    }

    static update = async (request: express.Request): Promise<UserUpdateModel> => {
        try {
            const schema = Joi.object({
                RoleId            : Joi.number().optional(),
                Prefix            : Joi.string().optional().trim(),
                FirstName         : Joi.string().optional().trim(),
                MiddleName        : Joi.string().optional().trim(),
                LastName          : Joi.string().optional().trim(),
                PhoneNumber       : Joi.string().optional().trim(),
                Email             : Joi.string().optional().trim(),
                DefaultTimeZone   : Joi.string().optional().trim(),
                CurrentTimeZone   : Joi.string().optional().trim(),
                PreferredLanguage : Joi.string().optional().valid(...Object.values(SupportedLanguage)),
                ImageResourceId   : Joi.string().uuid().optional()
            });

            await schema.validateAsync(request.body);

            return UserValidator.getUpdateModel(request);
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static getUpdateModel = (request): UserUpdateModel => {
        const model: UserUpdateModel = {
            Prefix          : request.body.Prefix ?? null,
            FirstName       : request.body.FirstName,
            MiddleName      : request.body.MiddleName ?? null,
            LastName        : request.body.LastName,
            PhoneNumber     : request.body.PhoneNumber ?? null,
            Email           : request.body.Email ?? null,
            ProfileImageUrl : request.body.ProfileImageUrl ?? null,
        };
        return model;
    };

    static getById = async (request: express.Request): Promise<string> => {
        try {
            const schema = Joi.object({
                id : Joi.string().uuid().required()
            });

            await schema.validateAsync(request.params);

            return request.params.id;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    static search = async (request: express.Request): Promise<UserSearchFilters> => {
        try {
            const schema = Joi.object({
                phone           : Joi.string().optional().trim(),
                email           : Joi.string().optional().trim(),
                userId          : Joi.string().uuid().optional(),
                name            : Joi.string().optional().trim(),
                gender          : Joi.string().optional().trim(),
                userName        : Joi.string().optional().trim(),
                roleIds         : Joi.string().optional().trim(),
                createdDateFrom : Joi.date().optional(),
                createdDateTo   : Joi.date().optional(),
                orderBy         : Joi.string().optional().trim(),
                order           : Joi.string().optional().trim(),
                pageIndex       : Joi.number().integer().optional(),
                itemsPerPage    : Joi.number().integer().optional(),
                full            : Joi.boolean().optional()
            });

            await schema.validateAsync(request.query);

            return UserValidator.getFilter(request);
        } catch (error) {
            throw new InputValidationError(error.message);
        }
    };

    private static getFilter(request: express.Request): UserSearchFilters {
        const pageIndex = request.query.pageIndex !== 'undefined' ? parseInt(request.query.pageIndex as string, 10) : 0;
        const itemsPerPage = request.query.itemsPerPage !== 'undefined' ? parseInt(request.query.itemsPerPage as string, 10) : 25;
        const tenantId: uuid = request.currentUserTenantId ?? null;

        const filters: UserSearchFilters = {
            TenantId        : tenantId,
            PhoneNumber     : request.query.phone as string ?? null,
            Email           : request.query.email as string ?? null,
            UserId          : request.query.userId as string ?? null,
            Name            : request.query.name as string ?? null,
            Gender          : request.query.gender as Gender ?? null,
            UserName        : request.query.userName as string ?? null,
            CreatedDateFrom : request.query.createdDateFrom ? new Date(request.query.createdDateFrom as string) : null,
            CreatedDateTo   : request.query.createdDateTo ? new Date(request.query.createdDateTo as string) : null,
            OrderBy         : request.query.orderBy as string ?? 'CreatedAt',
            Order           : request.query.order as string ?? 'descending',
            PageIndex       : pageIndex,
            ItemsPerPage    : itemsPerPage,
        };
        return filters;
    }

    static userExists = async (request: express.Request): Promise<UserUniqueIdentifiers> => {
        try {
            const schema = Joi.object({
                PhoneCode   : Joi.string().optional().trim(),
                PhoneNumber : Joi.string().optional().trim(),
                Email       : Joi.string().optional().trim(),
                UserName    : Joi.string().optional().trim()
            });

            await schema.validateAsync(request.body);

            const model: UserUniqueIdentifiers = {
                PhoneCode   : request.body.PhoneCode ?? null,
                PhoneNumber : request.body.PhoneNumber ?? null,
                Email       : request.body.Email ?? null,
                UserName    : request.body.UserName ?? null,
            };

            return model;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

}
