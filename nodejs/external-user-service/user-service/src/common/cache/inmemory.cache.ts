import { CacheMap } from "./cache.map";
import { type ICache } from "./cache.interface";

////////////////////////////////////////////////////////////////////////////////////////

export class InMemoryCache implements ICache {

    private cache: CacheMap<any> = new CacheMap<any>();

    constructor() {
        this.cache = new CacheMap<any>();
    }

    set = async (key: string, value: any): Promise<void> => {
        this.cache.set(key, value);
    };

    get = async (key: string): Promise<any | undefined> => {
        return this.cache.get(key);
    };

    has = async (key: string): Promise<boolean> => {
        return this.cache.has(key);
    };

    delete = async (key: string): Promise<boolean> => {
        return this.cache.delete(key);
    };

    clear = async (): Promise<void> => {
        this.cache.clear();
    };

    findAndClear = async (searchPattern: string): Promise<string[]> => {
        return this.cache.findAndClear(searchPattern);
    };

}
