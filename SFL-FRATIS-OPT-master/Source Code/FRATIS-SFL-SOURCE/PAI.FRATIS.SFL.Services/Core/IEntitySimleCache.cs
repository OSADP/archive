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

using System.Collections.Generic;

namespace PAI.FRATIS.SFL.Services.Core
{
    public interface IEntitySimpleCache<T> where T : Domain.EntityBase
    {
        ICollection<T> Get();

        ICollection<T> Get(IEnumerable<int> ids);

        void ClearCache();

        void Clear(int id);

        T Get(int id);
    }

    /// <summary>The entity simple cache base.</summary>
    /// <typeparam name="T"></typeparam>
    public class EntitySimpleCacheBase<T> : IEntitySimpleCache<T> where T : Domain.EntityBase
    {
        /// <summary>The dictionary object for cache backing.</summary>
        private readonly Dictionary<int, T> _dictionary = new Dictionary<int, T>();

        /// <summary>Gets all objects from the cache.</summary>
        /// <returns>The <see cref="ICollection"/>.</returns>
        public ICollection<T> Get()
        {
            return _dictionary.Values;
        }

        public ICollection<T> Get(IEnumerable<int> ids)
        {
            var result = new List<T>();
            foreach (var id in ids)
            {
                T entity = Get(id);
                if (entity != null)
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        public void ClearCache()
        {
            _dictionary.Clear();
        }

        public void Clear(int id)
        {
            _dictionary[id] = null;
        }

        public T Get(int id)
        {
            T result = null;
            _dictionary.TryGetValue(id, out result);
            return result;
        }
    }

}
