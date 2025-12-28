using Android.OS;

namespace Myket.InAppBilling.Platforms.Android;

public class InAppBillingServiceStub : Binder, IInterface
    {
        // توکن استاندارد گوگل که مایکت هم از آن استفاده می‌کند
        const string DESCRIPTOR = "com.android.vending.billing.IInAppBillingService";

        const int TRANSACTION_isBillingSupported = 1;
        const int TRANSACTION_getSkuDetails = 2;
        const int TRANSACTION_getBuyIntent = 3;
        const int TRANSACTION_getPurchases = 4;
        const int TRANSACTION_consumePurchase = 5;

        public InAppBillingServiceStub()
        {
            this.AttachInterface(this, DESCRIPTOR);
        }

        public static InAppBillingServiceStub AsInterface(IBinder obj)
        {
            if (obj == null) return null;
            var iin = obj.QueryLocalInterface(DESCRIPTOR);
            if (iin != null && iin is InAppBillingServiceStub stub)
                return stub;
            return new InAppBillingServiceStub(obj);
        }

        // پروکسی داخلی برای ارسال اطلاعات به سرویس مایکت
        private IBinder _remote;
        public InAppBillingServiceStub(IBinder remote)
        {
            _remote = remote;
        }

        public IBinder AsBinder() => _remote;

        // متد دریافت Intent خرید
        public Bundle GetBuyIntent(int apiVersion, string packageName, string sku, string type, string developerPayload)
        {
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            Bundle result = null;
            try
            {
                data.WriteInterfaceToken(DESCRIPTOR);
                data.WriteInt(apiVersion);
                data.WriteString(packageName);
                data.WriteString(sku);
                data.WriteString(type);
                data.WriteString(developerPayload);
                
                // ارسال درخواست به مایکت
                _remote.Transact(TRANSACTION_getBuyIntent, data, reply, 0);
                
                reply.ReadException();
                if (reply.ReadInt() != 0)
                {
                    result = (Bundle)Bundle.Creator.CreateFromParcel(reply);
                }
            }
            finally
            {
                reply.Recycle();
                data.Recycle();
            }
            return result;
        }

        // متد مصرف کردن خرید (Consume)
        public int ConsumePurchase(int apiVersion, string packageName, string purchaseToken)
        {
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            int result = 0;
            try
            {
                data.WriteInterfaceToken(DESCRIPTOR);
                data.WriteInt(apiVersion);
                data.WriteString(packageName);
                data.WriteString(purchaseToken);
                
                _remote.Transact(TRANSACTION_consumePurchase, data, reply, 0);
                
                reply.ReadException();
                result = reply.ReadInt();
            }
            finally
            {
                reply.Recycle();
                data.Recycle();
            }
            return result;
        }
    }