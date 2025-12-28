namespace Myket.InAppBilling
{
    public enum PurchaseStatus
    {
        Purchased,
        Canceled,
        Failed,
        Consumed,
        NotSupported
    }

    public class PurchaseResult
    {
        public PurchaseStatus Status { get; set; }
        public string PurchaseToken { get; set; }
        public string ProductId { get; set; }
        public string OrderId { get; set; }
        public string OriginalJson { get; set; }
        public string Message { get; set; }
    }
}