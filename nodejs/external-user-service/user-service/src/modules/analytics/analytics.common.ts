import express from 'express';
import { UserDto } from "../../domain.types/users/user.types";

////////////////////////////////////////////////////////////////////////////////

export const getCommonEventParams = (request: express.Request, user?: UserDto) => {
    const sourceName = request.currentClient?.ClientName ?? 'Unknown';
    const userId     = user?.id ?? request.currentUser.UserId ?? null;
    const tenantId   = user?.Tenant?.id ?? request.currentUser.TenantId ?? null;
    const sessionId  = request.currentUser?.SessionId ?? null;
    const actionType = 'user-action';
    return {
        UserId        : userId,
        TenantId      : tenantId,
        SessionId     : sessionId,
        SourceName    : sourceName,
        SourceVersion : 'unknown',
        ActionType    : actionType,
        Timestamp     : new Date()
    };

};
