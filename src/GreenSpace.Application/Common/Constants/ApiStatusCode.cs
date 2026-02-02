using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.Common.Constants
{
    /// <summary>
    /// This class contains constant API status codes used throughout the application.
    /// </summary>
    public static class ApiStatusCodes
    {
        /// <summary> Yêu cầu thành công và trả về dữ liệu. </summary>
        public const int OK = 200;

        /// <summary> Tạo mới tài nguyên thành công (vạch ra Resource ID mới). </summary>
        public const int Created = 201;

        /// <summary> Đã nhận yêu cầu nhưng đang chờ xử lý background. </summary>
        public const int Accepted = 202;

        /// <summary> Xử lý thành công nhưng không có dữ liệu trả về (thường dùng cho Delete/Update). </summary>
        public const int NoContent = 204;

        /// <summary> Lỗi cú pháp hoặc dữ liệu đầu vào không hợp lệ (Validation fail). </summary>
        public const int BadRequest = 400;

        /// <summary> Chưa đăng nhập hoặc Token hết hạn/không hợp lệ. </summary>
        public const int Unauthorized = 401;

        /// <summary> Đã đăng nhập nhưng không có quyền truy cập vào tài nguyên này. </summary>
        public const int Forbidden = 403;

        /// <summary> Không tìm thấy tài nguyên (ID không tồn tại trong DB). </summary>
        public const int NotFound = 404;

        /// <summary> HTTP Method (GET/POST/PUT...) không được hỗ trợ cho endpoint này. </summary>
        public const int MethodNotAllowed = 405;

        /// <summary> Xung đột dữ liệu (ví dụ: Trùng Email, Số điện thoại đã tồn tại). </summary>
        public const int Conflict = 409;

        /// <summary> Tài nguyên đã từng tồn tại nhưng hiện đã bị xóa vĩnh viễn. </summary>
        public const int Gone = 410;

        /// <summary> Dữ liệu đúng format nhưng vi phạm logic nghiệp vụ (thường dùng trong ASP.NET Core). </summary>
        public const int UnprocessableEntity = 422;

        /// <summary> Tài nguyên đang bị khóa (đang có process khác xử lý). </summary>
        public const int Locked = 423;

        /// <summary> Client gửi quá nhiều yêu cầu trong một khoảng thời gian (Spam/Rate limit). </summary>
        public const int TooManyRequests = 429;

        /// <summary> Lỗi hệ thống không xác định phía Server. </summary>
        public const int InternalServerError = 500;

        /// <summary> Server chưa hỗ trợ tính năng này. </summary>
        public const int NotImplemented = 501;

        /// <summary> Server trung gian nhận phản hồi lỗi từ server đích (Lỗi Proxy/Gateway). </summary>
        public const int BadGateway = 502;

        /// <summary> Server bị quá tải hoặc đang bảo trì. </summary>
        public const int ServiceUnavailable = 503;
        //thêm mã lỗi vào đây
    }
}