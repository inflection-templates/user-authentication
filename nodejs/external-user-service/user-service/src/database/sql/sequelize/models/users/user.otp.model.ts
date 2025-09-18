import {
    Table,
    Column,
    Model,
    DataType,
    CreatedAt,
    UpdatedAt,
    DeletedAt,
    IsUUID,
    PrimaryKey,
    Length,
    IsDate    } from 'sequelize-typescript';

import { v4 } from 'uuid';
import {
    OTPChannel,
    OTPChannels,
    OTPScope,
    OTPScopes
} from '../../../../../domain.types/users/user.enums';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserOtp',
    tableName       : 'user_otps',
    paranoid        : true,
    freezeTableName : true
})
export default class UserOtp extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type         : DataType.UUID,
        defaultValue : () => { return v4(); },
        allowNull    : false
    })
    id: string;

    @IsUUID(4)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    UserId: string;

    // This is a enum
    @Column({
        type      : DataType.ENUM(...OTPScopes),
        allowNull : false,
    })
    Scope: OTPScope;

    @Column({
        type      : DataType.ENUM(...OTPChannels),
        allowNull : false,
    })
    Channel: OTPChannel;

    @Length({ min: 6, max: 8 })
    @Column({
        type      : DataType.STRING(8),
        allowNull : false,
    })
    Otp: string;

    @IsDate
    @Column({
        type      : DataType.DATE,
        allowNull : false
    })
    ValidFrom: Date;

    @IsDate
    @Column({
        type      : DataType.DATE,
        allowNull : false
    })
    ValidTill: Date;

    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    Validated: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
