using Domain.Enums;

namespace Domain.Contracts.Common
{
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string PropertyName { get; set; }
        public ErrorTypes Type { get; set; }

        public Error(string message, ErrorTypes type = ErrorTypes.Unknown, string code = null, string propertyName = null)
        {
            Message = message;
            Type = type;
            Code = code ?? type.ToString();
            PropertyName = propertyName;
        }

        // Factory methods for common error types
        public static Error Validation(string message, string propertyName = null) =>
            new Error(message, ErrorTypes.Validation, "Validation", propertyName);

        public static Error NotFound(string message, string entityName = null) =>
            new Error(message, ErrorTypes.NotFound, "NotFound", entityName);

        public static Error Conflict(string message) =>
            new Error(message, ErrorTypes.Conflict, "Conflict");

        public static Error Unauthorized(string message = "Unauthorized") =>
            new Error(message, ErrorTypes.Unauthorized, "Unauthorized");

        public static Error Forbidden(string message = "Forbidden") =>
            new Error(message, ErrorTypes.Forbidden, "Forbidden");

        public static Error Database(string message) =>
            new Error(message, ErrorTypes.Database, "Database");

        public static Error Failure(string message, string code = null) =>
            new Error(message, ErrorTypes.Unknown, code);
    }
}
