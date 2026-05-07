namespace Identity.Model
{
    public class JWTConfig
    {
        public string Issuer { get; set; }
        public string Secret { get; set; }
        public string Audience { get; set; }
        public int MinutesValid { get; set; }
        public string RedirectUriExternalLogin { get; set; }
        public string RedirectUriEmailConfirm { get; set; }
        public string RedirectUriResetPassword { get; set; }

    }
}
