namespace MediCart.Web.Services
{
    // Single source of truth for payment method string constants used
    // across checkout, order placement, and admin order close-out logic.
    // Mirrors the pattern established by SensitivityFlagHelper and
    // StockExpiryHelper — never hard-code these strings elsewhere.
    public static class PaymentMethods
    {
        public const string CashOnDelivery = "Cash on delivery";
        public const string Bkash = "bKash";
        public const string Card = "Card";
    }
}