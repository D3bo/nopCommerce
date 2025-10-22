using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;

namespace Nop.Services.Helpers;

/// <summary>
/// User synchronous code helper implementation
/// </summary>
public partial class SynchronousCodeHelper : ISynchronousCodeHelper
{
    #region Fields

    protected readonly CatalogSettings _catalogSettings;
    protected readonly CommonSettings _commonSettings;
    protected readonly CustomerSettings _customerSettings;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly INopDataProvider _dataProvider;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreService _storeService;
    protected readonly IWebHelper _webHelper;

    protected Store _cachedStore;

    #endregion

    #region Ctor

    public SynchronousCodeHelper(CatalogSettings catalogSettings,
        CommonSettings commonSettings,
        CustomerSettings customerSettings,
        IHttpContextAccessor httpContextAccessor,
        INopDataProvider dataProvider,
        IShortTermCacheManager shortTermCacheManager,
        IStaticCacheManager staticCacheManager,
        IStoreService storeService,
        IWebHelper webHelper)
    {
        _catalogSettings = catalogSettings;
        _commonSettings = commonSettings;
        _customerSettings = customerSettings;
        _httpContextAccessor = httpContextAccessor;
        _dataProvider = dataProvider;
        _shortTermCacheManager = shortTermCacheManager;
        _staticCacheManager = staticCacheManager;
        _storeService = storeService;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Load settings
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="storeId">Store identifier for which settings should be loaded</param>
    /// <returns>Settings</returns>
    protected virtual ISettings LoadSetting(Type type, int storeId = 0)
    {
        var settings = Activator.CreateInstance(type);

        if (!DataSettingsManager.IsDatabaseInstalled())
            return settings as ISettings;

        foreach (var prop in type.GetProperties())
        {
            // get properties we can read and write to
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var key = type.Name + "." + prop.Name;
            //load by store
            var setting = GetSettingByKey<string>(key, storeId: storeId, loadSharedValueIfNotFound: true);
            if (setting == null)
                continue;

            if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                continue;

            if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                continue;

            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

            //set property
            prop.SetValue(settings, value, null);
        }

        return settings as ISettings;
    }

    /// <summary>
    /// Prepare log item
    /// </summary>
    /// <param name="logLevel">Log level</param>
    /// <param name="shortMessage">The short message</param>
    /// <param name="fullMessage">The full message</param>
    /// <param name="customer">The customer to associate log record with</param>
    /// <returns>Log item</returns>
    protected virtual Log PrepareLog(LogLevel logLevel, string shortMessage, string fullMessage = "",
        Customer customer = null)
    {
        return new Log
        {
            LogLevel = logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            IpAddress = _customerSettings.StoreIpAddresses ? _webHelper.GetCurrentIpAddress() : string.Empty,
            CustomerId = customer?.Id,
            PageUrl = _webHelper.GetThisPageUrl(true),
            ReferrerUrl = _webHelper.GetUrlReferrer(),
            CreatedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a value indicating whether this message should not be logged
    /// </summary>
    /// <param name="message">Message</param>
    /// <returns>Result</returns>
    protected virtual bool IgnoreLog(string message)
    {
        if (!_commonSettings.IgnoreLogWordlist.Any())
            return false;

        if (string.IsNullOrWhiteSpace(message))
            return false;

        return _commonSettings
            .IgnoreLogWordlist
            .Any(x => message.Contains(x, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Determines whether a log level is enabled
    /// </summary>
    /// <param name="level">Log level</param>
    /// <returns>Result</returns>
    protected virtual bool IsLogEnabled(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => false,
            _ => true,
        };
    }

    protected virtual IDictionary<string, string> UpdateLocaleResource(IDictionary<string, string> resources,
        int? languageId = null, bool clearCache = true)
    {
        var localResources = new Dictionary<string, string>(resources, StringComparer.InvariantCultureIgnoreCase);
        var keys = localResources.Keys.Select(key => key.ToLowerInvariant()).ToArray();
        var resourcesToUpdate = GetAll<LocaleStringResource>(query =>
        {
            var rez = query.Where(p => !languageId.HasValue || p.LanguageId == languageId)
                .Where(p => keys.Contains(p.ResourceName.ToLower()));

            return rez;
        });

        var existsResources = new List<string>();

        foreach (var localeStringResource in resourcesToUpdate.ToList())
        {
            var newValue = localResources[localeStringResource.ResourceName];

            if (localeStringResource.ResourceValue.Equals(newValue))
                resourcesToUpdate.Remove(localeStringResource);

            localeStringResource.ResourceValue = newValue;
            existsResources.Add(localeStringResource.ResourceName);
        }

        Update(resourcesToUpdate);

        //clear cache
        if (clearCache)
            _staticCacheManager.RemoveByPrefix(NopEntityCacheDefaults<LocaleStringResource>.Prefix);

        return localResources
            .Where(item => !existsResources.Contains(item.Key, StringComparer.InvariantCultureIgnoreCase))
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    /// Gets all settings
    /// </summary>
    /// <returns>
    /// Settings
    /// </returns>
    protected virtual IList<Setting> GetAllSettings()
    {
        var settings = GetAll<Setting>(query => from s in query
                                                orderby s.Name, s.StoreId
                                                select s, _ => default);

        return settings;
    }

    /// <summary>
    /// Gets all settings
    /// </summary>
    /// <returns>
    /// Settings
    /// </returns>
    protected virtual IDictionary<string, IList<Setting>> GetAllSettingsDictionary()
    {
        return _staticCacheManager.Get(NopSettingsDefaults.SettingsAllAsDictionaryCacheKey, () =>
        {
            var settings = GetAllSettings();

            var dictionary = new Dictionary<string, IList<Setting>>();
            foreach (var s in settings)
            {
                var resourceName = s.Name.ToLowerInvariant();
                var settingForCaching = new Setting { Id = s.Id, Name = s.Name, Value = s.Value, StoreId = s.StoreId };
                if (!dictionary.TryGetValue(resourceName, out var value))
                    //first setting
                    dictionary.Add(resourceName, new List<Setting> { settingForCaching });
                else
                    //already added
                    //most probably it's the setting with the same name but for some certain store (storeId > 0)
                    value.Add(settingForCaching);
            }

            return dictionary;
        });
    }

    /// <summary>
    /// Get setting key (stored into database)
    /// </summary>
    /// <typeparam name="TSettings">Type of settings</typeparam>
    /// <typeparam name="T">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <returns>Key</returns>
    protected virtual string GetSettingKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
        where TSettings : ISettings, new()
    {
        if (keySelector.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

        var key = $"{typeof(TSettings).Name}.{propInfo.Name}";

        return key;
    }

    /// <summary>
    /// Get all entity entries
    /// </summary>
    /// <param name="getAll">Function to select entries</param>
    /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
    /// <returns>Entity entries</returns>
    protected virtual IList<TEntity> GetEntities<TEntity>(Func<IList<TEntity>> getAll,
        Func<IStaticCacheManager, CacheKey> getCacheKey)
        where TEntity : BaseEntity
    {
        if (getCacheKey == null)
            return getAll();

        //caching
        var cacheKey = getCacheKey(_staticCacheManager)
            ?? _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<TEntity>.AllCacheKey);

        return _staticCacheManager.Get(cacheKey, getAll);
    }

    /// <summary>
    /// Adds "deleted" filter to query which contains <see cref="ISoftDeletedEntity"/> entries, if its need
    /// </summary>
    /// <param name="query">Entity entries</param>
    /// <param name="includeDeleted">Whether to include deleted items</param>
    /// <returns>Entity entries</returns>
    protected virtual IQueryable<TEntity> AddDeletedFilter<TEntity>(IQueryable<TEntity> query, in bool includeDeleted)
        where TEntity : BaseEntity
    {
        if (includeDeleted)
            return query;

        if (typeof(TEntity).GetInterface(nameof(ISoftDeletedEntity)) == null)
            return query;

        return query.OfType<ISoftDeletedEntity>().Where(entry => !entry.Deleted).OfType<TEntity>();
    }

    /// <summary>
    /// Get the entity entry
    /// </summary>
    /// <param name="id">Entity entry identifier</param>
    /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
    /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="ISoftDeletedEntity"/> entities)</param>
    /// <returns>
    /// The entity entry
    /// </returns>
    protected virtual TEntity GetById<TEntity>(int? id, Func<ICacheKeyService, CacheKey> getCacheKey = null,
        bool includeDeleted = true)
        where TEntity : BaseEntity
    {
        if (id is null or 0)
            return null;

        TEntity getEntity()
        {
            return AddDeletedFilter(_dataProvider.GetTable<TEntity>(), includeDeleted)
                .FirstOrDefault(entity => entity.Id == Convert.ToInt32(id));
        }

        if (getCacheKey == null)
            return getEntity();

        //caching
        var cacheKey = getCacheKey(_staticCacheManager)
            ?? _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<TEntity>.ByIdCacheKey, id);

        return _staticCacheManager.Get(cacheKey, getEntity);
    }

    /// <summary>
    /// Get all entity entries
    /// </summary>
    /// <param name="func">Function to select entries</param>
    /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
    /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="Nop.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
    /// <returns>Entity entries</returns>
    protected virtual IList<TEntity> GetAll<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
        Func<ICacheKeyService, CacheKey> getCacheKey = null, bool includeDeleted = true)
        where TEntity : BaseEntity
    {
        IList<TEntity> getAll()
        {
            var query = AddDeletedFilter(_dataProvider.GetTable<TEntity>(), includeDeleted);
            query = func != null ? func(query) : query;

            return query.ToList();
        }

        return GetEntities(getAll, getCacheKey);
    }

    /// <summary>
    /// Insert the entity entry
    /// </summary>
    /// <param name="entity">Entity entry</param>
    protected virtual void Insert<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dataProvider.InsertEntity(entity);
    }

    /// <summary>
    /// Insert entity entries
    /// </summary>
    /// <param name="entities">Entity entries</param>
    protected virtual void Insert<TEntity>(IList<TEntity> entities)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        _dataProvider.BulkInsertEntities(entities);
        transaction.Complete();
    }

    /// <summary>
    /// Update the entity entry
    /// </summary>
    /// <param name="entity">Entity entry</param>
    protected virtual void Update<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dataProvider.UpdateEntity(entity);
    }

