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
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace PAI.FRATIS.Data
{
    /// <summary>
    /// EF eager loading property includer
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DbIncluder : IIncluder
    {
        IQueryable<T> IIncluder.Include<T, TProperty>(IQueryable<T> source, Expression<Func<T, TProperty>> path)
        {
            return source.Include(path);
        }

        public IQueryable<TEntity> Include<TEntity>(IQueryable<TEntity> source, string path) where TEntity : class
        {
            return source.Include(path);
        }
    }
}