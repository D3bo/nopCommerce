using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Web.Framework.Migrations.UpgradeTo470;

[NopUpdateMigration("2023-02-01 14:00:03", "4.70", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var synchronousCodeHelper = EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        var customerSettings = synchronousCodeHelper.LoadSetting<CustomerSettings>();
        if (!synchronousCodeHelper.SettingExists(customerSettings, settings => settings.PasswordMaxLength))
        {
            customerSettings.PasswordMaxLength = 64;
            synchronousCodeHelper.SaveSetting(customerSettings, settings => settings.PasswordMaxLength);
        }

        if (!synchronousCodeHelper.SettingExists(customerSettings, settings => settings.DefaultCountryId))
        {
            customerSettings.DefaultCountryId = null;
            synchronousCodeHelper.SaveSetting(customerSettings, settings => settings.DefaultCountryId);
        }

        var securitySettings = synchronousCodeHelper.LoadSetting<SecuritySettings>();
        if (!synchronousCodeHelper.SettingExists(securitySettings, settings => settings.UseAesEncryptionAlgorithm))
        {
            securitySettings.UseAesEncryptionAlgorithm = false;
            synchronousCodeHelper.SaveSetting(securitySettings, settings => settings.UseAesEncryptionAlgorithm);
        }

        if (!synchronousCodeHelper.SettingExists(securitySettings, settings => settings.AllowStoreOwnerExportImportCustomersWithHashedPassword))
        {
            securitySettings.AllowStoreOwnerExportImportCustomersWithHashedPassword = true;
            synchronousCodeHelper.SaveSetting(securitySettings, settings => settings.AllowStoreOwnerExportImportCustomersWithHashedPassword);
        }

        //#7053
        if (!synchronousCodeHelper.SettingExists(securitySettings, settings => settings.LogHoneypotDetection))
        {
            securitySettings.LogHoneypotDetection = true;
            synchronousCodeHelper.SaveSetting(securitySettings, settings => settings.LogHoneypotDetection);
        }

        var addressSettings = synchronousCodeHelper.LoadSetting<AddressSettings>();
        if (!synchronousCodeHelper.SettingExists(addressSettings, settings => settings.DefaultCountryId))
        {
            addressSettings.DefaultCountryId = null;
            synchronousCodeHelper.SaveSetting(addressSettings, settings => settings.DefaultCountryId);
        }

        var captchaSettings = synchronousCodeHelper.LoadSetting<CaptchaSettings>();
        //#6682
        if (!synchronousCodeHelper.SettingExists(captchaSettings, settings => settings.ShowOnNewsletterPage))
        {
            captchaSettings.ShowOnNewsletterPage = false;
            synchronousCodeHelper.SaveSetting(captchaSettings, settings => settings.ShowOnNewsletterPage);
        }

        var taxSettings = synchronousCodeHelper.LoadSetting<TaxSettings>();
        if (!synchronousCodeHelper.SettingExists(taxSettings, settings => settings.AutomaticallyDetectCountry))
        {
            taxSettings.AutomaticallyDetectCountry = true;
            synchronousCodeHelper.SaveSetting(taxSettings, settings => settings.AutomaticallyDetectCountry);
        }

        //#6716
        var newDisallowPaths = new[]
        {
            "/cart/estimateshipping", "/cart/selectshippingoption", "/customer/addressdelete",
            "/customer/removeexternalassociation", "/customer/checkusernameavailability",
            "/catalog/searchtermautocomplete", "/catalog/getcatalogroot", "/addproducttocart/catalog/*",
            "/addproducttocart/details/*", "/compareproducts/add/*", "/backinstocksubscribe/*",
            "/subscribenewsletter", "/t-popup/*", "/setproductreviewhelpfulness", "/poll/vote",
            "/country/getstatesbycountryid/", "/eucookielawaccept", "/topic/authenticate",
            "/category/products/", "/product/combinations", "/uploadfileproductattribute/*",
            "/shoppingcart/productdetails_attributechange/*", "/uploadfilereturnrequest",
            "/boards/topicwatch/*", "/boards/forumwatch/*", "/install/restartapplication",
            "/boards/postvote", "/product/estimateshipping/*", "/shoppingcart/checkoutattributechange/*"
        };

        var robotsTxtSettings = synchronousCodeHelper.LoadSetting<RobotsTxtSettings>();

        foreach (var path in newDisallowPaths)
        {
            if (robotsTxtSettings.DisallowPaths.Contains(path))
                continue;

            robotsTxtSettings.DisallowPaths.Add(path);
        }

        synchronousCodeHelper.SaveSetting(robotsTxtSettings, settings => settings.DisallowPaths);

        //#6853
        if (!synchronousCodeHelper.SettingExists(customerSettings, settings => settings.NeutralGenderEnabled))
        {
            customerSettings.NeutralGenderEnabled = false;
            synchronousCodeHelper.SaveSetting(customerSettings, settings => settings.NeutralGenderEnabled);
        }

        //#6891
        if (!synchronousCodeHelper.SettingExists(customerSettings, settings => settings.RequiredReLoginAfterPasswordChange))
        {
            customerSettings.RequiredReLoginAfterPasswordChange = false;
            synchronousCodeHelper.SaveSetting(customerSettings, settings => settings.RequiredReLoginAfterPasswordChange);
        }

        //#7064
        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.UseStandardSearchWhenSearchProviderThrowsException))
        {
            catalogSettings.UseStandardSearchWhenSearchProviderThrowsException = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.UseStandardSearchWhenSearchProviderThrowsException);
        }

    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}