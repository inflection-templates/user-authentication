using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IOtpRepository
{
    Task<Otp?> CreateAsync(Guid userId, string otp, string purpose);
    Task<Otp?> GetByOtpAsync(Guid userId, string otp);
}
