using System.Linq.Expressions;
using Nop.Core.Configuration;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace Nop.Services.Helpers;

/// <summary>
/// User synchronous code helper interface
/// </summary>
public partial interface ISynchronousCodeHelper
{
    #region Setting

    /// <summary>
    /// Deletes a setting
    /// </summary>
    /// <param name="setting">Setting</param>
    void DeleteSetting(Setting setting);

    /// <summary>
    /// Get setting by key
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
    /// <returns>
    /// The setting
    /// </returns>
    Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false);

    /// <summary>
    /// Get setting value by key
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
    /// <returns>
    /// Setting value
    /// </returns>
    T GetSettingByKey<T>(string key, T defaultValue = default,
        int storeId = 0, bool loadSharedValueIfNotFound = false);

    /// <summary>
    /// Set setting value
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true);

    /// <summary>
    /// Determines whether a setting exists
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// The true -setting exists; false - does not exist
    /// </returns>
    bool SettingExists<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector, int storeId = 0)
        where T : ISettings, new();

    /// <summary>
    /// Load settings
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="storeId">Store identifier for which settings should be loaded</param>
    /// <returns>Settings</returns>
    T LoadSetting<T>(int storeId = 0) where T : ISettings, new();

    /// <summary>
    /// Save settings object
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="storeId">Store identifier</param>
    /// <param name="settings">Setting instance</param>
    void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new();

    /// <summary>
    /// Save settings object
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="storeId">Store ID</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    void SaveSetting<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector,
        int storeId = 0, bool clearCache = true) where T : ISettings, new();

    #endregion

    #region Language

    /// <summary>
    /// Gets all languages
    /// </summary>
    /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>
    /// The languages
    /// </returns>
    IList<Language> GetAllLanguages(bool showHidden = false, int storeId = 0);

    #endregion

    #region Localization

    /// <summary>
    /// Updates the locale string resource
    /// </summary>
    /// <param name="localeStringResource">Locale string resource</param>
    void UpdateLocaleStringResource(LocaleStringResource localeStringResource);

    /// <summary>
    /// Deletes a locale string resource
    /// </summary>
    /// <param name="localeStringResource">Locale string resource</param>
    void DeleteLocaleStringResource(LocaleStringResource localeStringResource);

    /// <summary>
    /// Gets a locale string resource
    /// </summary>
    /// <param name="resourceName">A string representing a resource name</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
    /// <returns>
    /// The locale string resource
    /// </returns>
    LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId,
        bool logIfNotFound = true);

    /// <summary>
    /// Add locale resources
    /// </summary>
    /// <param name="resources">Resource name-value pairs</param>
    /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
    void AddOrUpdateLocaleResource(IDictionary<string, string> resources, int? languageId = null);

    /// <summary>
    /// Delete locale resources
    /// </summary>
    /// <param name="resourceNames">Resource names</param>
    /// <param name="languageId">Language identifier; pass null to delete the passed resources from all languages</param>
    void DeleteLocaleResources(IList<string> resourceNames, int? languageId = null);

    #endregion

    #region Logger

    /// <summary>
    /// Information
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    void Information(string message, Exception exception = null, Customer customer = null);

    /// <summary>
    /// Warning
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    void Warning(string message, Exception exception = null, Customer customer = null);

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    void Error(string message, Exception exception = null, Customer customer = null);

    #endregion

    #region Store

    /// <summary>
    /// Updates the store
    /// </summary>
    /// <param name="store">Store</param>
    void UpdateStore(Store store);

    /// <summary>
    /// Gets all stores
    /// </summary>
    /// <returns>
    /// The stores
    /// </returns>
    IList<Store> GetAllStores();

    /// <summary>
    /// Gets the current store
    /// </summary>
    Store GetCurrentStore();

    #endregion
}