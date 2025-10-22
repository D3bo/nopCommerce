using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Web.Framework.Migrations.UpgradeTo480;

[NopUpdateMigration("2024-11-01 00:00:00", "4.80", UpdateMigrationType.Settings)]
public class SettingMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var synchronousCodeHelper = EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        //#7215
        var displayAttributeCombinationImagesOnly = synchronousCodeHelper.GetSetting("producteditorsettings.displayattributecombinationimagesonly");
        if (displayAttributeCombinationImagesOnly is not null)
            synchronousCodeHelper.DeleteSetting(displayAttributeCombinationImagesOnly);

        //#7325
        var orderSettings = synchronousCodeHelper.LoadSetting<OrderSettings>();
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.PlaceOrderWithLock))
        {
            orderSettings.PlaceOrderWithLock = false;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.PlaceOrderWithLock);
        }
        
        //#7394
        if (orderSettings.MinimumOrderPlacementInterval > 10)
        {
            if (orderSettings.MinimumOrderPlacementInterval < 60)
                orderSettings.MinimumOrderPlacementInterval = 1;
            else
                orderSettings.MinimumOrderPlacementInterval = Math.Truncate(orderSettings.MinimumOrderPlacementInterval / 60.0) + (orderSettings.MinimumOrderPlacementInterval % 60) == 0 ? 0 : 1;

            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.MinimumOrderPlacementInterval);
        }

        //#7265
        var taxSetting = synchronousCodeHelper.LoadSetting<TaxSettings>();
        if (!synchronousCodeHelper.SettingExists(taxSetting, settings => settings.EuVatRequired))
        {
            taxSetting.EuVatRequired = false;
            synchronousCodeHelper.SaveSetting(taxSetting, settings => settings.EuVatRequired);
        }

        //#4306
        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ShowSearchBoxCategories))
        {
            catalogSettings.ShowSearchBoxCategories = false;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ShowSearchBoxCategories);
        }

        //#2388
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.ExportImportTierPrices))
        {
            catalogSettings.ExportImportTierPrices = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.ExportImportTierPrices);
        }

        //#7228
        var adminAreaSettings = synchronousCodeHelper.LoadSetting<AdminAreaSettings>();
        if (!synchronousCodeHelper.SettingExists(adminAreaSettings, settings => settings.ProductsBulkEditGridPageSize))
        {
            adminAreaSettings.ProductsBulkEditGridPageSize = 100;
            synchronousCodeHelper.SaveSetting(adminAreaSettings, settings => settings.ProductsBulkEditGridPageSize);
        }

        //#7244
        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.VendorProductReviewsPageSize))
        {
            catalogSettings.VendorProductReviewsPageSize = 6;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.VendorProductReviewsPageSize);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}