import {
    Column,
    CreatedAt,
    DataType,
    DeletedAt,
    Index,
    IsEmail,
    IsUrl,
    IsUUID,
    Length,
    Model,
    PrimaryKey,
    Table,
    UpdatedAt
} from 'sequelize-typescript';
import { v4 } from 'uuid';
import { Gender, Genders } from '../../../../../domain.types/miscellaneous/system.types';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'User',
    tableName       : 'users',
    paranoid        : true,
    freezeTableName : true,
})
export default class User extends Model {

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

    @Length({ min: 1, max: 32 })
    @Column({
        type      : DataType.STRING(32),
        allowNull : true,
        unique    : true
    })
    UserName: string;

    @IsUUID(4)
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    TenantId: string;

    @Length({ max: 16 })
    @Column({
        type      : DataType.STRING(16),
        allowNull : true,
    })
    Prefix: string;

    @Length({ max: 70 })
    @Column({
        type      : DataType.STRING(70),
        allowNull : true,
    })
    FirstName: string;

    @Length({ max: 70 })
    @Column({
        type      : DataType.STRING(70),
        allowNull : true,
    })
    MiddleName: string;

    @Length({ max: 70 })
    @Column({
        type      : DataType.STRING(70),
        allowNull : true,
    })
    LastName: string;

    @Length({ min: 1, max: 16 })
    @Column({
        type      : DataType.STRING(16),
        allowNull : true,
    })
    PhoneCode: string;

    @Index
    @Length({ max: 24 })
    @Column({
        type      : DataType.STRING(24),
        allowNull : true,
    })
    PhoneNumber: string;

    @Length({ max: 128 })
    @IsEmail
    @Column({
        type      : DataType.STRING(128),
        allowNull : true,
    })
    Email: string;

    @Column({
        type         : DataType.ENUM,
        values       : Genders,
        defaultValue : Gender.Unknown,
        allowNull    : true,
    })
    Gender: string;

    @Column({
        type      : DataType.DATE,
        allowNull : true,
    })
    DateOfBirth: Date;

    @IsUrl
    @Column({
        type      : DataType.STRING(512),
        allowNull : true,
    })
    ProfileImageUrl: string;

    @Column
    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
