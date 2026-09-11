namespace MediCart.Web.Services
{
    // Single source of truth for sensitivity-tier flagging thresholds.
    // Used both at order placement time (OrderService) and at display
    // time (AdminOrdersController), so the two can never drift apart.
    //
    // High  >= 5 units
    // Mid   >= 15 units
    // Low   >= 30 units
    public static class SensitivityFlagHelper
    {
        public const int HighThreshold = 5;
        public const int MidThreshold = 15;
        public const int LowThreshold = 30;

        public static bool IsOverThreshold(string? sensitivityLevel, int quantity)
        {
            return sensitivityLevel?.ToLower() switch
            {
                "high" => quantity >= HighThreshold,
                "mid" => quantity >= MidThreshold,
                "low" => quantity >= LowThreshold,
                _ => false
            };
        }

        public static int? GetThreshold(string? sensitivityLevel)
        {
            return sensitivityLevel?.ToLower() switch
            {
                "high" => HighThreshold,
                "mid" => MidThreshold,
                "low" => LowThreshold,
                _ => null
            };
        }
    }
}