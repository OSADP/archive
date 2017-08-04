//===============================================================================
// Microsoft patterns & practices
// Windows Azure Architecture Guide
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// This code released under the terms of the 
// Microsoft patterns & practices license (http://wag.codeplex.com/license)
//===============================================================================


using System;

namespace IDTO.Common.Storage
{
    public interface IAzureBlobContainer<T>
    {
        void EnsureExist();
        void Save(string objId, T obj);
        T Get(string objId);
        Uri GetUri(string objId);
        void Delete(string objId);
        void DeleteContainer();
    }
}