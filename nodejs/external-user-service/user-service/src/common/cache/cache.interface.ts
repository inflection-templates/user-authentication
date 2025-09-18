
export interface ICache {
    set(key: string, value: unknown): Promise<void>;
    get(key: string): Promise<unknown | undefined>;
    has(key: string): Promise<boolean>;
    delete(key: string): Promise<boolean>;
    clear(): Promise<void>;
    findAndClear(searchPattern: string): Promise<string[]>;
}
