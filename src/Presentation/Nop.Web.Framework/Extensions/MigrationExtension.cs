using System.Globalization;
using FluentMigrator;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Helpers;

namespace Nop.Web.Framework.Extensions;

public static partial class MigrationExtension
{
    /// <summary>
    /// Get language data
    /// </summary>
    /// <returns>Default language identifier and all languages</returns>
    public static (int? defaultLanguageId, IList<Language> allLanguages) GetLanguageData(this IMigration _, ISynchronousCodeHelper synchronousCodeHelper = null)
    {
        synchronousCodeHelper ??= EngineContext.Current.Resolve<ISynchronousCodeHelper>();

        var languages = synchronousCodeHelper.GetAllLanguages(true);
        var languageId = languages
            .FirstOrDefault(lang => lang.UniqueSeoCode == new CultureInfo(NopCommonDefaults.DefaultLanguageCulture).TwoLetterISOLanguageName)?.Id;

        return (languageId, languages);
    }

    /// <summary>
    /// Rename locales
    /// </summary>
    /// <param name="_">Migration</param>
    /// <param name="localesToRename">Locales to rename. Key - old name, Value - new name</param>
    /// <param name="allLanguages">All languages</param>
    /// <param name="synchronousCodeHelper">Synchronous code helper</param>
    public static void RenameLocales(this IMigration _, Dictionary<string, string> localesToRename, IList<Language> allLanguages, ISynchronousCodeHelper synchronousCodeHelper)
    {
        foreach (var lang in allLanguages)
        foreach (var locale in localesToRename)
        {
            var lsr = synchronousCodeHelper.GetLocaleStringResourceByName(locale.Key, lang.Id, false);
            
            if (lsr == null) 
                continue;

            var exist = synchronousCodeHelper.GetLocaleStringResourceByName(locale.Value, lang.Id, false);

            if (exist != null)
                synchronousCodeHelper.DeleteLocaleStringResource(lsr);
            else
            {
                lsr.ResourceName = locale.Value.ToLowerInvariant();
                synchronousCodeHelper.UpdateLocaleStringResource(lsr);
            }
        }
    }
}