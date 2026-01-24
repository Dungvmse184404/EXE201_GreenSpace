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

        protected ServiceResult(bool isSuccess, string? message = null, IEnumerable<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors;
        }

        public static ServiceResult Success(string? message = null)
            => new(true, message);

        public static ServiceResult Failure(string message)
            => new(false, message);

        public static ServiceResult Failure(IEnumerable<string> errors)
            => new(false, errors: errors);

        public static ServiceResult Failure(string message, IEnumerable<string> errors)
            => new(false, message, errors);
    }

    /// <summary>
    /// Result pattern implementation for service operations with data
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class ServiceResult<T> : ServiceResult, IServiceResult<T>
    {
        public T? Data { get; private set; }

        protected ServiceResult(bool isSuccess, T? data = default, string? message = null, IEnumerable<string>? errors = null)
            : base(isSuccess, message, errors)
        {
            Data = data;
        }

        public static ServiceResult<T> Success(T data, string? message = null)
            => new(true, data, message);

        public static new ServiceResult<T> Failure(string message)
            => new(false, default, message);

        public static new ServiceResult<T> Failure(IEnumerable<string> errors)
            => new(false, default, errors: errors);

        public static new ServiceResult<T> Failure(string message, IEnumerable<string> errors)
            => new(false, default, message, errors);
    } 
}
