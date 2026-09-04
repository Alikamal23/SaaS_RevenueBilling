using RevenueBillingApp.Models.Authentication;

namespace RevenueBillingApp.Models.Session
{
    public class SessionItems
    {
        public string UniqueKey { get; set; }
        public string? IpAddress { get; set; }
        public UserInfo userInfo { get; set; }
        public List<RolesMapping> rolesMapping { get; set; }
        public Tokens authToken { get; set; }
        public List<Form> forms { get; set; }
    }
}
