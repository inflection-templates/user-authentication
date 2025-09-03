using shala.api.database.interfaces.models;
using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IUserRepository
  : ICRUDRepository<
    User,
    UserCreateModel,
    UserUpdateModel,
    UserSearchFilters,
    IDbModel>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber);
    Task<User?> GetByUserNameAsync(string userName);

}
