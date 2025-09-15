import { InMemoryCache } from './inmemory.cache';
import type { ICache } from './cache.interface';

////////////////////////////////////////////////////////////////////////////////////////

const getCache = () => {
    return new InMemoryCache();
    // return new RedisCache();
};

////////////////////////////////////////////////////////////////////////////////////////

export class CacheService {

    static _cache: ICache = getCache();

    static get = async (key: string): Promise<any | undefined> => {
        return CacheService._cache.get(key);
    };

    static set = async (key: string, value: any): Promise<void> => {
        await CacheService._cache.set(key, value);
    };

    static has = async (key: string): Promise<boolean> => {
        return CacheService._cache.has(key);
    };

    static delete = async (key: string): Promise<boolean> => {
        return CacheService._cache.delete(key);
    };

    static deleteMany = async (keys: string[]): Promise<boolean> => {
        let result = true;
        for (const key of keys) {
            result = result && await CacheService._cache.delete(key);
        }
        return result;
    };

    static findAndClear = async (searchPatterns: string[]): Promise<string[]> => {
        var keys: string[] = [];
        for (var substr of searchPatterns)
        {
            var removedKeys = await CacheService._cache.findAndClear(substr);
            keys.push(...removedKeys);
        }
        return keys;
    };

    static clear = async (): Promise<void> => {
        await CacheService._cache.clear();
    };

}
