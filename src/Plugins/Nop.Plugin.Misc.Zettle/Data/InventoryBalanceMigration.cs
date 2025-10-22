using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Zettle.Domain;
using Nop.Services.Helpers;

namespace Nop.Plugin.Misc.Zettle.Data;

[NopMigration("2024-04-16 00:00:00", "Misc.Zettle 4.70.5 Inventory balance tracking", MigrationProcessType.Update)]
public class InventoryBalanceMigration : MigrationBase
{
    #region Fields

    protected readonly ISynchronousCodeHelper _synchronousCodeHelper;

    #endregion

    #region Ctor

    public InventoryBalanceMigration(ISynchronousCodeHelper synchronousCodeHelper)
    {
        _synchronousCodeHelper = synchronousCodeHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //add column
        if (!Schema.Table(nameof(ZettleRecord)).Column(nameof(ZettleRecord.ExternalUuid)).Exists())
            Alter.Table(nameof(ZettleRecord)).AddColumn(nameof(ZettleRecord.ExternalUuid)).AsString().Nullable();

        //delete settings
        var setting = _synchronousCodeHelper.GetSetting($"{nameof(ZettleSettings)}.InventoryTrackingIds");
        if (setting is null)
            return;

        _synchronousCodeHelper.DeleteSetting(setting);
    }

    /// <summary>
    /// Collects the DOWN migration expressions
    /// </summary>
    public override void Down()
    {
        //nothing
    }

    #endregion
}