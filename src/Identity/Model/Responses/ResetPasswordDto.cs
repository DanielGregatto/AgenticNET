namespace Identity.Model.Responses
{
    public class ResetPasswordDto
    {
        public ResetPasswordDto(string message)
        {
            this.message = message;
        }

        public string message { get; set; }

    }
}
