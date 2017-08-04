// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace IDTO.Common.Storage
{
    public class AzureTable<T> : IAzureTable<T> where T : TableEntity, new()
    {
        private readonly string _tableName;
        private readonly CloudStorageAccount _account;

        public AzureTable()
            : this(CloudStorageAccount.DevelopmentStorageAccount)
        {
        }

        public AzureTable(CloudStorageAccount account)
            : this(account, typeof(T).Name)
        {
        }

        public AzureTable(CloudStorageAccount account, string tableName)
        {
            this._tableName = tableName;
            this._account = account;
        }

        public TableQuery<T> Query
        {
            get
            {
                CloudTableClient tableClient = this._account.CreateCloudTableClient();
                CloudTable snapshotTable = tableClient.GetTableReference(this._tableName);
                return snapshotTable.CreateQuery<T>();
            }
        }

        public bool CreateIfNotExist()
        {
            CloudTableClient tableClient = this._account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(this._tableName);
            return table.CreateIfNotExists();
        }

        public bool DeleteIfExist()
        {
            CloudTableClient tableClient = this._account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(this._tableName);
            return table.DeleteIfExists();
        }

        public void AddEntity(T obj)
        {
            this.AddEntity(new[] { obj });
        }

        public void AddEntity(IEnumerable<T> objs)
        {
            // Create the table client.
            CloudTableClient tableClient = this._account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(this._tableName);

            // Define a batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            foreach (var obj in objs)
            {
                batchOperation.Insert(obj);
            }

            table.ExecuteBatch(batchOperation);
        }

        public void AddOrUpdateEntity(T obj)
        {
            this.AddOrUpdateEntity(new[] { obj });
        }

        public void AddOrUpdateEntity(IEnumerable<T> objs)
        {
            foreach (var obj in objs)
            {
                var pk = obj.PartitionKey;
                var rk = obj.RowKey;
                T existingObj = null;

                try
                {
                    existingObj = (from o in this.Query
                                   where o.PartitionKey == pk && o.RowKey == rk
                                   select o).SingleOrDefault();
                }
                catch
                {
                }

                if (existingObj == null)
                {
                    this.AddEntity(obj);
                }
                else
                {
                    //TODO: Make this work with Azure Storage 3
                    //TableServiceContext context = this.CreateContext();
                    //context.AttachTo(this._tableName, obj, "*");
                    //context.UpdateObject(obj);
                    //context.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
                }
            }
        }

        public void DeleteEntity(T obj)
        {
            this.DeleteEntity(new[] { obj });
        }

        public void DeleteEntity(IEnumerable<T> objs)
        {
            //TODO: Make this work with Azure Storage 3 (or see if it is still needed)
            //TableServiceContext context = this.CreateContext();
            //foreach (var obj in objs)
            //{
            //    context.AttachTo(this._tableName, obj, "*");
            //    context.DeleteObject(obj);
            //}

            //try
            //{
            //    context.SaveChanges();
            //}
            //catch (DataServiceRequestException ex)
            //{
            //    var dataServiceClientException = ex.InnerException as DataServiceClientException;
            //    if (dataServiceClientException != null)
            //    {
            //        if (dataServiceClientException.StatusCode == 404)
            //        {
            //            return;
            //        }
            //    }

            //    throw;
            //}
        }

        //private TableServiceContext CreateContext()
        //{
        //    var context = new TableServiceContext(this._account.TableEndpoint.ToString(), this._account.Credentials)
        //    {
        //        ResolveType = t => typeof(T),
        //        RetryPolicy = RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount, RetryPolicies.DefaultClientBackoff)
        //    };

        //    return context;
        //}

        private class PartitionKeyComparer : IEqualityComparer<TableEntity>
        {
            public bool Equals(TableEntity x, TableEntity y)
            {
                return string.Compare(x.PartitionKey, y.PartitionKey, true, CultureInfo.InvariantCulture) == 0;
            }

            public int GetHashCode(TableEntity obj)
            {
                return obj.PartitionKey.GetHashCode();
            }
        }
    }
}