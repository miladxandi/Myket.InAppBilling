# Myket In-App Billing for .NET MAUI

کتابخانه غیررسمی و ساده برای پیاده‌سازی پرداخت درون‌برنامه‌ای **مایکت (Myket)** در پروژه‌های .NET MAUI.
این کتابخانه مشکل عدم وجود فایل‌های AIDL و پیاده‌سازی‌های پیچیده بایندرها را حل کرده و یک رابط کاربری ساده `async/await` در اختیار شما قرار می‌دهد.

## ویژگی‌ها

- ✅ اتصال آسان به سرویس پرداخت مایکت
- ✅ خرید محصول (Purchase)
- ✅ مصرف محصول (Consume) برای محصولات مصرفی (سکه، جم و...)
- ✅ بدون نیاز به فایل‌های Java یا AIDL خارجی
- ✅ کاملاً منطبق با .NET 8 و MAUI

## نصب

پکیج را از طریق NuGet به پروژه خود اضافه کنید:

```bash
dotnet add package Myket.InAppBilling
```

## راه‌اندازی (Setup)

### ۱. تنظیمات AndroidManifest.xml

در فایل `Platforms/Android/AndroidManifest.xml` مجوزهای زیر را اضافه کنید:

```xml
<uses-permission android:name="ir.mservices.market.BILLING" />
<uses-permission android:name="com.android.vending.BILLING" />

<!-- اضافه کردن کوئری برای دیده شدن اپلیکیشن مایکت در اندروید 11 به بالا -->
<queries>
    <package android:name="ir.mservices.market" />
</queries>
```

### ۲. تنظیمات MainActivity.cs

برای اینکه نتیجه خرید از اکتیویتی به کتابخانه برگردد، باید متد `OnActivityResult` را در `MainActivity.cs` خود بازنویسی (Override) کنید:

```csharp
using Android.App;
using Android.Content;
using Myket.InAppBilling; // فضای نام کتابخانه

namespace YourAppName
{
    [Activity(Label = "YourApp", MainLauncher = true, ...)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            // هدایت نتیجه به کتابخانه مایکت
            CrossMyketBilling.Current.OnActivityResult(requestCode, (int)resultCode, data);
        }
        
        protected override void OnDestroy()
        {
            // قطع اتصال سرویس برای جلوگیری از نشت حافظه
            CrossMyketBilling.Current.Disconnect();
            base.OnDestroy();
        }
    }
}
```

## نحوه استفاده (Usage)

### خرید محصول

```csharp
using Myket.InAppBilling;

private async void OnBuyClicked(object sender, EventArgs e)
{
    // شناسه محصول تعریف شده در پنل توسعه‌دهندگان مایکت
    string productId = "coin_pack_100";
    
    // درخواست خرید
    var result = await CrossMyketBilling.Current.PurchaseAsync(productId);

    if (result.Status == PurchaseStatus.Purchased)
    {
        // خرید موفقیت آمیز بود
        // result.PurchaseToken و result.OrderId در دسترس هستند
        
        // اگر محصول مصرفی است (مثل سکه)، حتما متد زیر را صدا بزنید:
        bool consumed = await CrossMyketBilling.Current.ConsumeAsync(result.PurchaseToken);
        
        if (consumed)
        {
            // سکه را به کاربر اضافه کنید
            await DisplayAlert("تبریک", "خرید انجام شد و سکه اضافه شد", "باشه");
        }
    }
    else if (result.Status == PurchaseStatus.Canceled)
    {
        await DisplayAlert("توجه", "خرید توسط کاربر لغو شد", "باشه");
    }
    else
    {
        await DisplayAlert("خطا", $"خطا در خرید: {result.Message}", "باشه");
    }
}
```

### متدهای موجود

| متد | توضیحات |
| --- | --- |
| `ConnectAsync()` | اتصال دستی به سرویس مایکت (معمولا خودکار انجام می‌شود اما برای چک کردن وضعیت مفید است). |
| `PurchaseAsync(id, payload)` | شروع پروسه خرید. `payload` اختیاری است. |
| `ConsumeAsync(token)` | مصرف کردن خرید. برای محصولاتی که باید دوباره قابل خرید باشند الزامی است. |
| `Disconnect()` | قطع ارتباط با سرویس مایکت. |

## نکات مهم

1. **تست روی دستگاه:** پرداخت درون‌برنامه‌ای روی شبیه‌ساز (Emulator) کار نمی‌کند. حتماً روی گوشی واقعی که اپلیکیشن **مایکت** روی آن نصب است و کاربر در آن لاگین کرده، تست کنید.
2. **شناسه محصول:** شناسه محصول (Product ID) باید دقیقاً با پنل مایکت مطابقت داشته باشد.
3. **نسخه اندروید:** این کتابخانه با Android 5.0 (API 21) به بالا سازگار است.

## لایسنس
MIT