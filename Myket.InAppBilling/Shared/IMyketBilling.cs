using System.Threading.Tasks;

namespace Myket.InAppBilling
{
    public interface IMyketBilling
    {
        /// <summary>
        /// اتصال به سرویس مایکت
        /// </summary>
        Task<bool> ConnectAsync();

        /// <summary>
        /// قطع اتصال
        /// </summary>
        void Disconnect();

        /// <summary>
        /// شروع فرایند خرید
        /// </summary>
        Task<PurchaseResult> PurchaseAsync(string productId, string payload = "");

        /// <summary>
        /// مصرف کردن محصول (برای محصولات مصرفی مثل سکه)
        /// </summary>
        Task<bool> ConsumeAsync(string purchaseToken);
        
        /// <summary>
        /// این متد باید در OnActivityResult اکتیویتی صدا زده شود
        /// </summary>
        bool OnActivityResult(int requestCode, int resultCode, object data);
    }
}