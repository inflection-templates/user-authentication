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
    IsDate
} from 'sequelize-typescript';

import {
    OAuthProviders,
    OAuthProvider
} from '../../../../../domain.types/users/user.enums';

import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserOAuth',
    tableName       : 'user_oauth',
    paranoid        : true,
    freezeTableName : true
})
export default class UserOAuth extends Model {

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

    // Make this a enum
    @Column({
        type      : DataType.ENUM(...OAuthProviders),
        allowNull : false,
    })
    Provider: OAuthProvider;

    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    AccessToken: string;

    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    SessionId: string;

    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    Claims: string;

    // This is a response payload from the OAuth provider
    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    Metadata: string;

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
    IsExpired: boolean;

    // This is generally optional
    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    RefreshToken: string;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
