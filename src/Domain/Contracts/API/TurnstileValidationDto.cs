using System.Collections.Generic;

namespace Domain.Contracts.API
{
    public class TurnstileValidationDto
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
        public List<string> errorCodes { get; set; }
    }
}
