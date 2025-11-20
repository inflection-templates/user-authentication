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
    Default
} from 'sequelize-typescript';

import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserMfa',
    tableName       : 'user_mfa',
    paranoid        : true,
    freezeTableName : true
})
export default class UserMfa extends Model {

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

    @Default(false)
    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    MfaEnabled: boolean;

    @Length({ max: 50 })
    @Column({
        type         : DataType.STRING(50),
        allowNull    : false,
        defaultValue : 'TOTP'
    })
    MfaType: string; // TOTP, SMS, EMAIL

    @Length({ max: 255 })
    @Column({
        type      : DataType.STRING(255),
        allowNull : true,
    })
    TotpSecret: string;

    @Column({
        type      : DataType.DATE,
        allowNull : true,
    })
    TotpSecretLastRotated: Date;

    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    BackupCodes: string; // JSON array of backup codes

    @Default(false)
    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    IsEmailVerified: boolean;

    @Default(false)
    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    IsPhoneVerified: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
