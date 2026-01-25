using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Common.Constants
{
    public class ApiMessages
    {
        public static class Auth
        {
            /// <summary>
            /// The login success
            /// </summary>
            public const string LoginSuccess = "Login successful";
            /// <summary>
            /// The login failed
            /// </summary>
            public const string LoginFailed = "Invalid email or password";
            /// <summary>
            /// The account locked
            /// </summary>
            public const string AccountLocked = "Account is locked. Try again in {0} minutes.";
            /// <summary>
            /// The account locked multiple attempts
            /// </summary>
            public const string AccountLockedMultipleAttempts = "Account locked due to multiple failed login attempts. Try again after {0} minutes.";
            /// <summary>
            /// The account inactive
            /// </summary>
            public const string AccountInactive = "Account is inactive. Please contact support.";
            /// <summary>
            /// The account not verified
            /// </summary>
            public const string AccountNotVerified = "Account is not verified. Please verify your email.";
            /// <summary>
            /// The unauthorized
            /// </summary>
            public const string Unauthorized = "Unauthorized access";
            /// <summary>
            /// The invalid credentials
            /// </summary>
            public const string InvalidCredentials = "Invalid credentials";
            /// <summary>
            /// The password changed success
            /// </summary>
            public const string PasswordChangedSuccess = "Password changed successfully";
            /// <summary>
            /// The current password incorrect
            /// </summary>
            public const string CurrentPasswordIncorrect = "Current password is incorrect";
            /// <summary>
            /// The password must be different
            /// </summary>
            public const string PasswordMustBeDifferent = "New password must be different from current password";
            /// <summary>
            /// The access denied
            /// </summary>
            public const string AccessDenied = "You do not have permission to view this user.";
            /// <summary>
            /// The existed email
            /// </summary>
            public const string ExistedEmail = "email already exists.";
            /// <summary>
            /// The existed phone
            /// </summary>
            public const string ExistedPhone = "phone number already exists.";
            /// <summary>
            /// The register success
            /// </summary>
            public const string RegisterSuccess = "Registration successful. Please check your email to verify your account.";

        }


        public static class User
        {
            public const string NotFound = "User not found.";

            public const string Created = "User created successfully.";
            public const string CreateFailed = "User creation failed.";

            public const string Updated = "User updated successfully.";
            public const string UpdateFailed = "User update failed.";

            public const string Deleted = "User deleted successfully.";
            public const string DeleteFailed = "User deletion failed.";

            public const string HardDeleted = "User hard delete successfully.";
            public const string HardDeleteFailed = "User hard delete failed.";

        }

        public static class OTP
        {
            public const string Sent = "OTP has been sent to your email.";
            public const string Verified = "OTP verified successfully.";
            public const string Invalid = "Invalid OTP. Please try again.";
            public const string Expired = "OTP has expired. Please request a new one.";

        }


        public static class Error
        {
            public const string General = "An error occurred. Please try again later.";
        }

        public static class Product
        {
            public const string NotFound = "Product not found.";
            public const string Created = "Product created successfully.";
            public const string Updated = "Product updated successfully.";
            public const string Deleted = "Product deleted successfully.";

        }


        public static class Mail
        {
            public const string SendSuccess = "Mail sent successfully.";
            public const string SendFailure = "Failed to send mail.";
            public const string InvalidRecipient = "Invalid email recipient.";
            public const string Existed = "Email already exists.";

        }


        public static class Token
        {
            public const string Refreshed = "Token refreshed successfully.";
            public const string Revoked = "Token revoked successfully.";
            public const string InvalidToken = "Invalid token.";
            public const string ExpiredToken = "Token has expired.";
        }


        public static class role
        {
            public const string NotFound = "Role not found.";
        }







    }
}
