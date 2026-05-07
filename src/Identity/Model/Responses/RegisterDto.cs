namespace Identity.Model.Responses
{
    public class RegisterDto
    {
        public RegisterDto(string id)
        {
            this.id = id;
        }

        public string id { get; set; }
    }
}