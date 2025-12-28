using Android.App;
using Android.Content;
using Android.OS;
using System.Threading.Tasks;
using Application = Android.App.Application;

namespace Myket.InAppBilling.Platforms.Android
{
    public class MyketBillingImplementation : IMyketBilling
    {
        private MyketServiceConnection _serviceConnection;
        private TaskCompletionSource<bool> _connectTcs;
        private TaskCompletionSource<PurchaseResult> _purchaseTcs;
        
        // کد درخواست ثابت
        private const int REQUEST_CODE = 1001;

        public async Task<bool> ConnectAsync()
        {
            if (_serviceConnection != null && _serviceConnection.IsConnected)
                return true;

            _connectTcs = new TaskCompletionSource<bool>();
            _serviceConnection = new MyketServiceConnection();
            
            _serviceConnection.Connected += (s, e) => _connectTcs.TrySetResult(true);
            
            var intent = new Intent("ir.mservices.market.InAppBillingService.BIND");
            intent.SetPackage("ir.mservices.market");

            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Application.Context;
            
            try
            {
                bool result = context.BindService(intent, _serviceConnection, Bind.AutoCreate);
                if (!result) return false;
            }
            catch
            {
                return false;
            }

            return await _connectTcs.Task;
        }

        public void Disconnect()
        {
            if (_serviceConnection != null && _serviceConnection.IsConnected)
            {
                var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Application.Context;
                context.UnbindService(_serviceConnection);
                _serviceConnection = null;
            }
        }

        public async Task<PurchaseResult> PurchaseAsync(string productId, string payload = "")
        {
            if (_serviceConnection == null || !_serviceConnection.IsConnected)
            {
                bool connected = await ConnectAsync();
                if (!connected) return new PurchaseResult { Status = PurchaseStatus.Failed, Message = "Could not connect to Myket." };
            }

            var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (context == null) return new PurchaseResult { Status = PurchaseStatus.Failed, Message = "Activity is null." };

            try
            {
                var bundle = _serviceConnection.Service.GetBuyIntent(3, context.PackageName, productId, "inapp", payload);
                int response = bundle.GetInt("RESPONSE_CODE");

                if (response == 0)
                {
                    _purchaseTcs = new TaskCompletionSource<PurchaseResult>();
                    var pendingIntent = bundle.GetParcelable("BUY_INTENT") as PendingIntent;
                    
                    // شروع اکتیویتی
                    context.StartIntentSenderForResult(pendingIntent.IntentSender, REQUEST_CODE, new Intent(), 0, 0, 0);
                    
                    return await _purchaseTcs.Task;
                }
            }
            catch (System.Exception ex)
            {
                return new PurchaseResult { Status = PurchaseStatus.Failed, Message = ex.Message };
            }

            return new PurchaseResult { Status = PurchaseStatus.Failed, Message = "Unknown error" };
        }

        public Task<bool> ConsumeAsync(string purchaseToken)
        {
             return Task.Run(() =>
             {
                 if (_serviceConnection == null || !_serviceConnection.IsConnected) return false;
                 
                 var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Application.Context;
                 int result = _serviceConnection.Service.ConsumePurchase(3, context.PackageName, purchaseToken);
                 return result == 0;
             });
        }

        public bool OnActivityResult(int requestCode, int resultCode, object dataObj)
        {
            if (requestCode != REQUEST_CODE) return false;
            
            var data = dataObj as Intent;
            if (_purchaseTcs == null || _purchaseTcs.Task.IsCompleted) return false;

            if (resultCode == (int)Result.Ok && data != null)
            {
                int responseCode = data.GetIntExtra("RESPONSE_CODE", 0);
                string purchaseData = data.GetStringExtra("INAPP_PURCHASE_DATA");
                
                if (responseCode == 0 && !string.IsNullOrEmpty(purchaseData))
                {
                    try
                    {
                        var json = System.Text.Json.JsonDocument.Parse(purchaseData);
                        var root = json.RootElement;
                        
                        var result = new PurchaseResult
                        {
                            Status = PurchaseStatus.Purchased,
                            OriginalJson = purchaseData,
                            PurchaseToken = root.GetProperty("purchaseToken").GetString(),
                            ProductId = root.GetProperty("productId").GetString(),
                            OrderId = root.TryGetProperty("orderId", out var order) ? order.GetString() : ""
                        };
                        _purchaseTcs.TrySetResult(result);
                    }
                    catch
                    {
                        _purchaseTcs.TrySetResult(new PurchaseResult { Status = PurchaseStatus.Failed, Message = "JSON Parse Error" });
                    }
                }
                else
                {
                     _purchaseTcs.TrySetResult(new PurchaseResult { Status = PurchaseStatus.Failed, Message = "Response Code not 0" });
                }
            }
            else if (resultCode == (int)Result.Canceled)
            {
                _purchaseTcs.TrySetResult(new PurchaseResult { Status = PurchaseStatus.Canceled, Message = "User Canceled" });
            }
            else
            {
                _purchaseTcs.TrySetResult(new PurchaseResult { Status = PurchaseStatus.Failed });
            }

            return true;
        }
    }
}