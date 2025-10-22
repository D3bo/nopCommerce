using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Seo;

namespace Nop.Web.Framework.Migrations.UpgradeTo440;

[NopUpdateMigration("2020-06-10 00:00:00", "4.40", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var dataProvider = EngineContext.Current.Resolve<INopDataProvider>();
        var synchronousCodeHelper = EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        //#4904 External authentication errors logging
        var externalAuthenticationSettings = synchronousCodeHelper.LoadSetting<ExternalAuthenticationSettings>();
        if (!synchronousCodeHelper.SettingExists(externalAuthenticationSettings, settings => settings.LogErrors))
        {
            externalAuthenticationSettings.LogErrors = false;
            synchronousCodeHelper.SaveSetting(externalAuthenticationSettings, settings => settings.LogErrors);
        }

        var multiFactorAuthenticationSettings = synchronousCodeHelper.LoadSetting<MultiFactorAuthenticationSettings>();
        if (!synchronousCodeHelper.SettingExists(multiFactorAuthenticationSettings, settings => settings.ForceMultifactorAuthentication))
        {
            multiFactorAuthenticationSettings.ForceMultifactorAuthentication = false;

            synchronousCodeHelper.SaveSetting(multiFactorAuthenticationSettings, settings => settings.ForceMultifactorAuthentication);
        }

        //#5102 Delete Full-text settings
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == "commonsettings.usefulltextsearch" || setting.Name == "commonsettings.fulltextmode");

        //#4196
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == "commonsettings.scheduletaskruntimeout" ||
            setting.Name == "commonsettings.staticfilescachecontrol" ||
            setting.Name == "commonsettings.supportpreviousnopcommerceversions" ||
            setting.Name == "securitysettings.pluginstaticfileextensionsBlacklist");

        //#5384
        var seoSettings = synchronousCodeHelper.LoadSetting<SeoSettings>();
        foreach (var slug in NopSeoDefaults.ReservedUrlRecordSlugs)
        {
            if (!seoSettings.ReservedUrlRecordSlugs.Contains(slug))
                seoSettings.ReservedUrlRecordSlugs.Add(slug);
        }
        synchronousCodeHelper.SaveSetting(seoSettings, settings => seoSettings.ReservedUrlRecordSlugs);

        //#3015
        var homepageTitleKey = $"{nameof(SeoSettings)}.HomepageTitle".ToLower();
        if (synchronousCodeHelper.GetSettingByKey<string>(homepageTitleKey) == null)
            synchronousCodeHelper.SetSetting(homepageTitleKey, synchronousCodeHelper.GetSettingByKey<string>($"{nameof(SeoSettings)}.DefaultTitle"));

        var homepageDescriptionKey = $"{nameof(SeoSettings)}.HomepageDescription".ToLower();
        if (synchronousCodeHelper.GetSettingByKey<string>(homepageDescriptionKey) == null)
            synchronousCodeHelper.SetSetting(homepageDescriptionKey, "Your home page description");

        //#5210
        var adminAreaSettings = synchronousCodeHelper.LoadSetting<AdminAreaSettings>();
        if (!synchronousCodeHelper.SettingExists(adminAreaSettings, settings => settings.ShowDocumentationReferenceLinks))
        {
            adminAreaSettings.ShowDocumentationReferenceLinks = true;
            synchronousCodeHelper.SaveSetting(adminAreaSettings, settings => settings.ShowDocumentationReferenceLinks);
        }

        //#4944
        var shippingSettings = synchronousCodeHelper.LoadSetting<ShippingSettings>();
        if (!synchronousCodeHelper.SettingExists(shippingSettings, settings => settings.RequestDelay))
        {
            shippingSettings.RequestDelay = 300;
            synchronousCodeHelper.SaveSetting(shippingSettings, settings => settings.RequestDelay);
        }

        //#276 AJAX filters
        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.UseAjaxCatalogProductsLoading))
        {
            catalogSettings.UseAjaxCatalogProductsLoading = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.UseAjaxCatalogProductsLoading);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.EnableManufacturerFiltering))
        {
            catalogSettings.EnableManufacturerFiltering = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.EnableManufacturerFiltering);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.EnablePriceRangeFiltering))
        {
            catalogSettings.EnablePriceRangeFiltering = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.EnablePriceRangeFiltering);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.SearchPagePriceRangeFiltering))
        {
            catalogSettings.SearchPagePriceRangeFiltering = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.SearchPagePriceRangeFiltering);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.SearchPagePriceFrom))
        {
            catalogSettings.SearchPagePriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.SearchPagePriceFrom);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.SearchPagePriceTo))
        {
            catalogSettings.SearchPagePriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.SearchPagePriceTo);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.SearchPageManuallyPriceRange))
        {
            catalogSettings.SearchPageManuallyPriceRange = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.SearchPageManuallyPriceRange);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ProductsByTagPriceRangeFiltering))
        {
            catalogSettings.ProductsByTagPriceRangeFiltering = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ProductsByTagPriceRangeFiltering);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ProductsByTagPriceFrom))
        {
            catalogSettings.ProductsByTagPriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ProductsByTagPriceFrom);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ProductsByTagPriceTo))
        {
            catalogSettings.ProductsByTagPriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ProductsByTagPriceTo);
        }

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ProductsByTagManuallyPriceRange))
        {
            catalogSettings.ProductsByTagManuallyPriceRange = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ProductsByTagManuallyPriceRange);
        }

        //#4303
        var orderSettings = synchronousCodeHelper.LoadSetting<OrderSettings>();
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.DisplayCustomerCurrencyOnOrders))
        {
            orderSettings.DisplayCustomerCurrencyOnOrders = false;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.DisplayCustomerCurrencyOnOrders);
        }

        //#16 #2909
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.AttributeValueOutOfStockDisplayType))
        {
            catalogSettings.AttributeValueOutOfStockDisplayType = AttributeValueOutOfStockDisplayType.AlwaysDisplay;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.AttributeValueOutOfStockDisplayType);
        }

        //#5482
        synchronousCodeHelper.SetSetting("avalarataxsettings.gettaxratebyaddressonly", true);
        synchronousCodeHelper.SetSetting("avalarataxsettings.taxratebyaddresscachetime", 480);

        //#5349
        if (!synchronousCodeHelper.SettingExists(shippingSettings, settings => settings.EstimateShippingCityNameEnabled))
        {
            shippingSettings.EstimateShippingCityNameEnabled = false;
            synchronousCodeHelper.SaveSetting(shippingSettings, settings => settings.EstimateShippingCityNameEnabled);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}