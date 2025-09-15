import {
    BelongsTo,
    Column,
    CreatedAt,
    DataType,
    DeletedAt,
    ForeignKey,
    IsUUID,
    Model,
    PrimaryKey,
    Table,
    UpdatedAt
} from 'sequelize-typescript';
import Role from './role.model';
import User from '../users/user.model';
import Tenant from '../tenant/tenant.model';
import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserRole',
    tableName       : 'user_roles',
    paranoid        : true,
    freezeTableName : true
})
export default class UserRole extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type         : DataType.UUID,
        allowNull    : false,
        defaultValue : () => {
            return v4();
        },
    })
    id: string;

    @IsUUID(4)
    @ForeignKey(() => Tenant)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    TenantId: string;

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
    @ForeignKey(() => Role)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    RoleId: string;

    @BelongsTo(() => Role)
    Role: Role;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
