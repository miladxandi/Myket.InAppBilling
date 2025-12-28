using Android.Content;
using Android.OS;

namespace Myket.InAppBilling.Platforms.Android;

public class MyketServiceConnection : Java.Lang.Object, IServiceConnection
{
    public InAppBillingServiceStub Service { get; private set; }
    public bool IsConnected { get; private set; }
        
    // رویداد برای خبر دادن به اکتیویتی که اتصال برقرار شد
    public event EventHandler Connected;

    public void OnServiceConnected(ComponentName name, IBinder service)
    {
        Service = InAppBillingServiceStub.AsInterface(service);
        IsConnected = true;
        Connected?.Invoke(this, EventArgs.Empty); // خبر بده!
    }

    public void OnServiceDisconnected(ComponentName name)
    {
        Service = null;
        IsConnected = false;
    }
}