import { ApiError } from '../../../../../common/api.error';
import { logger } from '../../../../../logger/logger';
import {
    UserDeviceDetailsDomainModel,
    UserDeviceDetailsDto,
    UserDeviceDetailsSearchFilters,
    UserDeviceDetailsSearchResults
} from "../../../../../domain.types/users/user.device.details.types";
import { IUserDeviceDetailsRepo } from '../../../../repository.interfaces/users/user.device.details.repo.interface';
import { UserDeviceDetailsMapper } from '../../../sequelize/mappers/users/user.device.details.mapper';
import UserDeviceDetails from '../../../sequelize/models/users/user.device.details.model';

///////////////////////////////////////////////////////////////////////

export class UserDeviceDetailsRepo implements IUserDeviceDetailsRepo {

    create = async (model: UserDeviceDetailsDomainModel):
    Promise<UserDeviceDetailsDto> => {
        try {
            const entity = {
                UserId      : model.UserId,
                Token       : model.Token,
                DeviceName  : model.DeviceName,
                OSType      : model.OSType,
                OSVersion   : model.OSVersion,
                AppName     : model.AppName,
                AppVersion  : model.AppVersion,
                ChangeCount : model.ChangeCount
            };
            const record = await UserDeviceDetails.create(entity);
            const dto = UserDeviceDetailsMapper.toDto(record);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: string): Promise<UserDeviceDetailsDto> => {
        try {
            const record = await UserDeviceDetails.findByPk(id);
            const dto = UserDeviceDetailsMapper.toDto(record);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getByUserId = async (userId: string): Promise<UserDeviceDetailsDto[]> => {
        try {
            const list = await UserDeviceDetails.findAll({
                where : {
                    UserId : userId
                }
            });
            return list.map(x => UserDeviceDetailsMapper.toDto(x));
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getExistingRecord = async (deviceDetails: any): Promise<UserDeviceDetailsDto> => {
        try {
            const existingRecord =  await UserDeviceDetails.findOne({
                where : {
                    UserId  : deviceDetails.UserId,
                    AppName : deviceDetails.AppName,
                    Token   : deviceDetails.Token,
                }
            });
            return await UserDeviceDetailsMapper.toDto(existingRecord);

        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: UserDeviceDetailsSearchFilters): Promise<UserDeviceDetailsSearchResults> => {
        try {
            const search = { where: {} };

            if (filters.UserId != null) {
                search.where['UserId'] = filters.UserId;
            }
            if (filters.Token != null) {
                search.where['Token'] = filters.Token;
            }
            if (filters.DeviceName != null) {
                search.where['DeviceName'] = filters.DeviceName;
            }
            if (filters.OSType != null) {
                search.where['OSType'] = filters.OSType;
            }
            if (filters.OSVersion != null) {
                search.where['OSVersion'] = filters.OSVersion;
            }
            if (filters.AppName != null) {
                search.where['AppName'] = filters.AppName;
            }
            if (filters.AppVersion != null) {
                search.where['AppVersion'] = filters.AppVersion;
            }
            let orderByColum = 'CreatedAt';
            if (filters.OrderBy) {
                orderByColum = filters.OrderBy;
            }
            let order = 'ASC';
            if (filters.Order === 'descending') {
                order = 'DESC';
            }
            search['order'] = [[orderByColum, order]];

            let limit = 25;
            if (filters.ItemsPerPage) {
                limit = filters.ItemsPerPage;
            }
            let offset = 0;
            let pageIndex = 0;
            if (filters.PageIndex) {
                pageIndex = filters.PageIndex < 0 ? 0 : filters.PageIndex;
                offset = pageIndex * limit;
            }
            search['limit'] = limit;
            search['offset'] = offset;

            const foundResults = await UserDeviceDetails.findAndCountAll(search);

            const dtos: UserDeviceDetailsDto[] = [];
            for (const userDeviceDetails of foundResults.rows) {
                const dto = UserDeviceDetailsMapper.toDto(userDeviceDetails);
                dtos.push(dto);
            }

            const searchResults: UserDeviceDetailsSearchResults = {
                TotalCount     : foundResults.count,
                RetrievedCount : dtos.length,
                PageIndex      : pageIndex,
                ItemsPerPage   : limit,
                Order          : order === 'DESC' ? 'descending' : 'ascending',
                OrderedBy      : orderByColum,
                Items          : dtos,
            };

            return searchResults;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    update = async (id: string, model: UserDeviceDetailsDomainModel):
    Promise<UserDeviceDetailsDto> => {
        try {
            const record = await UserDeviceDetails.findByPk(id);

            if (model.UserId != null) {
                record.UserId = model.UserId;
            }
            if (model.Token != null) {
                record.Token = model.Token;
            }
            if (model.DeviceName != null) {
                record.DeviceName = model.DeviceName;
            }
            if (model.OSType != null) {
                record.OSType = model.OSType;
            }
            if (model.OSVersion != null) {
                record.OSVersion = model.OSVersion;
            }
            if (model.AppName != null) {
                record.AppName = model.AppName;
            }
            if (model.AppVersion != null) {
                record.AppVersion = model.AppVersion;
            }
            if (model.ChangeCount != null) {
                record.ChangeCount = model.ChangeCount;
            }

            await record.save();

            const dto = UserDeviceDetailsMapper.toDto(record);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (id: string): Promise<boolean> => {
        try {
            logger.info(id);

            const result = await UserDeviceDetails.destroy({ where: { id: id } });
            return result === 1;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
