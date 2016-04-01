using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using UIKit;

namespace CodeHub.iOS.Services
{
    public class FeaturesService : IFeaturesService
    {
        private readonly IDefaultValueService _defaultValueService;
        private readonly IInAppPurchaseService _inAppPurchaseService;
   
        public const string ProEdition = "com.dillonbuchanan.codehub.pro";

        public FeaturesService(IDefaultValueService defaultValueService, IInAppPurchaseService inAppPurchaseService)
        {
            _defaultValueService = defaultValueService;
            _inAppPurchaseService = inAppPurchaseService;
        }

        public bool IsProEnabled
        {
            get
            {
                return IsActivated(ProEdition);
            }
        }

        public async Task ActivatePro()
        {
            var productData = (await _inAppPurchaseService.RequestProductData(ProEdition)).Products.FirstOrDefault();
            if (productData == null)
                throw new InvalidOperationException("Unable to activate CodeHub Pro. iTunes returns no products to purchase!");
            await _inAppPurchaseService.PurchaseProduct(productData);
            _defaultValueService.Set(ProEdition, true);
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            appDelegate?.RegisterUserForNotifications();
        }

        public void ActivateProDirect()
        {
            _defaultValueService.Set(ProEdition, true);
        }

        public Task RestorePro()
        {
            return _inAppPurchaseService.Restore();
        }

        private bool IsActivated(string id)
        {
            bool value;
            return _defaultValueService.TryGet(id, out value) && value;
        }
    }
}

