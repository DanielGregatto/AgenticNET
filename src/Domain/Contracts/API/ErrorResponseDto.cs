using System.Collections.Generic;

namespace Domain.Contracts.API
{
    public class ErrorResponseDto
    {
        public ErrorResponseDto(
            string type,
            string title,
            int status,
            string traceId,
            List<ErrorResponseItemDto> items)
        {
            this.type = type;
            this.title = title;
            this.status = status;
            this.traceId = traceId;

            if (items != null)
            {
                var errorsList = new Dictionary<string, List<string>>();
                foreach (var error in items)
                {
                    if (errorsList.ContainsKey(error.type))
                        errorsList[error.type].Add(error.errorDesc);
                    else
                        errorsList[error.type] = new List<string> { error.errorDesc };
                }
                errors = errorsList;
            }
        }

        public string type { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public string traceId { get; set; }
        public IDictionary<string, List<string>> errors { get; set; }
    }
}