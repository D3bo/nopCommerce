using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Plugin.ExchangeRate.EcbExchange.Data;

[NopMigration("2021-09-16 00:00:00", "ExchangeRate.EcbExchange 1.30. Add setting for url for ECB", MigrationProcessType.Update)]
public class ExchangeEcbMigration : MigrationBase
{
    #region Fields

    protected readonly EcbExchangeRateSettings _ecbExchangeRateSettings;
    protected readonly ISynchronousCodeHelper _synchronousCodeHelper;

    #endregion

    #region Ctor

    public ExchangeEcbMigration(EcbExchangeRateSettings ecbExchangeRateSettings,
        ISynchronousCodeHelper synchronousCodeHelper)
    {
        _ecbExchangeRateSettings = ecbExchangeRateSettings;
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

        //settings
        if (!_synchronousCodeHelper.SettingExists(_ecbExchangeRateSettings, settings => settings.EcbLink))
            _ecbExchangeRateSettings.EcbLink = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

        _synchronousCodeHelper.SaveSetting(_ecbExchangeRateSettings);
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