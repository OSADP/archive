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
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using PAI.FRATIS.Data;
using PAI.FRATIS.SFL.Common;
using PAI.FRATIS.SFL.Infrastructure;

namespace PAI.FRATIS.SFL.Data
{
    /// <summary>
    /// Represents the DataContext
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataContext : DbContext, IDbContext
    {
        public DataContext()
        {
            Database.Connection.ConnectionString = ConnectionStringManager.ConnectionString;
        }

        public void SetConnectionString(string connectionString)
        {
            Database.Connection.ConnectionString = connectionString;
        }

        public DataContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Dynamically load all configurations
            var configType = typeof(DataContext);
            var typesToRegister = Assembly.GetAssembly(configType).GetTypes()
                .Where(type => !string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.BaseType != null
                    && type.BaseType.IsGenericType
                    && (type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>)));

            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }

            // Remove undesired conventions
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public void Detach<TEntity>(TEntity entity) where TEntity : class
        {
             if (entity != null)
             {
                 Entry(entity).State = EntityState.Detached;
             }  
        }

        public void MarkDirty<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                Entry(entity).State = EntityState.Modified;
            }
            catch
            {
                // log error
            }
            

        }

        public TEntity Attach<TEntity>(TEntity entity, bool markDirty = false) where TEntity : class
        {
            try
            {
                if (entity != null)
                {
                    if (Entry(entity).State == EntityState.Detached)
                    {
                        try
                        {
                            Set<TEntity>().Attach(entity);
                        }
                        finally
                        {
                            if (markDirty) Entry(entity).State = EntityState.Modified;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log failure
                // DEBUG ajh
                ;
            }
            return entity;
        }

    }
}
