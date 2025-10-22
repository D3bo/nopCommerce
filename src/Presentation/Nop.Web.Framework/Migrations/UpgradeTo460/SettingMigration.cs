using FluentMigrator;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Web.Framework.Migrations.UpgradeTo460;

[NopUpdateMigration("2023-07-26 14:00:00", "4.60", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var synchronousCodeHelper = EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();

        //#3075
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.AllowCustomersToSearchWithManufacturerName))
        {
            catalogSettings.AllowCustomersToSearchWithManufacturerName = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.AllowCustomersToSearchWithManufacturerName);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.AllowCustomersToSearchWithCategoryName))
        {
            catalogSettings.AllowCustomersToSearchWithCategoryName = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.AllowCustomersToSearchWithCategoryName);
        }

        //#1933
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.DisplayAllPicturesOnCatalogPages))
        {
            catalogSettings.DisplayAllPicturesOnCatalogPages = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.DisplayAllPicturesOnCatalogPages);
        }

        //#3511
        var newProductsNumber = synchronousCodeHelper.GetSetting("catalogsettings.newproductsnumber");
        if (newProductsNumber is not null && int.TryParse(newProductsNumber.Value, out var newProductsPageSize))
        {
            catalogSettings.NewProductsPageSize = newProductsPageSize;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.NewProductsPageSize);
            synchronousCodeHelper.DeleteSetting(newProductsNumber);
        }
        else if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.NewProductsPageSize))
        {
            catalogSettings.NewProductsPageSize = 6;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.NewProductsPageSize);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.NewProductsAllowCustomersToSelectPageSize))
        {
            catalogSettings.NewProductsAllowCustomersToSelectPageSize = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.NewProductsAllowCustomersToSelectPageSize);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.NewProductsPageSizeOptions))
        {
            catalogSettings.NewProductsPageSizeOptions = "6, 3, 9";
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.NewProductsPageSizeOptions);
        }

        //#29
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.DisplayFromPrices))
        {
            catalogSettings.DisplayFromPrices = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.DisplayFromPrices);
        }

        //#6115
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ShowShortDescriptionOnCatalogPages))
        {
            catalogSettings.ShowShortDescriptionOnCatalogPages = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ShowShortDescriptionOnCatalogPages);
        }

        var storeInformationSettings = synchronousCodeHelper.LoadSetting<StoreInformationSettings>();

        //#3997
        if (!synchronousCodeHelper.SettingExists(storeInformationSettings, settings => settings.InstagramLink))
        {
            storeInformationSettings.InstagramLink = "";
            synchronousCodeHelper.SaveSetting(storeInformationSettings, settings => settings.InstagramLink);
        }

        var commonSettings = synchronousCodeHelper.LoadSetting<CommonSettings>();

        //#5802
        if (!synchronousCodeHelper.SettingExists(commonSettings, settings => settings.HeaderCustomHtml))
        {
            commonSettings.HeaderCustomHtml = "";
            synchronousCodeHelper.SaveSetting(commonSettings, settings => settings.HeaderCustomHtml);
        }

        if (!synchronousCodeHelper.SettingExists(commonSettings, settings => settings.FooterCustomHtml))
        {
            commonSettings.FooterCustomHtml = "";
            synchronousCodeHelper.SaveSetting(commonSettings, settings => settings.FooterCustomHtml);
        }

        var orderSettings = synchronousCodeHelper.LoadSetting<OrderSettings>();

        //#5604
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.ShowProductThumbnailInOrderDetailsPage))
        {
            orderSettings.ShowProductThumbnailInOrderDetailsPage = true;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.ShowProductThumbnailInOrderDetailsPage);
        }

        var mediaSettings = synchronousCodeHelper.LoadSetting<MediaSettings>();

        //#5604
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.OrderThumbPictureSize))
        {
            mediaSettings.OrderThumbPictureSize = 80;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.OrderThumbPictureSize);
        }

        var adminSettings = synchronousCodeHelper.LoadSetting<AdminAreaSettings>();
        if (!synchronousCodeHelper.SettingExists(adminSettings, settings => settings.CheckLicense))
        {
            adminSettings.CheckLicense = true;
            synchronousCodeHelper.SaveSetting(adminSettings, settings => settings.CheckLicense);
        }

        var gdprSettings = synchronousCodeHelper.LoadSetting<GdprSettings>();

        //#5809
        if (!synchronousCodeHelper.SettingExists(gdprSettings, settings => settings.DeleteInactiveCustomersAfterMonths))
        {
            gdprSettings.DeleteInactiveCustomersAfterMonths = 36;
            synchronousCodeHelper.SaveSetting(gdprSettings, settings => settings.DeleteInactiveCustomersAfterMonths);
        }

        var captchaSettings = synchronousCodeHelper.LoadSetting<CaptchaSettings>();

        //#6182
        if (!synchronousCodeHelper.SettingExists(captchaSettings, settings => settings.ShowOnCheckoutPageForGuests))
        {
            captchaSettings.ShowOnCheckoutPageForGuests = false;
            synchronousCodeHelper.SaveSetting(captchaSettings, settings => settings.ShowOnCheckoutPageForGuests);
        }

        //#7
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.VideoIframeAllow))
        {
            mediaSettings.VideoIframeAllow = "fullscreen";
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.VideoIframeAllow);
        }

        //#7
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.VideoIframeWidth))
        {
            mediaSettings.VideoIframeWidth = 300;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.VideoIframeWidth);
        }

        //#7
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.VideoIframeHeight))
        {
            mediaSettings.VideoIframeHeight = 150;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.VideoIframeHeight);
        }

        //#385
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ProductUrlStructureTypeId))
        {
            catalogSettings.ProductUrlStructureTypeId = (int)ProductUrlStructureType.Product;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ProductUrlStructureTypeId);
        }

        //#5261
        var robotsTxtSettings = synchronousCodeHelper.LoadSetting<RobotsTxtSettings>();

        if (!synchronousCodeHelper.SettingExists(robotsTxtSettings, settings => settings.DisallowPaths))
        {
            robotsTxtSettings.DisallowPaths.AddRange(new[]
            {
                "/admin",
                "/bin/",
                "/files/",
                "/files/exportimport/",
                "/country/getstatesbycountryid",
                "/install",
                "/setproductreviewhelpfulness",
                "/*?*returnUrl="
            });

            synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.DisallowPaths);
        }

        if (!synchronousCodeHelper.SettingExists(robotsTxtSettings, settings => settings.LocalizableDisallowPaths))
        {
            robotsTxtSettings.LocalizableDisallowPaths.AddRange(new[]
            {
                "/addproducttocart/catalog/",
                "/addproducttocart/details/",
                "/backinstocksubscriptions/manage",
                "/boards/forumsubscriptions",
                "/boards/forumwatch",
                "/boards/postedit",
                "/boards/postdelete",
                "/boards/postcreate",
                "/boards/topicedit",
                "/boards/topicdelete",
                "/boards/topiccreate",
                "/boards/topicmove",
                "/boards/topicwatch",
                "/cart$",
                "/changecurrency",
                "/changelanguage",
                "/changetaxtype",
                "/checkout",
                "/checkout/billingaddress",
                "/checkout/completed",
                "/checkout/confirm",
                "/checkout/shippingaddress",
                "/checkout/shippingmethod",
                "/checkout/paymentinfo",
                "/checkout/paymentmethod",
                "/clearcomparelist",
                "/compareproducts",
                "/compareproducts/add/*",
                "/customer/avatar",
                "/customer/activation",
                "/customer/addresses",
                "/customer/changepassword",
                "/customer/checkusernameavailability",
                "/customer/downloadableproducts",
                "/customer/info",
                "/customer/productreviews",
                "/deletepm",
                "/emailwishlist",
                "/eucookielawaccept",
                "/inboxupdate",
                "/newsletter/subscriptionactivation",
                "/onepagecheckout",
                "/order/history",
                "/orderdetails",
                "/passwordrecovery/confirm",
                "/poll/vote",
                "/privatemessages",
                "/recentlyviewedproducts",
                "/returnrequest",
                "/returnrequest/history",
                "/rewardpoints/history",
                "/search?",
                "/sendpm",
                "/sentupdate",
                "/shoppingcart/*",
                "/storeclosed",
                "/subscribenewsletter",
                "/topic/authenticate",
                "/viewpm",
                "/uploadfilecheckoutattribute",
                "/uploadfileproductattribute",
                "/uploadfilereturnrequest",
                "/wishlist"
            });

            synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.LocalizableDisallowPaths);
        }

        if (!synchronousCodeHelper.SettingExists(robotsTxtSettings, settings => settings.DisallowLanguages))
            synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.DisallowLanguages);

        if (!synchronousCodeHelper.SettingExists(robotsTxtSettings, settings => settings.AdditionsRules))
            synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.AdditionsRules);

        if (!synchronousCodeHelper.SettingExists(robotsTxtSettings, settings => settings.AllowSitemapXml))
            synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.AllowSitemapXml);

        //#5753
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.ProductDefaultImageId))
        {
            mediaSettings.ProductDefaultImageId = 0;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.ProductDefaultImageId);
        }

        //#3651
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.AttachPdfInvoiceToOrderProcessingEmail))
        {
            orderSettings.AttachPdfInvoiceToOrderProcessingEmail = false;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.AttachPdfInvoiceToOrderProcessingEmail);
        }

        var taxSettings = synchronousCodeHelper.LoadSetting<TaxSettings>();

        //#1961
        if (!synchronousCodeHelper.SettingExists(taxSettings, settings => settings.EuVatEnabledForGuests))
        {
            taxSettings.EuVatEnabledForGuests = false;
            synchronousCodeHelper.SaveSetting(taxSettings, settings => settings.EuVatEnabledForGuests);
        }

        //#5570
        var sitemapXmlSettings = synchronousCodeHelper.LoadSetting<SitemapXmlSettings>();

        if (!synchronousCodeHelper.SettingExists(sitemapXmlSettings, settings => settings.RebuildSitemapXmlAfterHours))
        {
            sitemapXmlSettings.RebuildSitemapXmlAfterHours = 2 * 24;
            synchronousCodeHelper.SaveSetting(sitemapXmlSettings, settings => settings.RebuildSitemapXmlAfterHours);
        }

        if (!synchronousCodeHelper.SettingExists(sitemapXmlSettings, settings => settings.SitemapBuildOperationDelay))
        {
            sitemapXmlSettings.SitemapBuildOperationDelay = 60;
            synchronousCodeHelper.SaveSetting(sitemapXmlSettings, settings => settings.SitemapBuildOperationDelay);
        }

        //#6378
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.AllowSvgUploads))
        {
            mediaSettings.AllowSvgUploads = false;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.AllowSvgUploads);
        }

        //#5599
        var messagesSettings = synchronousCodeHelper.LoadSetting<MessagesSettings>();

        if (!synchronousCodeHelper.SettingExists(messagesSettings, settings => settings.UseDefaultEmailAccountForSendStoreOwnerEmails))
        {
            messagesSettings.UseDefaultEmailAccountForSendStoreOwnerEmails = false;
            synchronousCodeHelper.SaveSetting(messagesSettings, settings => settings.UseDefaultEmailAccountForSendStoreOwnerEmails);
        }

        //#228
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ActiveSearchProviderSystemName))
        {
            catalogSettings.ActiveSearchProviderSystemName = string.Empty;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ActiveSearchProviderSystemName);
        }

        //#43
        var metaTitleKey = $"{nameof(SeoSettings)}.DefaultTitle".ToLower();
        var metaKeywordsKey = $"{nameof(SeoSettings)}.DefaultMetaKeywords".ToLower();
        var metaDescriptionKey = $"{nameof(SeoSettings)}.DefaultMetaDescription".ToLower();
        var homepageTitleKey = $"{nameof(SeoSettings)}.HomepageTitle".ToLower();
        var homepageDescriptionKey = $"{nameof(SeoSettings)}.HomepageDescription".ToLower();

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();

        foreach (var store in synchronousCodeHelper.GetAllStores())
        {
            var metaTitle = synchronousCodeHelper.GetSettingByKey<string>(metaTitleKey, storeId: store.Id) ?? synchronousCodeHelper.GetSettingByKey<string>(metaTitleKey);
            var metaKeywords = synchronousCodeHelper.GetSettingByKey<string>(metaKeywordsKey, storeId: store.Id) ?? synchronousCodeHelper.GetSettingByKey<string>(metaKeywordsKey);
            var metaDescription = synchronousCodeHelper.GetSettingByKey<string>(metaDescriptionKey, storeId: store.Id) ?? synchronousCodeHelper.GetSettingByKey<string>(metaDescriptionKey);
            var homepageTitle = synchronousCodeHelper.GetSettingByKey<string>(homepageTitleKey, storeId: store.Id) ?? synchronousCodeHelper.GetSettingByKey<string>(homepageTitleKey);
            var homepageDescription = synchronousCodeHelper.GetSettingByKey<string>(homepageDescriptionKey, storeId: store.Id) ?? synchronousCodeHelper.GetSettingByKey<string>(homepageDescriptionKey);

            if (metaTitle != null)
                store.DefaultTitle = metaTitle;

            if (metaKeywords != null)
                store.DefaultMetaKeywords = metaKeywords;

            if (metaDescription != null)
                store.DefaultMetaDescription = metaDescription;

            if (homepageTitle != null)
                store.HomepageTitle = homepageTitle;

            if (homepageDescription != null)
                store.HomepageDescription = homepageDescription;

            synchronousCodeHelper.UpdateStore(store);
        }

        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == metaTitleKey);
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == metaKeywordsKey);
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == metaDescriptionKey);
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == homepageTitleKey);
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == homepageDescriptionKey);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}