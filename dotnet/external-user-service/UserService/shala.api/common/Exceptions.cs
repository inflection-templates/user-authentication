namespace shala.api.common;

public class InvalidApiKeyException : Exception
{
    public InvalidApiKeyException()
    : base("Unauthorized client: API Key is invalid.")
    { }

    public int StatusCode { get; set; } = StatusCodes.Status401Unauthorized;

}

public class ExpiredApiKeyException : Exception
{
    public ExpiredApiKeyException()
    : base("Unauthorized client: API Key has expired.")
    { }

    public int StatusCode { get; set; } = StatusCodes.Status401Unauthorized;

}

public class MissingApiKeyException : Exception
{
    public MissingApiKeyException()
    : base("Unauthorized client: API Key is missing.")
    { }

    public int StatusCode { get; set; } = StatusCodes.Status401Unauthorized;

}

public class UnauthorizedUserException : Exception
{
    public UnauthorizedUserException()
    : base("Unauthorized user: User does not have enough permissions.")
    { }

    public int StatusCode { get; set; } = StatusCodes.Status401Unauthorized;

}
