
namespace Myket.InAppBilling
{
    public static class CrossMyketBilling
    {
        static IMyketBilling implementation;

        public static IMyketBilling Current
        {
            get
            {
                if (implementation == null)
                {
#if ANDROID
                    implementation = new Platforms.Android.MyketBillingImplementation();
#else
                    throw new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
#endif
                }
                return implementation;
            }
        }
    }
}