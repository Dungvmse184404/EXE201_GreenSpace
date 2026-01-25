using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Interfaces.Security
{
    public interface IPasswordService
    {
        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verify hashed password  
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hashedPassword">Hashed password</param>
        /// <returns>True if password matches</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
