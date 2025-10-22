using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Services.Helpers;

namespace Nop.Plugin.Shipping.UPS.Migrations;

[NopMigration("2023-12-13 20:00:00", "Shipping.UPS Update to v2.0", MigrationProcessType.Update)]
public class UpgradeTo470 : Migration
{
    private ISynchronousCodeHelper _synchronousCodeHelper;

    public UpgradeTo470(ISynchronousCodeHelper synchronousCodeHelper)
    {
        _synchronousCodeHelper = synchronousCodeHelper;
    }

    public override void Up()
    {
        _synchronousCodeHelper.DeleteLocaleResources(new[]
        {
            "Plugins.Shipping.UPS.Fields.Password",
            "Plugins.Shipping.UPS.Fields.Password.Hint",
            "Plugins.Shipping.UPS.Fields.Username",
            "Plugins.Shipping.UPS.Fields.Username.Hint"
        });

        _synchronousCodeHelper.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.Shipping.UPS.Fields.ClientId"] = "Client ID",
            ["Plugins.Shipping.UPS.Fields.ClientId.Hint"] = "Specify UPS client ID.",
            ["Plugins.Shipping.UPS.Fields.ClientSecret"] = "Client secret",
            ["Plugins.Shipping.UPS.Fields.ClientSecret.Hint"] = "Specify UPS client secret.",
            ["Plugins.Shipping.UPS.Fields.Tracing.Hint"] = "Check if you want to record plugin tracing in System Log. Warning: The entire request and response will be logged (including Client Id/secret, AccountNumber). Do not leave this enabled in a production environment."
        });

        var setting = _synchronousCodeHelper.LoadSetting<UPSSettings>();
        if (!_synchronousCodeHelper.SettingExists(setting, settings => settings.RequestTimeout))
        {
            setting.RequestTimeout = UPSDefaults.RequestTimeout;
            _synchronousCodeHelper.SaveSetting(setting, settings => settings.RequestTimeout);
        }
            
        var accessKey = _synchronousCodeHelper.GetSetting("upssettings.accesskey");
        if (accessKey is not null) 
            _synchronousCodeHelper.DeleteSetting(accessKey);

        var username = _synchronousCodeHelper.GetSetting("upssettings.username");
        if (username is not null)
            _synchronousCodeHelper.DeleteSetting(username);

        var password = _synchronousCodeHelper.GetSetting("upssettings.password");
        if (password is not null)
            _synchronousCodeHelper.DeleteSetting(password);
    }

    public override void Down()
    {
        //add the downgrade logic if necessary 
    }
}