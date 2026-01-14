import { NextFunction, Request, Response } from 'express';

////////////////////////////////////////////////////////////////////////////////////////////////

export type Middleware =
    (request: Request, response: Response, next: NextFunction)
        => Promise<void>;
