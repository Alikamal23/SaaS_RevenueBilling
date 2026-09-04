namespace RevenueBillingApp.Models.Authentication
{
    public class UserPolicies
    {
        public int ID { set; get; }
        public bool AlphaType { set; get; }
        public bool NumericType { set; get; }
        public int MinAlphaChar { set; get; }
        public int MinNumericChar { set; get; }
        public bool CharCaps { set; get; }
        public int MinCharCaps { set; get; }
        public bool IsSpecialChar { set; get; }
        public int MinSpecialChar { set; get; }
        public int MaxLoginAttempt { set; get; }
        public int IntervalBadeLoginMinuts { set; get; }
        public int PasswordExpiryDays { set; get; }
        public bool ResetPasswordOnFirstLogin { set; get; }
        public bool BlockUserAfterMaxAttempt { set; get; }
        public bool AllowConcurrentLogin { set; get; }
        public int AvoidPasswordHistory { set; get; }
        public double? idletimeout { set; get; }
        public int? inactiveindays { set; get; }
    }
}
