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

namespace PAI.FRATIS.SFL.Services.Core.Caching
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CacheExtensions
    {
        public static bool Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire, out T value)
        {
            return Get(cacheManager, key, 60, acquire, out value);
        }

        public static bool Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire, out T value) 
        {
            if (cacheManager.IsSet(key))
            {
                value = cacheManager.Get<T>(key);
                return true;
            }

            value = acquire();
            cacheManager.Set(key, value, cacheTime);
            return false;
        }
    }
}
