import {
    Table,
    Column,
    Model,
    DataType,
    CreatedAt,
    UpdatedAt,
    DeletedAt,
    PrimaryKey,
    Length,
    IsUUID,
    ForeignKey
} from 'sequelize-typescript';
import Tenant from '../tenant/tenant.model';
import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'Permission',
    tableName       : 'permissions',
    paranoid        : true,
    freezeTableName : true
})
export default class Permission extends Model {

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

    @Length({ min: 1, max: 512 })
    @Column({
        type      : DataType.STRING(512),
        allowNull : false,
        unique    : true
    })
    Name: string;

    @Length({ min: 1, max: 1024 })
    @Column({
        type      : DataType.STRING(1024),
        allowNull : true,
    })
    Description: string;

    @IsUUID(4)
    @ForeignKey(() => Tenant)
    @Column({
        type         : DataType.UUID,
        allowNull    : true,
        defaultValue : null
    })
    TenantId: string;

    @Length({ min: 1, max: 32 })
    @Column({
        type      : DataType.STRING(32),
        allowNull : true,
    })
    Module: string;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
