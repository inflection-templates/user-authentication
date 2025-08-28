using AspNetJwtAuth.Models;

namespace AspNetJwtAuth.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> VerifyPasswordAsync(User user, string password);
        Task<User> UpdatePasswordAsync(User user, string newPassword);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> AssignRoleToUserAsync(int userId, int roleId);
        Task<User> RemoveRoleFromUserAsync(int userId, int roleId);
    }
}
