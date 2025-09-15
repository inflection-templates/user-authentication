import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { UserPasswordDto } from '../../../domain.types/users/user.password.change.types';
////////////////////////////////////////////////////////////////////////////////////

export interface IUserPasswordRepo
{
    isCurrentPasswordExpired(userId: uuid): Promise<boolean>;

    getUserCurrentHashedPassword(userId: uuid): Promise<UserPasswordDto>;

    updateCurrentUserHashedPassword(userId: uuid, newHashedPassword: string): Promise<void>;

    checkPastPasswordForSimilarity(userId: uuid, newHashedPassword: string, numberOfPastPasswords: number): Promise<boolean>;

}
