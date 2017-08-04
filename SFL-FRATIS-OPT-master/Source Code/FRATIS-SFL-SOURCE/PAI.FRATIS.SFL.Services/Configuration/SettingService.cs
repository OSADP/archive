//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

using PAI.FRATIS.SFL.Common.Infrastructure;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Domain.Configuration;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Security;

namespace PAI.FRATIS.SFL.Services.Configuration
{
    /// <summary>
    /// Setting service
    /// </summary>
    [MemoryCache]
    public partial class SettingService : EntityServiceBase<Setting>, ISettingService
    {
        private readonly IEngine _engine;
        private readonly IEncryptionService _encryptionService;
        private readonly ICacheManager _cacheManager;

        public override string CachePatternKey
        {
            get
            {
                return "SettingService.";
            }
        }

        public SettingService(IRepository<Setting> repository, ICacheManager cacheManager, IEngine engine, IEncryptionService encryptionService)
            : base(repository, cacheManager)
        {
            this._engine = engine;
            this._encryptionService = encryptionService;
            this._cacheManager = cacheManager;
        }

        public bool IsPropertyEncrypted(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(EncryptedSettingAttribute), false).FirstOrDefault() != null;
        }

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="isEncrypted"></param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Setting value</returns>
        public T GetSettingByKey<T>(string key, bool isEncrypted, T defaultValue = default(T))
        {
            T result = defaultValue;

            if (!string.IsNullOrWhiteSpace(key))
            {
                var settings = this.GetAllSettings();

                Setting setting = null;
                if (settings.TryGetValue(key, out setting))
                {
                    var settingValue = isEncrypted ? this._encryptionService.DecryptText(setting.Value) : setting.Value;
                    result = CommonHelper.To<T>(settingValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all settings
        /// </summary>
        /// <returns>Setting collection</returns>
        public IDictionary<string, Setting> GetAllSettings()
        {
            //cache
            var key = string.Format(this.CachePatternAllKey);

            IDictionary<string, Setting> settingsDictionary;

            this._cacheManager.Get(key,
                () =>
                {
                    var query = this._repository.Select().OrderBy(f => f.Name);
                    var settings = query.ToDictionary(s => s.Name, StringComparer.InvariantCultureIgnoreCase);
                    return settings;
                },
                out settingsDictionary);

            return settingsDictionary;
        }

        /// <summary>
        /// Set setting value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="isEncrypted"></param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        public void SetSetting<T>(string key, T value, bool isEncrypted, bool clearCache = true)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            key = key.Trim().ToLowerInvariant();

            var settings = this.GetAllSettings();

            Setting setting = null;
            string valueStr = CommonHelper.GetCustomTypeConverter(typeof(T)).ConvertToInvariantString(value);

            var settingValue = isEncrypted ? this._encryptionService.EncryptText(valueStr) : valueStr;

            if (settings.ContainsKey(key))
            {
                setting = settings[key];

                // Attach the cached entity to the context
                this._repository.Attach(setting);

                setting.Value = settingValue;
                this.Update(setting);
            }
            else
            {
                // Insert
                setting = new Setting()
                {
                    Name = key,
                    Value = settingValue
                };

                this.Insert(setting);
            }

            if (clearCache)
                this.ClearCache();
        }

        /// <summary>
        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="settingInstance">Setting instance</param>
        public void SaveSettings<T>(T settingInstance) where T : class, new()
        {
            var configurationProvider = this._engine.Get<IConfigurationProvider<T>>();
            configurationProvider.SaveSettings(settingInstance);
        }

    }
}