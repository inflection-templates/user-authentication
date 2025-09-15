import { CurrentClient } from "../../../domain.types/miscellaneous/current.client";

////////////////////////////////////////////////////////////////////////////////////////////////

export interface IApiKeyVerifier {

    /**
     * Verify the validity of an API key and return client information
     * @param apiKey The API key to verify
     * @returns Promise<CurrentClient | null> - Client information if valid, null if invalid
     */
    verify(apiKey: string): Promise<CurrentClient | null>;

    /**
     * Clear any cached data for API key verification (optional)
     */
    clearCache?(): Promise<void>;

    /**
     * Get the name/type of this verifier for logging purposes
     */
    getVerifierType(): string;
}
