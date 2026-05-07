namespace Domain.Contracts.API
{
    public class ErrorResponseItemDto
    {
        public ErrorResponseItemDto(string type, string errorDesc)
        {
            this.type = type;
            this.errorDesc = errorDesc;
        }

        public string type { get; set; }
        public string errorDesc { get; set; }
    }
}