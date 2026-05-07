using Domain.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Contracts.Common
{
    /// <summary>
    /// Represents the result of an operation that returns data
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Data { get; }
        public List<Error> Errors { get; }

        protected Result(bool isSuccess, T data, List<Error> errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Errors = errors ?? new List<Error>();
        }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        public static Result<T> Success(T data) =>
            new Result<T>(true, data, null);

        /// <summary>
        /// Creates a failed result with errors
        /// </summary>
        public static Result<T> Failure(params Error[] errors) =>
            new Result<T>(false, default, errors?.ToList() ?? new List<Error>());

        /// <summary>
        /// Creates a failed result with a single error message
        /// </summary>
        public static Result<T> Failure(string message, ErrorTypes type = ErrorTypes.Unknown) =>
            new Result<T>(false, default, new List<Error> { new Error(message, type) });

        /// <summary>
        /// Creates a failed result from validation errors
        /// </summary>
        public static Result<T> ValidationFailure(params Error[] errors) =>
            new Result<T>(false, default, errors?.ToList() ?? new List<Error>());

        /// <summary>
        /// Creates a not found result
        /// </summary>
        public static Result<T> NotFound(string message = "Resource not found") =>
            new Result<T>(false, default, new List<Error> { Error.NotFound(message) });

        /// <summary>
        /// Creates an unauthorized result
        /// </summary>
        public static Result<T> Unauthorized(string message = "Unauthorized") =>
            new Result<T>(false, default, new List<Error> { Error.Unauthorized(message) });
    }

    /// <summary>
    /// Represents the result of an operation without return data
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public List<Error> Errors { get; }

        protected Result(bool isSuccess, List<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? new List<Error>();
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static Result Success() =>
            new Result(true, null);

        /// <summary>
        /// Creates a failed result with errors
        /// </summary>
        public static Result Failure(params Error[] errors) =>
            new Result(false, errors?.ToList() ?? new List<Error>());

        /// <summary>
        /// Creates a failed result with a single error message
        /// </summary>
        public static Result Failure(string message, ErrorTypes type = ErrorTypes.Unknown) =>
            new Result(false, new List<Error> { new Error(message, type) });

        /// <summary>
        /// Creates a failed result from validation errors
        /// </summary>
        public static Result ValidationFailure(params Error[] errors) =>
            new Result(false, errors?.ToList() ?? new List<Error>());

        /// <summary>
        /// Creates a not found result
        /// </summary>
        public static Result NotFound(string message = "Resource not found") =>
            new Result(false, new List<Error> { Error.NotFound(message) });

        /// <summary>
        /// Creates an unauthorized result
        /// </summary>
        public static Result Unauthorized(string message = "Unauthorized") =>
            new Result(false, new List<Error> { Error.Unauthorized(message) });
    }
}
