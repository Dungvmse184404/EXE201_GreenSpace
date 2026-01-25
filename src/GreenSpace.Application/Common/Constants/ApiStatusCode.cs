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
        /// <summary>
        /// The ok
        /// </summary>
        public const int OK = 200;
        /// <summary>
        /// The created
        /// </summary>
        public const int Created = 201;
        /// <summary>
        /// The accepted
        /// </summary>
        public const int Accepted = 202;
        /// <summary>
        /// The no content
        /// </summary>
        public const int NoContent = 204;

        /// <summary>
        /// The bad request
        /// </summary>
        public const int BadRequest = 400;
        /// <summary>
        /// The unauthorized
        /// </summary>
        public const int Unauthorized = 401;
        /// <summary>
        /// The forbidden
        /// </summary>
        public const int Forbidden = 403;
        /// <summary>
        /// The not found
        /// </summary>
        public const int NotFound = 404;
        /// <summary>
        /// The method not allowed
        /// </summary>
        public const int MethodNotAllowed = 405;
        /// <summary>
        /// The conflict
        /// </summary>
        public const int Conflict = 409;
        /// <summary>
        /// The gone
        /// </summary>
        public const int Gone = 410;
        /// <summary>
        /// The unprocessable entity
        /// </summary>
        public const int UnprocessableEntity = 422;
        /// <summary>
        /// The locked
        /// </summary>
        public const int Locked = 423;
        /// <summary>
        /// The too many requests
        /// </summary>
        public const int TooManyRequests = 429;

        /// <summary>
        /// The internal server error
        /// </summary>
        public const int InternalServerError = 500;
        /// <summary>
        /// The not implemented
        /// </summary>
        public const int NotImplemented = 501;
        /// <summary>
        /// The bad gateway
        /// </summary>
        public const int BadGateway = 502;
        /// <summary>
        /// The service unavailable
        /// </summary>
        public const int ServiceUnavailable = 503;
    }
}