    /// <summary>
    /// Update entity entries
    /// </summary>
    /// <param name="entities">Entity entries</param>
    protected virtual void Update<TEntity>(IList<TEntity> entities)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (!entities.Any())
            return;

        _dataProvider.UpdateEntities(entities);
    }

    /// <summary>
    /// Delete the entity entry
    /// </summary>
    /// <param name="entity">Entity entry</param>
    protected virtual void Delete<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        switch (entity)
        {
            case ISoftDeletedEntity softDeletedEntity:
                softDeletedEntity.Deleted = true;
                _dataProvider.UpdateEntity(entity);
                break;

            default:
                _dataProvider.DeleteEntity(entity);
                break;
        }
    }

    /// <summary>
    /// Delete entity entries
    /// </summary>
    /// <param name="entities">Entity entries</param>
    protected virtual void Delete<TEntity>(IList<TEntity> entities)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (!entities.Any())
            return;

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (typeof(TEntity).GetInterface(nameof(ISoftDeletedEntity)) == null)
            _dataProvider.BulkDeleteEntities(entities);
        else
        {
            foreach (var entity in entities)
                ((ISoftDeletedEntity)entity).Deleted = true;

            _dataProvider.UpdateEntities(entities);
        }

        transaction.Complete();
    }

    /// <summary>
    /// Delete entity entries by the passed predicate
    /// </summary>
    /// <param name="predicate">A function to test each element for a condition</param>
    /// <returns>
    /// The number of deleted records
    /// </returns>
    protected virtual int Delete<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(predicate);

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var countDeletedRecords = _dataProvider.BulkDeleteEntities(predicate);
        transaction.Complete();

        return countDeletedRecords;
    }

    /// <summary>
    /// Set setting value
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    protected virtual void SetSetting(Type type, string key, object value, int storeId = 0, bool clearCache = true)
    {
        ArgumentNullException.ThrowIfNull(key);
        key = key.Trim().ToLowerInvariant();
        var valueStr = TypeDescriptor.GetConverter(type).ConvertToInvariantString(value);

        var allSettings = GetAllSettingsDictionary();
        var settingForCaching = allSettings.TryGetValue(key, out var settings)
            ? settings.FirstOrDefault(x => x.StoreId == storeId)
            : null;
        if (settingForCaching != null)
        {
            //update
            var setting = GetSettingById(settingForCaching.Id);
            setting.Value = valueStr;
            UpdateSetting(setting, clearCache);
        }
        else
        {
            //insert
            var setting = new Setting { Name = key, Value = valueStr, StoreId = storeId };
            InsertSetting(setting, clearCache);
        }
    }

    /// <summary>
    /// Gets a setting by identifier
    /// </summary>
    /// <param name="settingId">Setting identifier</param>
    /// <returns>
    /// The setting
    /// </returns>
    protected virtual Setting GetSettingById(int settingId)
    {
        return GetById<Setting>(settingId, _ => default);
    }

    /// <summary>
    /// Authorize whether entity could be accessed in a store (mapped to this store)
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports store mapping</typeparam>
    /// <param name="entity">Entity</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// True - authorized; otherwise, false
    /// </returns>
    protected virtual bool AuthorizeStoreMapping<TEntity>(TEntity entity, int storeId)
        where TEntity : BaseEntity, IStoreMappingSupported
    {
        if (entity == null)
            return false;

        if (storeId == 0)
            //return true if no store specified/found
            return true;

        if (_catalogSettings.IgnoreStoreLimitations)
            return true;

        if (!entity.LimitedToStores)
            return true;

        foreach (var storeIdWithAccess in GetStoresIdsWithAccess(entity))
            if (storeId == storeIdWithAccess)
                //yes, we have such permission
                return true;

        //no permission found
        return false;
    }

    /// <summary>
    /// Find store identifiers with granted access (mapped to the entity)
    /// </summary>
    /// <typeparam name="TEntity">Type of entity that supports store mapping</typeparam>
    /// <param name="entity">Entity</param>
    /// <returns>
    /// The store identifiers
    /// </returns>
    protected virtual int[] GetStoresIdsWithAccess<TEntity>(TEntity entity)
        where TEntity : BaseEntity, IStoreMappingSupported
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entityId = entity.Id;
        var entityName = entity.GetType().Name;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopStoreDefaults.StoreMappingIdsCacheKey, entityId,
            entityName);

        var query = from sm in _dataProvider.GetTable<StoreMapping>()
                    where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                    select sm.StoreId;

        return _staticCacheManager.Get(key, () => query.ToArray());
    }

    /// <summary>
    /// Adds a setting
    /// </summary>
    /// <param name="setting">Setting</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    protected virtual void InsertSetting(Setting setting, bool clearCache = true)
    {
        Insert(setting);

        //cache
        if (clearCache)
            ClearCache();
    }

    /// <summary>
    /// Updates a setting
    /// </summary>
    /// <param name="setting">Setting</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    protected virtual void UpdateSetting(Setting setting, bool clearCache = true)
    {
        ArgumentNullException.ThrowIfNull(setting);

        Update(setting);

        //cache
        if (clearCache)
            ClearCache();
    }

    /// <summary>
    /// Clear cache
    /// </summary>
    protected virtual void ClearCache()
    {
        _staticCacheManager.RemoveByPrefix(NopEntityCacheDefaults<Setting>.Prefix);
    }

    /// <summary>
    /// Inserts a log item
    /// </summary>
    /// <param name="logLevel">Log level</param>
    /// <param name="shortMessage">The short message</param>
    /// <param name="fullMessage">The full message</param>
    /// <param name="customer">The customer to associate log record with</param>
    protected virtual void InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "",
        Customer customer = null)
    {
        //check ignore word/phrase list?
        if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
            return;

        Insert(PrepareLog(logLevel, shortMessage, fullMessage, customer));
    }

    #endregion

    #region Methods

    #region Setting

    /// <summary>
    /// Deletes a setting
    /// </summary>
    /// <param name="setting">Setting</param>
    public virtual void DeleteSetting(Setting setting)
    {
        Delete(setting);

        //cache
        ClearCache();
    }

    /// <summary>
    /// Get setting by key
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
    /// <returns>
    /// The setting
    /// </returns>
    public virtual Setting GetSetting(string key, int storeId = 0, bool loadSharedValueIfNotFound = false)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        var settings = GetAllSettingsDictionary();
        key = key.Trim().ToLowerInvariant();
        if (!settings.TryGetValue(key, out var value))
            return null;

        var settingsByKey = value;
        var setting = settingsByKey.FirstOrDefault(x => x.StoreId == storeId);

        //load shared value?
        if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
            setting = settingsByKey.FirstOrDefault(x => x.StoreId == 0);

        return setting;
    }

    /// <summary>
    /// Get setting value by key
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain is not found</param>
    /// <returns>
    /// Setting value
    /// </returns>
    public virtual T GetSettingByKey<T>(string key, T defaultValue = default,
        int storeId = 0, bool loadSharedValueIfNotFound = false)
    {
        if (string.IsNullOrEmpty(key))
            return defaultValue;

        var settings = GetAllSettingsDictionary();
        key = key.Trim().ToLowerInvariant();
        if (!settings.TryGetValue(key, out var value))
            return defaultValue;

        var settingsByKey = value;
        var setting = settingsByKey.FirstOrDefault(x => x.StoreId == storeId);

        //load shared value?
        if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
            setting = settingsByKey.FirstOrDefault(x => x.StoreId == 0);

        return setting != null ? CommonHelper.To<T>(setting.Value) : defaultValue;
    }

    /// <summary>
    /// Set setting value
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    public virtual void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
    {
        SetSetting(typeof(T), key, value, storeId, clearCache);
    }

    /// <summary>
    /// Determines whether a setting exists
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Entity</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// The true -setting exists; false - does not exist
    /// </returns>
    public virtual bool SettingExists<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector, int storeId = 0)
        where T : ISettings, new()
    {
        var key = GetSettingKey(settings, keySelector);

        var setting = GetSettingByKey<string>(key, storeId: storeId);
        return setting != null;
    }

    /// <summary>
    /// Load settings
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="storeId">Store identifier for which settings should be loaded</param>
    public virtual T LoadSetting<T>(int storeId = 0) where T : ISettings, new()
    {
        return (T)LoadSetting(typeof(T), storeId);
    }

    /// <summary>
    /// Save settings object
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="storeId">Store identifier</param>
    /// <param name="settings">Setting instance</param>
    public virtual void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new()
    {
        /* We do not clear cache after each setting update.
         * This behavior can increase performance because cached settings will not be cleared
         * and loaded from database after each update */
        foreach (var prop in typeof(T).GetProperties())
        {
            // get properties we can read and write to
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                continue;

            var key = typeof(T).Name + "." + prop.Name;
            var value = prop.GetValue(settings, null);
            if (value != null)
                SetSetting(prop.PropertyType, key, value, storeId, false);
            else
                SetSetting(key, string.Empty, storeId, false);
        }

        //and now clear cache
        ClearCache();
    }

    /// <summary>
    /// Save settings object
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TPropType">Property type</typeparam>
    /// <param name="settings">Settings</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="storeId">Store ID</param>
    /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
    public virtual void SaveSetting<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector,
        int storeId = 0, bool clearCache = true) where T : ISettings, new()
    {
        if (keySelector.Body is not MemberExpression member)
            throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

        var propInfo = member.Member as PropertyInfo
            ?? throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

        var key = GetSettingKey(settings, keySelector);
        var value = (TPropType)propInfo.GetValue(settings, null);
        if (value != null)
            SetSetting(key, value, storeId, clearCache);
        else
            SetSetting(key, string.Empty, storeId, clearCache);
    }

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
    public virtual IList<Language> GetAllLanguages(bool showHidden = false, int storeId = 0)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopLocalizationDefaults.LanguagesAllCacheKey, storeId,
            showHidden);

        var languages = _staticCacheManager.Get(key, () =>
        {
            var allLanguages = GetAll<Language>(query =>
            {
                if (!showHidden)
                    query = query.Where(l => l.Published);
                query = query.OrderBy(l => l.DisplayOrder).ThenBy(l => l.Id);

                return query;
            });

            //store mapping
            if (storeId > 0)
                allLanguages = allLanguages
                    .Where(l => AuthorizeStoreMapping(l, storeId))
                    .ToList();

            return allLanguages;
        });

        return languages;
    }

    #endregion

    #region Localization

    /// <summary>
    /// Updates the locale string resource
    /// </summary>
    /// <param name="localeStringResource">Locale string resource</param>
    public virtual void UpdateLocaleStringResource(LocaleStringResource localeStringResource)
    {
        Update(localeStringResource);
    }

    /// <summary>
    /// Deletes a locale string resource
    /// </summary>
    /// <param name="localeStringResource">Locale string resource</param>
    public virtual void DeleteLocaleStringResource(LocaleStringResource localeStringResource)
    {
        Delete(localeStringResource);
    }

    /// <summary>
    /// Gets a locale string resource
    /// </summary>
    /// <param name="resourceName">A string representing a resource name</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
    /// <returns>
    /// The locale string resource
    /// </returns>
    public virtual LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId,
        bool logIfNotFound = true)
    {
        var query = from lsr in _dataProvider.GetTable<LocaleStringResource>()
                    orderby lsr.ResourceName
                    where lsr.LanguageId == languageId && lsr.ResourceName == resourceName.ToLowerInvariant()
                    select lsr;

        var localeStringResource = query.FirstOrDefault();

        if (localeStringResource == null && logIfNotFound)
            Warning($"Resource string ({resourceName}) not found. Language ID = {languageId}");

        return localeStringResource;
    }

    /// <summary>
    /// Add locale resources
    /// </summary>
    /// <param name="resources">Resource name-value pairs</param>
    /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
    public virtual void AddOrUpdateLocaleResource(IDictionary<string, string> resources, int? languageId = null)
    {
        //first update all previous locales with the passed names if they exist
        var resourcesToInsert = UpdateLocaleResource(resources, languageId, false);

        if (resourcesToInsert.Any())
        {
            //insert new locale resources
            var locales = GetAllLanguages(true)
                .Where(language => !languageId.HasValue || language.Id == languageId.Value)
                .SelectMany(language => resourcesToInsert.Select(resource => new LocaleStringResource
                {
                    LanguageId = language.Id,
                    ResourceName = resource.Key.Trim().ToLowerInvariant(),
                    ResourceValue = resource.Value
                }))
                .ToList();

            Insert(locales);
        }

        //clear cache
        _staticCacheManager.RemoveByPrefix(NopEntityCacheDefaults<LocaleStringResource>.Prefix);
    }

    /// <summary>
    /// Delete locale resources
    /// </summary>
    /// <param name="resourceNames">Resource names</param>
    /// <param name="languageId">Language identifier; pass null to delete the passed resources from all languages</param>
    public virtual void DeleteLocaleResources(IList<string> resourceNames, int? languageId = null)
    {
        Delete<LocaleStringResource>(locale =>
            (!languageId.HasValue || locale.LanguageId == languageId.Value) &&
            resourceNames.Contains(locale.ResourceName, StringComparer.InvariantCultureIgnoreCase));

        //clear cache
        _staticCacheManager.RemoveByPrefix(NopEntityCacheDefaults<LocaleStringResource>.Prefix);
    }

    #endregion

    #region Logger

    /// <summary>
    /// Information
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    public virtual void Information(string message, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is ThreadAbortException)
            return;

        if (IsLogEnabled(LogLevel.Information))
            InsertLog(LogLevel.Information, message, exception?.ToString() ?? string.Empty, customer);
    }

    /// <summary>
    /// Warning
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    public virtual void Warning(string message, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is ThreadAbortException)
            return;

        if (IsLogEnabled(LogLevel.Warning))
            InsertLog(LogLevel.Warning, message, exception?.ToString() ?? string.Empty, customer);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="exception">Exception</param>
    /// <param name="customer">Customer</param>
    public virtual void Error(string message, Exception exception = null, Customer customer = null)
    {
        //don't log thread abort exception
        if (exception is ThreadAbortException)
            return;

        if (IsLogEnabled(LogLevel.Error))
            InsertLog(LogLevel.Error, message, exception?.ToString() ?? string.Empty, customer);
    }

    #endregion

    #region Store

    /// <summary>
    /// Gets all stores
    /// </summary>
    /// <returns>
    /// The stores
    /// </returns>
    public virtual IList<Store> GetAllStores()
    {
        return GetAll<Store>(query => query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id), _ => default,
            includeDeleted: false);
    }

    /// <summary>
    /// Updates the store
    /// </summary>
    /// <param name="store">Store</param>
    public virtual void UpdateStore(Store store)
    {
        Update(store);
    }

    /// <summary>
    /// Gets the current store
    /// </summary>
    public virtual Store GetCurrentStore()
    {
        if (_cachedStore != null)
            return _cachedStore;

        //try to determine the current store by HOST header
        string host = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Host];

        //we cannot call async methods here. otherwise, an application can hang. so it's a workaround to avoid that
        var allStores = GetAll<Store>(query => query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id), _ => default, includeDeleted: false);

        var store = allStores.FirstOrDefault(s => _storeService.ContainsHostValue(s, host)) ??
            allStores.FirstOrDefault();

        _cachedStore = store ?? throw new Exception("No store could be loaded");

        return _cachedStore;
    }

    #endregion

    #endregion
}
