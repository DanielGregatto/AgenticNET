namespace Identity.Model.Responses
{
    public class LoginDto
    {
        public LoginDto(string access_token, string refresh_token)
        {
            this.access_token = access_token;
            this.refresh_token = refresh_token;
        }

        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}