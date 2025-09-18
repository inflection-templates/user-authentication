import {
    Column,
    CreatedAt,
    DataType,
    DeletedAt,
    IsUUID,
    Model,
    PrimaryKey,
    Table,
    UpdatedAt
} from 'sequelize-typescript';

///////////////////////////////////////////////////////////////////////

@Table({
    timestamps      : true,
    modelName       : 'UserMetadata',
    tableName       : 'user_metadata',
    paranoid        : true,
    freezeTableName : true,
})
export default class UserMetadata extends Model {

    @IsUUID(4)
    @PrimaryKey
    @Column({
        type      : DataType.UUID,
        allowNull : false,
    })
    UserId: string;

    @Column({
        type      : DataType.STRING(128),
        allowNull : true,
    })
    DisplayName: string;

    @Column({
        type      : DataType.STRING(16),
        allowNull : true,
    })
    DefaultTimeZone: string;

    @Column({
        type      : DataType.STRING(16),
        allowNull : true,
    })
    CurrentTimeZone: string;

    @Column({
        type      : DataType.STRING(16),
        allowNull : true,
    })
    PreferredLanguage: string;

    @Column({
        type         : DataType.BOOLEAN,
        allowNull    : false,
        defaultValue : false
    })
    IsTestUser: boolean;

    @Column({
        type      : DataType.DATE,
        allowNull : true,
    })
    LastLogin: Date;

    @Column({
        type      : DataType.TEXT,
        allowNull : true,
    })
    UserSettings: string;

    @CreatedAt
    CreatedAt: Date;

    @UpdatedAt
    UpdatedAt: Date;

    @DeletedAt
    DeletedAt: Date;

}
