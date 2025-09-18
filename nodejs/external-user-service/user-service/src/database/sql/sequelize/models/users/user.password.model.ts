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

import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserPassword',
    tableName       : 'user_passwords',
    paranoid        : true,
    freezeTableName : true
})
export default class UserPassword extends Model {

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

    @Column({
        type      : DataType.STRING(256),
        allowNull : true,
    })
    PasswordHash: string;

    // @Column({
    //     type      : DataType.TEXT,
    //     allowNull : true,
    // })
    // PasswordSalt: string;

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
        defaultValue : false,
        allowNull    : false
    })
    Retired: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
