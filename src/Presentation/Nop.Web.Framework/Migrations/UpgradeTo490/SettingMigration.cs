using FluentMigrator;
using Nop.Core.Domain.ArtificialIntelligence;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Menus;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Translation;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.ArtificialIntelligence;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Media;

namespace Nop.Web.Framework.Migrations.UpgradeTo490;

[NopUpdateMigration("2025-02-26 00:00:00", "4.90", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var synchronousCodeHelper = EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        //#6590
        var adminAreaSettings = synchronousCodeHelper.LoadSetting<AdminAreaSettings>();
        if (!synchronousCodeHelper.SettingExists(adminAreaSettings, settings => settings.UseStickyHeaderLayout))
        {
            adminAreaSettings.UseStickyHeaderLayout = false;
            synchronousCodeHelper.SaveSetting(adminAreaSettings, settings => settings.UseStickyHeaderLayout);
        }

        //#7387
        var productEditorSettings = synchronousCodeHelper.LoadSetting<ProductEditorSettings>();
        if (!synchronousCodeHelper.SettingExists(productEditorSettings, settings => settings.AgeVerification))
        {
            productEditorSettings.AgeVerification = false;
            synchronousCodeHelper.SaveSetting(productEditorSettings, settings => settings.AgeVerification);
        }

        //#2184
        var vendorSettings = synchronousCodeHelper.LoadSetting<VendorSettings>();
        if (!synchronousCodeHelper.SettingExists(vendorSettings, settings => settings.MaximumProductPicturesNumber))
        {
            vendorSettings.MaximumProductPicturesNumber = 5;
            synchronousCodeHelper.SaveSetting(vendorSettings, settings => settings.MaximumProductPicturesNumber);
        }

        //#7571
        var captchaSettings = synchronousCodeHelper.LoadSetting<CaptchaSettings>();
        if (!synchronousCodeHelper.SettingExists(captchaSettings, settings => settings.ShowOnCheckGiftCardBalance))
        {
            captchaSettings.ShowOnCheckGiftCardBalance = true;
            synchronousCodeHelper.SaveSetting(captchaSettings, settings => settings.ShowOnCheckGiftCardBalance);
        }

        //#5818
        var mediaSettings = synchronousCodeHelper.LoadSetting<MediaSettings>();
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.AutoOrientImage))
        {
            mediaSettings.AutoOrientImage = false;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.AutoOrientImage);
        }

        //#1892
        if (!synchronousCodeHelper.SettingExists(adminAreaSettings, settings => settings.MinimumDropdownItemsForSearch))
        {
            adminAreaSettings.MinimumDropdownItemsForSearch = 50;
            synchronousCodeHelper.SaveSetting(adminAreaSettings, settings => settings.MinimumDropdownItemsForSearch);
        }

        //#7405
        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ExportImportCategoryUseLimitedToStores))
        {
            catalogSettings.ExportImportCategoryUseLimitedToStores = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ExportImportCategoryUseLimitedToStores);
        }

        //#7477
        var pdfSettings = synchronousCodeHelper.LoadSetting<PdfSettings>();
        var pdfSettingsFontFamily = synchronousCodeHelper.GetSetting("pdfsettings.fontfamily");
        if (pdfSettingsFontFamily is not null)
            synchronousCodeHelper.DeleteSetting(pdfSettingsFontFamily);

        if (!synchronousCodeHelper.SettingExists(pdfSettings, settings => settings.RtlFontName))
        {
            pdfSettings.RtlFontName = NopCommonDefaults.PdfRtlFontName;
            synchronousCodeHelper.SaveSetting(pdfSettings, settings => pdfSettings.RtlFontName);
        }

        if (!synchronousCodeHelper.SettingExists(pdfSettings, settings => settings.LtrFontName))
        {
            pdfSettings.LtrFontName = NopCommonDefaults.PdfLtrFontName;
            synchronousCodeHelper.SaveSetting(pdfSettings, settings => pdfSettings.LtrFontName);
        }

        if (!synchronousCodeHelper.SettingExists(pdfSettings, settings => settings.BaseFontSize))
        {
            pdfSettings.BaseFontSize = 10f;
            synchronousCodeHelper.SaveSetting(pdfSettings, settings => pdfSettings.BaseFontSize);
        }

        if (!synchronousCodeHelper.SettingExists(pdfSettings, settings => settings.ImageTargetSize))
        {
            pdfSettings.ImageTargetSize = 200;
            synchronousCodeHelper.SaveSetting(pdfSettings, settings => pdfSettings.ImageTargetSize);
        }

        //#7397
        var richEditorAllowJavaScript = synchronousCodeHelper.GetSetting("adminareasettings.richeditorallowjavascript");
        if (richEditorAllowJavaScript is not null)
            synchronousCodeHelper.DeleteSetting(richEditorAllowJavaScript);

        var richEditorAllowStyleTag = synchronousCodeHelper.GetSetting("adminareasettings.richeditorallowstyletag");
        if (richEditorAllowStyleTag is not null)
            synchronousCodeHelper.DeleteSetting(richEditorAllowStyleTag);

        if (synchronousCodeHelper.SettingExists(adminAreaSettings, settings => settings.RichEditorAdditionalSettings))
        {
            adminAreaSettings.RichEditorAdditionalSettings = string.Empty;
            synchronousCodeHelper.SaveSetting(adminAreaSettings, settings => settings.RichEditorAdditionalSettings);
        }

        //#6874
        var newsletterTickedByDefault = synchronousCodeHelper.GetSetting("customersettings.newslettertickedbydefault");
        if (newsletterTickedByDefault is not null)
            synchronousCodeHelper.DeleteSetting(newsletterTickedByDefault);

        //#820
        var currencySettings = synchronousCodeHelper.LoadSetting<CurrencySettings>();
        if (!synchronousCodeHelper.SettingExists(currencySettings, settings => settings.DisplayCurrencySymbolInCurrencySelector))
        {
            currencySettings.DisplayCurrencySymbolInCurrencySelector = false;
            synchronousCodeHelper.SaveSetting(currencySettings, settings => settings.DisplayCurrencySymbolInCurrencySelector);
        }

        //#1779
        var customerSettings = synchronousCodeHelper.LoadSetting<CustomerSettings>();
        if (!synchronousCodeHelper.SettingExists(customerSettings, settings => settings.NotifyFailedLoginAttempt))
        {
            customerSettings.NotifyFailedLoginAttempt = false;
            synchronousCodeHelper.SaveSetting(customerSettings, settings => settings.NotifyFailedLoginAttempt);
        }

        //#7630
        var taxSettings = synchronousCodeHelper.LoadSetting<TaxSettings>();

        if (!synchronousCodeHelper.SettingExists(taxSettings, settings => settings.HmrcApiUrl))
        {
            taxSettings.HmrcApiUrl = "https://api.service.hmrc.gov.uk";
            synchronousCodeHelper.SaveSetting(taxSettings, settings => taxSettings.HmrcApiUrl);
        }

        if (!synchronousCodeHelper.SettingExists(taxSettings, settings => settings.HmrcClientId))
        {
            taxSettings.HmrcClientId = string.Empty;
            synchronousCodeHelper.SaveSetting(taxSettings, settings => taxSettings.HmrcClientId);
        }

        if (!synchronousCodeHelper.SettingExists(taxSettings, settings => settings.HmrcClientSecret))
        {
            taxSettings.HmrcClientSecret = string.Empty;
            synchronousCodeHelper.SaveSetting(taxSettings, settings => taxSettings.HmrcClientSecret);
        }

        //#1266
        var orderSettings = synchronousCodeHelper.LoadSetting<OrderSettings>();
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.CustomerOrdersPageSize))
        {
            orderSettings.CustomerOrdersPageSize = 10;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.CustomerOrdersPageSize);
        }

        //#7625
        var addressSetting = synchronousCodeHelper.LoadSetting<AddressSettings>();
        if (!synchronousCodeHelper.SettingExists(addressSetting, settings => settings.PrePopulateCountryByCustomer))
        {
            addressSetting.PrePopulateCountryByCustomer = true;
            synchronousCodeHelper.SaveSetting(addressSetting, settings => settings.PrePopulateCountryByCustomer);
        }

        //#7747
        var forumSettings = synchronousCodeHelper.LoadSetting<ForumSettings>();
        if (!synchronousCodeHelper.SettingExists(forumSettings, settings => settings.TopicMetaDescriptionLength))
        {
            forumSettings.TopicMetaDescriptionLength = 160;
            synchronousCodeHelper.SaveSetting(forumSettings, settings => settings.TopicMetaDescriptionLength);
        }

        //#7388
        var translationSettings = synchronousCodeHelper.LoadSetting<TranslationSettings>();
        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.AllowPreTranslate))
        {
            translationSettings.AllowPreTranslate = false;
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.AllowPreTranslate);
        }

        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.TranslateFromLanguageId))
        {
            var languageRepository = EngineContext.Current.Resolve<IRepository<Language>>();

            translationSettings.TranslateFromLanguageId = languageRepository.Table.First().Id;
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.TranslateFromLanguageId);
        }

        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.GoogleApiKey))
        {
            translationSettings.GoogleApiKey = string.Empty;
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.GoogleApiKey);
        }

        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.DeepLAuthKey))
        {
            translationSettings.DeepLAuthKey = string.Empty;
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.DeepLAuthKey);
        }

        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.NotTranslateLanguages))
        {
            translationSettings.NotTranslateLanguages = new List<int>();
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.NotTranslateLanguages);
        }

        if (!synchronousCodeHelper.SettingExists(translationSettings, settings => settings.TranslationServiceId))
        {
            translationSettings.TranslationServiceId = 0;
            synchronousCodeHelper.SaveSetting(translationSettings, settings => settings.TranslationServiceId);
        }

        //#7779
        var robotsTxtSettings = synchronousCodeHelper.LoadSetting<RobotsTxtSettings>();
        var newDisallowPaths = new List<string> { "/*?*returnurl=", "/*?*ReturnUrl=" };

        foreach (var newDisallowPath in newDisallowPaths.Where(newDisallowPath => !robotsTxtSettings.DisallowPaths.Contains(newDisallowPath)))
            robotsTxtSettings.DisallowPaths.Add(newDisallowPath);

        robotsTxtSettings.DisallowPaths.Sort();
        synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.DisallowPaths);

        //#1921
        var shoppingCartSettings = synchronousCodeHelper.LoadSetting<ShoppingCartSettings>();
        if (!synchronousCodeHelper.SettingExists(shoppingCartSettings, settings => settings.AllowMultipleWishlist))
        {
            shoppingCartSettings.AllowMultipleWishlist = true;
            synchronousCodeHelper.SaveSetting(shoppingCartSettings, settings => settings.AllowMultipleWishlist);
        }
        if (!synchronousCodeHelper.SettingExists(shoppingCartSettings, settings => settings.MaximumNumberOfCustomWishlist))
        {
            shoppingCartSettings.MaximumNumberOfCustomWishlist = 10;
            synchronousCodeHelper.SaveSetting(shoppingCartSettings, settings => settings.MaximumNumberOfCustomWishlist);
        }

        //#7730
        var aiSettings = synchronousCodeHelper.LoadSetting<ArtificialIntelligenceSettings>();

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.Enabled))
        {
            aiSettings.Enabled = false;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.Enabled);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.ChatGptApiKey))
        {
            aiSettings.ChatGptApiKey = string.Empty;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.ChatGptApiKey);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.DeepSeekApiKey))
        {
            aiSettings.DeepSeekApiKey = string.Empty;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.DeepSeekApiKey);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.GeminiApiKey))
        {
            aiSettings.GeminiApiKey = string.Empty;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.GeminiApiKey);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.ProviderType))
        {
            aiSettings.ProviderType = ArtificialIntelligenceProviderType.Gemini;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.ProviderType);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.RequestTimeout))
        {
            aiSettings.RequestTimeout = ArtificialIntelligenceDefaults.RequestTimeout;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.RequestTimeout);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.ProductDescriptionQuery))
        {
            aiSettings.ProductDescriptionQuery = ArtificialIntelligenceDefaults.ProductDescriptionQuery;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.ProductDescriptionQuery);
        }

        //#5986
        if (!synchronousCodeHelper.SettingExists(mediaSettings, settings => settings.PicturePath))
        {
            mediaSettings.PicturePath = NopMediaDefaults.DefaultImagesPath;
            synchronousCodeHelper.SaveSetting(mediaSettings, settings => settings.PicturePath);
        }

        //#7390
        var menuSettings = synchronousCodeHelper.LoadSetting<MenuSettings>();
        if (!synchronousCodeHelper.SettingExists(menuSettings, settings => settings.MaximumNumberEntities))
        {
            menuSettings.MaximumNumberEntities = 8;
            synchronousCodeHelper.SaveSetting(menuSettings, settings => settings.MaximumNumberEntities);
        }

        if (!synchronousCodeHelper.SettingExists(menuSettings, settings => settings.NumberOfItemsPerGridRow))
        {
            menuSettings.NumberOfItemsPerGridRow = 4;
            synchronousCodeHelper.SaveSetting(menuSettings, settings => settings.NumberOfItemsPerGridRow);
        }

        if (!synchronousCodeHelper.SettingExists(menuSettings, settings => settings.NumberOfSubItemsPerGridElement))
        {
            menuSettings.NumberOfSubItemsPerGridElement = 3;
            synchronousCodeHelper.SaveSetting(menuSettings, settings => settings.NumberOfSubItemsPerGridElement);
        }

        if (!synchronousCodeHelper.SettingExists(menuSettings, settings => settings.MaximumMainMenuLevels))
        {
            menuSettings.MaximumMainMenuLevels = 2;
            synchronousCodeHelper.SaveSetting(menuSettings, settings => settings.MaximumMainMenuLevels);
        }

        if (!synchronousCodeHelper.SettingExists(menuSettings, settings => settings.GridThumbPictureSize))
        {
            menuSettings.GridThumbPictureSize = 340;
            synchronousCodeHelper.SaveSetting(menuSettings, settings => settings.GridThumbPictureSize);
        }

        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name.StartsWith("displaydefaultmenuitemsettings"));

        var useajaxloadmenu = synchronousCodeHelper.GetSetting("catalogsettings.useajaxloadmenu");
        if (useajaxloadmenu is not null)
            synchronousCodeHelper.DeleteSetting(useajaxloadmenu);

        //#7732
        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.AllowProductDescriptionGeneration))
        {
            aiSettings.AllowProductDescriptionGeneration = true;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.AllowProductDescriptionGeneration);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.AllowMetaTitleGeneration))
        {
            aiSettings.AllowMetaTitleGeneration = true;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.AllowMetaTitleGeneration);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.MetaTitleQuery))
        {
            aiSettings.MetaTitleQuery = ArtificialIntelligenceDefaults.MetaTitleQuery;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.MetaTitleQuery);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.AllowMetaKeywordsGeneration))
        {
            aiSettings.AllowMetaKeywordsGeneration = true;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.AllowMetaKeywordsGeneration);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.MetaKeywordsQuery))
        {
            aiSettings.MetaKeywordsQuery = ArtificialIntelligenceDefaults.MetaKeywordsQuery;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.MetaKeywordsQuery);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.AllowMetaDescriptionGeneration))
        {
            aiSettings.AllowMetaDescriptionGeneration = true;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.AllowMetaDescriptionGeneration);
        }

        if (!synchronousCodeHelper.SettingExists(aiSettings, settings => settings.MetaDescriptionQuery))
        {
            aiSettings.MetaDescriptionQuery = ArtificialIntelligenceDefaults.MetaDescriptionQuery;
            synchronousCodeHelper.SaveSetting(aiSettings, settings => settings.MetaDescriptionQuery);
        }

        //#7411
        var filterLevelSettings = synchronousCodeHelper.LoadSetting<FilterLevelSettings>();
        if (!synchronousCodeHelper.SettingExists(filterLevelSettings, settings => settings.DisplayOnHomePage))
        {
            filterLevelSettings.DisplayOnHomePage = true;
            synchronousCodeHelper.SaveSetting(filterLevelSettings, settings => settings.DisplayOnHomePage);
        }
        if (!synchronousCodeHelper.SettingExists(filterLevelSettings, settings => settings.DisplayOnProductDetailsPage))
        {
            filterLevelSettings.DisplayOnProductDetailsPage = true;
            synchronousCodeHelper.SaveSetting(filterLevelSettings, settings => settings.DisplayOnProductDetailsPage);
        }
        if (!synchronousCodeHelper.SettingExists(productEditorSettings, settings => settings.FilterLevelValuesProducts))
        {
            productEditorSettings.FilterLevelValuesProducts = true;
            synchronousCodeHelper.SaveSetting(productEditorSettings, settings => settings.FilterLevelValuesProducts);
        }

        //#7384
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.AllowCustomersCancelOrders))
        {
            orderSettings.AllowCustomersCancelOrders = true;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.AllowCustomersCancelOrders);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}
