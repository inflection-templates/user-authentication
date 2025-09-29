import * as asyncLib from 'async';
import axios from 'axios';
import { logger } from "../../logger/logger";
import { AnalyticsEvent } from "./analytics.types";
import { UserDto } from '../../domain.types/users/user.types';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { TimeHelper } from '../../common/time.helper';
import { DurationType } from '../../domain.types/miscellaneous/time.types';
import { ConfigurationManager } from '../../config/configuration.manager';

////////////////////////////////////////////////////////////////////////////////

export class AnalyticsHandler {

    private static _numAsyncTasks = 4;

    private static _eventQueue = asyncLib.queue((event: AnalyticsEvent, onCompleted) => {
        (async () => {
            await AnalyticsHandler.addEvent(event);
            onCompleted();
        })();
    }, AnalyticsHandler._numAsyncTasks);

    private static _addUserQueue = asyncLib.queue((user: UserDto, onCompleted) => {
        (async () => {
            await AnalyticsHandler.addNewUser(user);
            onCompleted();
        })();
    }, AnalyticsHandler._numAsyncTasks);

    private static _deleteUserQueue = asyncLib.queue((userId: uuid, onCompleted) => {
        (async () => {
            await AnalyticsHandler.deleteExistingUser(userId);
            onCompleted();
        })();
    }, AnalyticsHandler._numAsyncTasks);

    private static async addEvent(event: AnalyticsEvent): Promise<void> {
        try {
            if (!ConfigurationManager.AnalyticsEnabled) {
                return;
            }
            const apiKey = process.env.ANALYTICS_API_KEY;
            const headers = {
                'Content-Type'    : 'application/json',
                Accept            : '*/*',
                'Cache-Control'   : 'no-cache',
                'Accept-Encoding' : 'gzip, deflate, br',
                Connection        : 'keep-alive',
                'x-api-key'       : apiKey,
            };

            var url = ConfigurationManager.AnalyticsEndpoint + '/events/';
            var body = {
                ...event,
            };
            var response = await axios.post(url, body, { headers });
            if (response.status === 201) {
                logger.info('Successfully pushed analytics event!');
            } else {
                logger.error('Unable to push analytics event!');
            }
        } catch (error) {
            logger.info(`${error.message}`);
        }
    }

    private static async addNewUser(user: UserDto): Promise<void> {
        try {
            if (!ConfigurationManager.AnalyticsEnabled) {
                return;
            }
            const apiKey = process.env.ANALYTICS_API_KEY;
            const headers = {
                'Content-Type'    : 'application/json',
                Accept            : '*/*',
                'Cache-Control'   : 'no-cache',
                'Accept-Encoding' : 'gzip, deflate, br',
                Connection        : 'keep-alive',
                'x-api-key'       : apiKey,
            };

            const tenantId = user.Tenant?.id ?? null;

            const timezoneOffsetMinutes = user.Metadata?.CurrentTimeZone ?
                TimeHelper.getTimezoneOffsets(user.Metadata.CurrentTimeZone, DurationType.Minute) : null;

            var url = ConfigurationManager.AnalyticsEndpoint + '/users';
            var body = {
                id                : user.id,
                TenantId          : tenantId,
                RoleId            : [],
                OnboardingSource  : 'Unknown',
                TimezoneOffsetMin : timezoneOffsetMinutes,
                RegistrationDate  : user.CreatedAt ?? new Date(),
            };
            var response = await axios.post(url, body, { headers });
            if (response.status === 201) {
                logger.info('Successfully pushed analytics user!');
            } else {
                logger.error('Unable to push analytics user!');
            }
        } catch (error) {
            logger.info(`${error.message}`);
        }
    }

    private static async deleteExistingUser(userId: uuid): Promise<void> {
        try {
            if (!ConfigurationManager.AnalyticsEnabled) {
                return;
            }
            const apiKey = process.env.ANALYTICS_API_KEY;
            const headers = {
                'Content-Type'    : 'application/json',
                Accept            : '*/*',
                'Cache-Control'   : 'no-cache',
                'Accept-Encoding' : 'gzip, deflate, br',
                Connection        : 'keep-alive',
                'x-api-key'       : apiKey,
            };

            var url = ConfigurationManager.AnalyticsEndpoint + '/users/' + userId;
            var response = await axios.delete(url, { headers });
            if (response.status === 200) {
                logger.info('Successfully deleted analytics user!');
            } else {
                logger.error('Unable to delete analytics user!');
            }
        } catch (error) {
            logger.info(`${error.message}`);
        }
    }

    // Push an event to the queue. This is a non-blocking call.
    public static pushEvent(event: AnalyticsEvent): void {
        AnalyticsHandler._eventQueue.push(event, (err) => {
            if (err) {
                logger.info('Error pushing event:' + err.message);
            }
        });
    }

    // Create a user
    public static createUser(user: UserDto): void {
        AnalyticsHandler._addUserQueue.push(user, (err) => {
            if (err) {
                logger.info('Error creating user:' + err.message);
            }
        });
    }

    // Delete a user
    public static deleteUser(userId: uuid): void {
        AnalyticsHandler._deleteUserQueue.push(userId, (err) => {
            if (err) {
                logger.info('Error deleting user:' + err.message);
            }
        });
    }

}
