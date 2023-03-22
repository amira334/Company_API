using Company_API.Models.DTO;
using Company_API.Models;
using Company_API.Data;

namespace Company_API.Services
{
    public class UserService
    {
        private readonly DatabaseContext _dbContext;

        public UserService(DatabaseContext dbContext)
        {
            _dbContext = dbContext; //
        }

        public User Authenticate(LoginDTO loginDTO)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Username == loginDTO.Username);

            // Check if user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                return null;
            }

            // Authentication successful
            return user;
        }

        public async Task<User> SaveRefreshToken(int userId, string refreshToken)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            user.RefreshToken = refreshToken;
            await _dbContext.SaveChangesAsync();

            return user;
        }

        // To renew accesstoken
        public User GetUserByRefreshToken(string refreshToken)
        {
            var user = _dbContext.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                return null;
            }
            return user;
        }

        public UserRole GetUserRole(int userRoleId)
        {
            var userRole = _dbContext.UserRoles.SingleOrDefault(u => u.Id == userRoleId);

            if (userRole == null)
            {
                return null;
            }
            return userRole;
        }

        public async Task<User> Register(RegisterDTO registerDTO)
        {
            // Check if username already exists
            if (_dbContext.Users.Any(u => u.Username == registerDTO.Username))
            {
                throw new ApplicationException("Username already exists");
            }

            // Create new user object
            var user = new User
            {
                Username = registerDTO.Username,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                UserRoleId = registerDTO.UserRoleId
            };

            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

            // Save user to database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }
    }
}
