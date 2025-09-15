import { IClientAppRepo } from '../../../../repository.interfaces/client.apps/client.app.repo.interface';
import ClientApp from '../../models/client.apps/client.app.model';
import { Op } from 'sequelize';
import {
    ClientAppCreateModel,
    ClientAppDto,
    ApiKeyDto,
    ClientAppSearchFilters,
    ClientAppSearchResults,
} from '../../../../../domain.types/client.apps/client.app.types';
import { ClientMapper } from '../../mappers/client.apps/client.mapper';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { CurrentClient } from '../../../../../domain.types/miscellaneous/current.client';

///////////////////////////////////////////////////////////////////////

export class ClientAppRepo implements IClientAppRepo {

    create = async (model: ClientAppCreateModel): Promise<ClientAppDto> => {
        try {
            const entity = {
                ClientName   : model.ClientName,
                ClientCode   : model.ClientCode,
                IsPrivileged : model.IsPrivileged,
                PhoneCode    : model.PhoneCode,
                PhoneNumber  : model.PhoneNumber,
                Email        : model.Email,
                Password     : model.Password ?? null,
                ApiKey       : model.ApiKey ?? null,
                ValidFrom    : model.ValidFrom ?? null,
                ValidTill    : model.ValidTill ?? null,
            };
            const client = await ClientApp.create(entity);
            const dto = await ClientMapper.toDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getById = async (id: string): Promise<ClientAppDto> => {
        try {
            const client = await ClientApp.findByPk(id);
            const dto = await ClientMapper.toDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    search = async (filters: ClientAppSearchFilters): Promise<ClientAppSearchResults> => {
        try {

            const search = { where: {} };

            if (filters.ClientCode != null) {
                search.where['ClientCode'] = filters.ClientCode;
            }
            if (filters.ClientName != null) {
                search.where['ClientName'] = filters.ClientName;
            }

            const orderByColum = 'CreatedAt';
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

            const foundResults = await ClientApp.findAndCountAll(search);

            const dtos: ClientAppDto[] = [];
            for (const apiclient of foundResults.rows) {
                const dto = await ClientMapper.toDto(apiclient);
                dtos.push(dto);
            }

            const count = foundResults.count;
            const totalCount = typeof count === "number" ? count : count[0];

            const searchResults: ClientAppSearchResults = {
                TotalCount     : totalCount,
                RetrievedCount : dtos.length,
                PageIndex      : pageIndex,
                ItemsPerPage   : limit,
                Order          : order === 'DESC' ? 'descending' : 'ascending',
                OrderedBy      : orderByColum,
                Items          : dtos
            };

            return searchResults;

        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getByCode = async (clientCode: string): Promise<ClientAppDto> =>{
        try {
            const client = await ClientApp.findOne({
                where : {
                    ClientCode : clientCode
                }
            });
            const dto = await ClientMapper.toDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getClientHashedPassword = async(id: string): Promise<string> => {
        try {
            const client = await ClientApp.findByPk(id);
            return client.Password;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getApiKey = async(id: string): Promise<ApiKeyDto> => {
        try {
            const client = await ClientApp.findByPk(id);
            const dto = await ClientMapper.toClientSecretsDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    setApiKey = async(id: string, apiKey: string, validFrom: Date, validTill: Date): Promise<ApiKeyDto> => {
        try {
            const client = await ClientApp.findByPk(id);
            client.ApiKey = apiKey;
            client.ValidFrom = validFrom;
            client.ValidTill = validTill;
            await client.save();
            const dto = await ClientMapper.toClientSecretsDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    isApiKeyValid = async (apiKey: string): Promise<CurrentClient> => {
        try {

            const client = await ClientApp.findOne({
                where : {
                    ApiKey    : apiKey,
                    ValidFrom : { [Op.lte]: new Date() },
                    ValidTill : { [Op.gte]: new Date() }
                }
            });
            if (client == null){
                return null;
            }
            const currentClient: CurrentClient = {
                ClientName   : client.ClientName,
                ClientCode   : client.ClientCode,
                IsPrivileged : client.IsPrivileged
            };
            return currentClient;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    update = async (id: string, model: ClientAppCreateModel): Promise<ClientAppDto> => {
        try {
            const client = await ClientApp.findByPk(id);

            //Client code is not modifiable
            //Use renew key to update ApiKey, ValidFrom and ValidTill

            if (model.ClientName != null) {
                client.ClientName = model.ClientName;
            }
            if (model.Password != null) {
                client.Password = model.Password;
            }
            if (model.PhoneNumber != null) {
                client.PhoneNumber = model.PhoneNumber;
            }
            if (model.Email != null) {
                client.Email = model.Email;
            }
            if (model.ValidTill != null) {
                client.ValidTill = model.ValidTill;
            }
            await client.save();

            const dto = await ClientMapper.toDto(client);
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    delete = async (id: string): Promise<boolean> => {
        try {
            const result = await ClientApp.destroy({ where: { id: id } });
            return result === 1;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
