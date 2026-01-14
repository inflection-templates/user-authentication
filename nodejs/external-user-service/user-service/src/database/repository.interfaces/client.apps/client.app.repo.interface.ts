import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { ClientAppSearchFilters,
    ClientAppSearchResults,
    ClientAppCreateModel,
    ClientAppDto,
    ApiKeyDto,
} from '../../../domain.types/client.apps/client.app.types';
import { CurrentClient } from '../../../domain.types/miscellaneous/current.client';

////////////////////////////////////////////////////////////////////////////////////////////////

export interface IClientAppRepo {

    create(clientDomainModel: ClientAppCreateModel): Promise<ClientAppDto>;

    getById(id: uuid): Promise<ClientAppDto>;

    getByCode(clientCode: string): Promise<ClientAppDto>;

    getClientHashedPassword(id: uuid): Promise<string>;

    getApiKey(id: uuid): Promise<ApiKeyDto>;

    setApiKey(id: uuid, apiKey: string, validFrom: Date, validTill: Date): Promise<ApiKeyDto>;

    isApiKeyValid(apiKey: string): Promise<CurrentClient>;

    update(id: uuid, clientDomainModel: ClientAppCreateModel): Promise<ClientAppDto>;

    search(filters: ClientAppSearchFilters): Promise<ClientAppSearchResults>;

    delete(id: uuid): Promise<boolean>;

}
