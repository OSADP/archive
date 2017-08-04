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
using System.Collections.Generic;
using System.Linq;

namespace PAI.FRATIS.SFL.Infrastructure
{
    public static class Extensions
    {
        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

        public static string ToDescriptiveString(this Enum value)
        {
            return CommonHelper.ConvertEnum(value);
        }

        public static IEnumerable<string> SplitAndTrim(this string value, char delimiter)
        {
            return value.Split(delimiter)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v));
        }

        public static TValue TryGetOrSetValue<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue value)
        {
            TValue result;
            if (!d.TryGetValue(key, out result))
            {
                d[key] = value;
            }
            return result;
        }
    }
}
