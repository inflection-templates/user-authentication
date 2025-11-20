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
    modelName       : 'UserMfaTempSecret',
    tableName       : 'user_mfa_temp_secrets',
    paranoid        : true,
    freezeTableName : true
})
export default class UserMfaTempSecret extends Model {

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

    @Length({ min: 1, max: 255 })
    @Column({
        type      : DataType.STRING(255),
        allowNull : false,
    })
    Secret: string;

    @Length({ max: 50 })
    @Column({
        type         : DataType.STRING(50),
        allowNull    : false,
        defaultValue : 'TOTP'
    })
    SecretType: string; // TOTP, SMS, EMAIL

    @Column({
        type      : DataType.DATE,
        allowNull : false,
    })
    ExpiresAt: Date;

    @Default(false)
    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    IsUsed: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
