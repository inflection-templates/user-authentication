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
    ForeignKey,
    BelongsTo,
    Length,
} from 'sequelize-typescript';
import { v4 } from 'uuid';
import User from '../users/user.model';
import Permission from './permission.model';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserPermission',
    tableName       : 'user_permissions',
    paranoid        : true,
    freezeTableName : true
})
export default class UserPermission extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type         : DataType.UUID,
        defaultValue : () => { return v4(); },
        allowNull    : false
    })
    id: string;

    @IsUUID(4)
    @ForeignKey(() => User)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    UserId: string;

    @BelongsTo(() => User)
    User: User;

    @IsUUID(4)
    @ForeignKey(() => Permission)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    PermissionId: string;

    @BelongsTo(() => Permission)
    Permission: Permission;

    @Length({ max: 64 })
    @Column({
        type      : DataType.STRING(64),
        allowNull : false,
    })
    Scope: string;

    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : true,
    })
    Granted: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
