using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using GreenSpace.Application.Interfaces.Security;

namespace GreenSpace.Infrastructure.ExternalServices
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordHasher<object> _passHasher;

        public PasswordService()
        {
            _passHasher = new PasswordHasher<object>();
        }

        /// <summary>
        /// Hash password using ASP.NET Core Identity PasswordHasher
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        public string HashPassword(string password)
        {
            return _passHasher.HashPassword(new object(), password);
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">Hashed password</param>
        /// <returns>True if password matches</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            var result = _passHasher.VerifyHashedPassword(new object(), hashedPassword, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
