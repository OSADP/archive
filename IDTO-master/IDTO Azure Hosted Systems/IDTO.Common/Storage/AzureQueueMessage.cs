//===============================================================================
// Microsoft patterns & practices
// Windows Azure Architecture Guide
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// This code released under the terms of the 
// Microsoft patterns & practices license (http://wag.codeplex.com/license)
//===============================================================================


namespace IDTO.Common.Storage
{
    public abstract class AzureQueueMessage
    {
        public string Id { get; set; }
        public string PopReceipt { get; set; }
        public int DequeueCount { get; set; }
    }
}