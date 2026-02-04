
namespace GreenSpace.Domain.Constants
{
    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Staff = "STAFF";
        public const string Manager = "MANAGER";
        public const string Customer = "CUSTOMER";

        /// <summary>
        /// Chuẩn hóa role đầu vào:
        /// - Trim khoảng trắng
        /// - Chuyển về chữ hoa
        /// - Kiểm tra có nằm trong danh sách cho phép không
        /// </summary>
        /// <param name="inputRole">Role do FE gửi lên</param>
        /// <returns>Role chuẩn (ADMIN) hoặc null nếu không hợp lệ</returns>
        public static string? Normalize(string? inputRole)
        {
            if (string.IsNullOrWhiteSpace(inputRole))
                return null;
 
            var cleanRole = inputRole.Trim().ToUpper();
 
            if (All.Contains(cleanRole))
            {
                return cleanRole;
            }

            return null; 
        }

        /// <summary>
        /// Danh sách tất cả vai trò
        /// </summary>
        public static IEnumerable<string> All => new[] { Admin, Staff, Manager, Customer };
        
    }
}
