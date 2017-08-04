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
using System.Reflection;
using PAI.FRATIS.SFL.Common.Infrastructure;

namespace PAI.FRATIS.SFL.Services.Configuration
{
    public class ConfigurationProvider<TSettings> : IConfigurationProvider<TSettings> where TSettings : class, new()
    {
        private readonly ISettingService _settingService;

        public ConfigurationProvider(ISettingService settingService)
        {
            _settingService = settingService;
            BuildConfiguration();
        }

        public TSettings Settings { get; protected set; }

        public bool IsPropertyEncrypted(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(EncryptedSettingAttribute), false).FirstOrDefault() != null;
        }

        public void BuildConfiguration()
        {
            Settings = Activator.CreateInstance<TSettings>();

            // get properties we can write to
            var properties = from prop in typeof(TSettings).GetProperties()
                             where prop.CanWrite && prop.CanRead
                             let setting = _settingService.GetSettingByKey<string>(typeof(TSettings).Name + "." + prop.Name, IsPropertyEncrypted(prop))
                             where setting != null
                             where CommonHelper.GetCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string))
                             where CommonHelper.GetCustomTypeConverter(prop.PropertyType).IsValid(setting)
                             let value = CommonHelper.GetCustomTypeConverter(prop.PropertyType).ConvertFromInvariantString(setting)
                             select new { prop, value };


            foreach (var property in properties)
            {
                property.prop.SetValue(Settings, property.value, null);
            }
          
        }

        public void SaveSettings(TSettings settings)
        {
            var properties = from prop in typeof(TSettings).GetProperties()
                             where prop.CanWrite && prop.CanRead
                             where CommonHelper.GetCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string))
                             select prop;
            
            foreach (var prop in properties)
            {
                var key = typeof(TSettings).Name + "." + prop.Name;
                dynamic value = prop.GetValue(settings, null);

                _settingService.SetSetting(key, value ?? "", IsPropertyEncrypted(prop), false);
            }

            // _settingService.ClearCache();

            Settings = settings;
        }
    }
}
