import express from 'express';
import {
    AnalyticsEvent,
    AnalyticsEventCategory,
    AnalyticsEventSubject,
    AnalyticsEventType
} from '../../modules/analytics/analytics.types';
import { AnalyticsHandler } from '../../modules/analytics/analytics.handler';
import { UserDto } from '../../domain.types/users/user.types';
import { getCommonEventParams } from '../../modules/analytics/analytics.common';
import { logger } from '../../logger/logger';

///////////////////////////////////////////////////////////////////////////////////////

export class UserEvents {

    static async onUserCreated(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                return;
            }
            const message = `User '${user.id}' account is created.`;
            const eventName = AnalyticsEventType.UserCreate;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserUpdated(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserUpdate;
            const message = `User '${user.id}' account is updated.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserDeleted(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserDelete;
            const message = `User '${user.id}' account is deleted.`;
            UserEvents.logEvent(request, user, eventName, message);
            AnalyticsHandler.deleteUser(user.id);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserLoginWithPassword(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserLoginWithPassword;
            const message = `User '${user.id}' logged in using pasword.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserLoginWithOtp(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserLoginWithOtp;
            const message = `User '${user.id}' logged in using OTP.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserLogout(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserLogout;
            const message = `User '${user.id}' logged out.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserPasswordChanged(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserPasswordChange;
            const message = `User '${user.id}' changed password.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onUserPasswordReset(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserPasswordReset;
            const message = `User '${user.id}' password reset.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async sendResetPasswordByEmailLink(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserSendPasswordResetLink;
            const message = `User '${user.id}' password reset link sent.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async sendResetPasswordByPhoneOtp(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserSendPasswordResetPhoneOtp;
            const message = `User '${user.id}' password reset requested.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async sendResetPasswordByEmailOtp(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserSendPasswordResetEmailOtp;
            const message = `User '${user.id}' password reset requested.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    static async onGenerateOtp(request: express.Request, user: UserDto) {
        try {
            if (!user) {
                return;
            }
            if (user.Metadata?.IsTestUser) {
                // Do not record test user events
                return;
            }
            const eventName = AnalyticsEventType.UserGenerateOtp;
            const message = `User '${user.id}' OTP generated.`;
            UserEvents.logEvent(request, user, eventName, message);
        } catch (error) {
            logger.info(error?.message);
        }
    }

    private static logEvent(
        request: express.Request, user: UserDto, eventName: AnalyticsEventType, message: string) {
        const params = getCommonEventParams(request, user);
        const eventSubject = AnalyticsEventSubject.UserAccount;
        const eventCategory = AnalyticsEventCategory.UserAccount;
        const event: AnalyticsEvent = {
            ...params,
            ResourceId      : user.id,
            ResourceType    : 'user',
            SourceVersion   : null,
            EventName       : eventName,
            EventSubject    : eventSubject,
            EventCategory   : eventCategory,
            ActionStatement : message,
            Attributes      : null
        };
        AnalyticsHandler.pushEvent(event);
    }

}

