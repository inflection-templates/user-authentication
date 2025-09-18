using shala.api.domain.types;

namespace shala.api.services;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber);
    Task<User?> CreateAsync(UserCreateModel model);
    Task<User?> UpdateAsync(Guid id, UserUpdateModel model);
    Task<bool> DeleteAsync(Guid id);
    Task<SearchResults<User>> SearchAsync(UserSearchFilters filters);
}
