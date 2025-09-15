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
    ForeignKey,
    HasOne
} from 'sequelize-typescript';
import Tenant from '../tenant/tenant.model';
import { v4 } from 'uuid';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'Role',
    tableName       : 'roles',
    paranoid        : true,
    freezeTableName : true
})
export default class Role extends Model {

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

    @Length({ min: 1, max: 32 })
    @Column({
        type      : DataType.STRING(32),
        allowNull : false,
        unique    : true
    })
    Name: string;

    @Length({ min: 1, max: 256 })
    @Column({
        type      : DataType.STRING(256),
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

    @IsUUID(4)
    @ForeignKey(() => Role)
    @Column({
        type         : DataType.UUID,
        allowNull    : true,
        defaultValue : null
    })
    ParentRoleId: string;

    @HasOne(() => Role)
    ParentRole: Role;

    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false,
    })
    IsSystemRole: boolean;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
