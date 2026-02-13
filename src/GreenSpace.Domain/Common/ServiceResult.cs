using GreenSpace.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Common
{
    public class ServiceResult : IServiceResult
    {
        public bool IsSuccess { get; protected set; }
        public string? Message { get; protected set; }
        public IEnumerable<string>? Errors { get; protected set; }
        public int StatusCode { get; protected set; }

        protected ServiceResult(bool isSuccess, int statusCode, string? message = null, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
        public static ServiceResult Success(string? message = null, int statusCode = 200)
              => new(true, statusCode, message);

        public static ServiceResult Failure(int statusCode, string message)
            => new(false, statusCode, message);

        public static ServiceResult Failure(int statusCode, IEnumerable<string> errors)
            => new(false, statusCode, errors: errors);
    }

    /// <summary>
    /// Result pattern implementation for service operations with data
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class ServiceResult<T> : ServiceResult, IServiceResult<T>
    {
        public T? Data { get; private set; }

        protected ServiceResult(bool isSuccess, int statusCode, T? data = default, string? message = null, IEnumerable<string>? errors = null)
            : base(isSuccess, statusCode, message, errors)
        {
            Data = data;
        }

        public static ServiceResult<T> Success(T data, string? message = null, int statusCode = 200)
            => new(true, statusCode, data, message);

        public static new ServiceResult<T> Failure(int statusCode, string message)
            => new(false, statusCode, default, message);

        public static new ServiceResult<T> Failure(IEnumerable<string> errors)
            => new(false, default, errors: errors);

        /// <summary>
        /// Return data even on failure (for debug info)
        /// </summary>
        public static ServiceResult<T> SuccessWithData(T data, int statusCode, string? message = null)
            => new(statusCode >= 200 && statusCode < 300, statusCode, data, message);
    } 
}
