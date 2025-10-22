using FluentMigrator;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Web.Framework.Migrations.UpgradeTo450;

[NopUpdateMigration("2021-04-23 00:00:00", "4.50", UpdateMigrationType.Settings)]
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

        //miniprofiler settings are moved to appSettings
        dataProvider.BulkDeleteEntities<Setting>(setting => setting.Name == "storeinformationsettings.displayminiprofilerforadminonly" ||
                               setting.Name == "storeinformationsettings.displayminiprofilerinpublicstore");

        //#4363
        var commonSettings = synchronousCodeHelper.LoadSetting<CommonSettings>();

        if (!synchronousCodeHelper.SettingExists(commonSettings, settings => settings.ClearLogOlderThanDays))
        {
            commonSettings.ClearLogOlderThanDays = 0;
            synchronousCodeHelper.SaveSetting(commonSettings, settings => settings.ClearLogOlderThanDays);
        }

        //#5551
        var catalogSettings = synchronousCodeHelper.LoadSetting<CatalogSettings>();

        if (!synchronousCodeHelper.SettingExists(catalogSettings, settings => settings.EnableSpecificationAttributeFiltering))
        {
            catalogSettings.EnableSpecificationAttributeFiltering = true;
            synchronousCodeHelper.SaveSetting(catalogSettings, settings => settings.EnableSpecificationAttributeFiltering);
        }

        //#5204
        var shippingSettings = synchronousCodeHelper.LoadSetting<ShippingSettings>();

        if (!synchronousCodeHelper.SettingExists(shippingSettings, settings => settings.ShippingSorting))
        {
            shippingSettings.ShippingSorting = ShippingSortingEnum.Position;
            synchronousCodeHelper.SaveSetting(shippingSettings, settings => settings.ShippingSorting);
        }

        //#5698
        var orderSettings = synchronousCodeHelper.LoadSetting<OrderSettings>();
        if (!synchronousCodeHelper.SettingExists(orderSettings, settings => settings.DisplayOrderSummary))
        {
            orderSettings.DisplayOrderSummary = true;
            synchronousCodeHelper.SaveSetting(orderSettings, settings => settings.DisplayOrderSummary);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}