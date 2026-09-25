namespace MediCart.Web.Services
{
    // Single source of truth for payment status string constants used
    // across order placement and admin order close-out logic. Mirrors
    // PaymentMethods — never hard-code these strings elsewhere.
    public static class PaymentStatuses
    {
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Refunded = "refunded";
    }
}