using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static GreenSpace.Application.Common.Constants.ApiMessages;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            public const string SentFailed = "OTP sent failed";
        }


        public static class Error
        {
            public const string General = "Internal server error: ";
            public const string NotImplemented = "This feature is not implemented yet.";
            public const string ServiceUnavailable = "Service is currently unavailable. Please try again later.";
            public const string Timeout = "The request has timed out. Please try again.";
            public const string SystemBusy = "";
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
            public const string PhoneExisted = "Phone already exists";

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


        public static class Category
        {
            public const string NotFound = "Category not found.";
            public const string Created = "Category created successfully.";
            public const string Updated = "Category updated successfully.";
            public const string Deleted = "Category deleted successfully.";
        }


        public static class Order
        {
            public const string NotFound = "Order not found.";
            public const string Created = "Order created successfully.";
            public const string Updated = "Order updated successfully.";
            public const string Deleted = "Order deleted successfully.";
            public const string StatusUpdated = "Order status updated successfully.";

            public const string CreateFailed = "Order creation failed. Please try again";
            public const string UpdateFailed = "Failed to update order status.";
        }

        public static class Stock
        {
            public const string HighTraffic = "Unable to reserve stock due to high demand. Please try again.";
            public const string MutipleAttempts = "Failed to reserve stock after multiple attempts";
            public const string NoStock = "No stock to confirm";
            public const string Confirmed = "Stock confirmed successfully";
            public const string Released = "Stock released successfully";

        }



        public static class ProductVariant
        {
            public const string NotFound = "Product variant not found.";
            public const string InsufficientStock = "Insufficient stock for the requested quantity.";
            public const string Created = "Product variant created successfully.";
            public const string Updated = "Product variant updated successfully.";
            public const string Deleted = "Product variant deleted successfully.";
            public const string SkuExists = "SKU already exists.";
            public const string StockUpdated = "Product variant stock updated successfully.";
            public const string Added = "Variant added successfully.";
            public const string AddFailed = "Failed to add item to cart.";
        }

        public static class Rating
        {
            public const string NotFound = "Rating not found.";
            public const string Created = "Rating created successfully.";
            public const string Updated = "Rating updated successfully.";
            public const string Deleted = "Rating deleted successfully.";
            public const string StatusUpdated = "Rating status updated successfully.";
            public const string Existed = "You have already rated this product.";
            public const string NoPermit = "You can only update your own ratings.";
            public const string NoData = "No ratings found for this product.";
        }

        public static class Address
        {
            public const string NotFound = "Address not found.";
            public const string Created = "Address created successfully.";
            public const string Updated = "Address updated successfully.";
            public const string Deleted = "Address deleted successfully.";
            public const string NoPermit = "You can only manage your own addresses.";
        }

        public static class Payment
        {
            public static string Created = "Payment created successfully";
            public static string LinkCreated = "Payment link created successfully";
            public static string InvalidSignature = "Invalid signature";
            public static string InvalidTransaction = "Invalid transaction reference";
            public static string Failed = "Payment failed";
            public static string Success = "Payment successful";
            public static string NotFound = "Payment not found";
            public static string StatusUpdated = "Payment status updated successfully";
            public static string Paied = "Payment already processed";
        }

        public static class Cart
        {
            public const string NotFound = "Cart variant not found.";
            public const string ItemNotFound = "Itemnot found in cart.";
            public const string Created = "Cart variant created successfully.";
            public const string Updated = "Cart variant updated successfully.";
            public const string Deleted = "Cart variant deleted successfully.";
            public const string UpdateFailed = "Cart variant updated failed.";
            public const string Cleared = "Cart cleared successfully.";
            public const string ClearFailed = "Failed to clear cart.";

        }

        public static class Promotion
        {
            public const string NotFound = "Promotiont not found.";
            public const string Invalid = "Invalid promotion code.";
            public const string Expired = "Promotion code expired.";
            public const string InActived = "Inactived promotion code.";
            public const string LimitExceeded = "Promotion usage limit reached.";
            public const string Created = "Promotiont created successfully.";
            public const string Updated = "Promotiont updated successfully.";
            public const string Deleted = "Promotiont deleted successfully.";
            public const string UpdateFailed = "Promotiont updated failed.";

        }

        public static class Diagnostic
        {
            public const string ServiceUnavailable = "AI Diagnostic service unavailable.";
            public const string DiagnosicError = "An error occurred during the diagnostic process.";
            public const string InvalidApiKey = "Invalid API key for AI Diagnostic service.";
            public const string DiagnosicFailed = "Unable to process diagnosic results.";
            public const string Timeout = "AI Diagnostic request timed out.";
            public const string InvalidInput = "Invalid input for AI Diagnostic.";
            public const string QuotaExceeded = "AI Diagnostic service quota exceeded.";

            public const string DiagnosicImageFailed = "Unable to diagnosic the image.";
            public const string DiagnosicTextFailed = "Unable to diagnosic the text.";

            public const string Success = "Diagnostic successfully.";
        }

    }
}
