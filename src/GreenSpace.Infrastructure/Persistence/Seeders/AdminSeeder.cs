using GreenSpace.Application.Interfaces.Security;
using GreenSpace.Domain.Constants;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Options;
using GreenSpace.Application.Common.Settings;

namespace GreenSpace.Infrastructure.Persistence.Seeders
{
    public class AdminSeeder
    {
        private readonly AppDbContext _context;
        private readonly DefaultAdmin _options;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AdminSeeder> _logger;

        public AdminSeeder(
            IOptions<DefaultAdmin> options,
            AppDbContext context,
            IPasswordService passwordService,
            ILogger<AdminSeeder> logger)
        {
            _options = options.Value;
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Kiểm tra xem đã có admin nào chưa
                var adminExists = await _context.Users
                    .AnyAsync(u => u.Role == Roles.Admin);

                if (adminExists)
                {
                    _logger.LogInformation("Admin user already exists. Skipping seed.");
                    return;
                }

                // Tạo admin mặc định
                var adminUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = _options.Username,
                    Email = _options.Email,
                    PasswordHash = _passwordService.HashPassword(_options.Password),
                    FirstName = "System",
                    LastName = "Admin",
                    Phone = _options.Phone,
                    Role = Roles.Admin,
                    IsActive = true,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    LoginAttempts = 0
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default admin user created successfully. Email: {Email}", adminUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default admin user");
            }
        }
    }
}