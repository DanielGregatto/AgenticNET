namespace Domain.Configs
{
    public class EmailConfig
    {
        public string Host { get; set; }
        public int SmtpPort { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool UseSSL { get; set; }
        public string MainUrl { get; set; }
        public string MainUrlHelp { get; set; }
        public string MainUrlPayment { get; set; }
    }
}
