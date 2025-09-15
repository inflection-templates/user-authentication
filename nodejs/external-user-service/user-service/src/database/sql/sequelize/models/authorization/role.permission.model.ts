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
import Role from './role.model';
import Permission from './permission.model';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'RolePermission',
    tableName       : 'role_permissions',
    paranoid        : true,
    freezeTableName : true
})
export default class RolePermission extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type         : DataType.UUID,
        defaultValue : () => { return v4(); },
        allowNull    : false
    })
    id: string;

    @IsUUID(4)
    @ForeignKey(() => Role)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    RoleId: string;

    @BelongsTo(() => Role)
    Role: Role;

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
        defaultValue : false,
    })
    Enabled: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
