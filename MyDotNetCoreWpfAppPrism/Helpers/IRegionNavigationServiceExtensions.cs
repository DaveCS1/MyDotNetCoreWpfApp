﻿using Prism.Regions;

namespace MyDotNetCoreWpfAppPrism.Helpers
{
    public static class IRegionNavigationServiceExtensions
    {
        public static bool CanNavigate(this IRegionNavigationService navigationService, string target)
        {
            if (navigationService.Journal.CurrentEntry == null)
            {
                return true;
            }

            return target != navigationService.Journal.CurrentEntry.Uri.ToString();
        }
    }
}
