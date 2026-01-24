using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Domain.Interfaces
{
    /// <summary>
    /// Interface for service result pattern
    /// </summary>
    public interface IServiceResult
    {
        bool IsSuccess { get; }
        string? Message { get; }
        IEnumerable<string>? Errors { get; }
    }

    /// <summary>
    /// Interface for service result pattern with data
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public interface IServiceResult<T> : IServiceResult
    {
        T? Data { get; }
    }

}