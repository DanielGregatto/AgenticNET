namespace Identity.Model.Responses
{
    public class ForgotPasswordDto
    {
        public ForgotPasswordDto(string message, string token)
        {
            this.message = message;
            this.token = token;
        }

        public string message { get; set; }
        public string token { get; set; }
    }
}