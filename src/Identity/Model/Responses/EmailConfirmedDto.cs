namespace Identity.Model.Responses
{
    public class EmailConfirmedDto
    {
        public EmailConfirmedDto(string message, string token)
        {
            this.message = message;
            this.token = token;
        }

        public string message { get; set; }
        public string token { get; set; }

    }
}
