using Microsoft.AspNetCore.Identity;

namespace Identity.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CPF_CNPJ { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Complement { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? ExternalCustomerGatewayID { get; set; }
        public string? ExternalCustomerCardGatewayID { get; set; }
        public string? PreferenceName { get; set; }
        public string? PreferenceProfession { get; set; }
        public string? PreferencePosition { get; set; }
        public string? PreferenceBehaviors { get; set; }
        public string? PreferenceExtraInformation { get; set; }
    }
}
