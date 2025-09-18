using MassTransit;

namespace shala.api.eventmessaging;

public record UserSignedUp(
    string UserId,
    string? Email,
    string? CountryCode,
    string? PhoneNumber);
