namespace Domain.Enums
{
    public enum TransactionStatus
    {
        Success, // Payment was successful
        Pending, // Payment is being processed
        Failed, // Payment failed
        Refunded, // Money was returned
        Chargeback // Disputed payment
    }
}
