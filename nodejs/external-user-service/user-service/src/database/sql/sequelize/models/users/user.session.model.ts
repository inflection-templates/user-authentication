import {
    BelongsTo,
    Column, CreatedAt, DataType, DeletedAt, ForeignKey, IsDate, IsUUID,
    Model, PrimaryKey, Table, UpdatedAt
} from 'sequelize-typescript';
import { v4 } from 'uuid';
import User from './user.model';
import Tenant from '../tenant/tenant.model';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserSession',
    tableName       : 'user_sessions',
    paranoid        : true,
    freezeTableName : true,
})
export default class UserSession extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type         : DataType.UUID,
        defaultValue : () => {
            return v4();
        },
        allowNull : false,
    })
    id: string;

    @IsUUID(4)
    @ForeignKey(() => User)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    UserId: string;

    @IsUUID(4)
    @ForeignKey(() => Tenant)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    TenantId: string;

    @BelongsTo(() => Tenant)
    Tenant: Tenant;

    @BelongsTo(() => User)
    User: User;

    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : true,
    })
    IsActive: boolean;

    @IsDate
    @Column({
        type      : DataType.DATE,
        allowNull : true,
    })
    StartedAt: Date;

    @IsDate
    @Column({
        type      : DataType.DATE,
        allowNull : true,
    })
    ValidTill: Date;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
