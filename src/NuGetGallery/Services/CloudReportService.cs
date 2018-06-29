// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using NuGetGallery.Configuration;

namespace NuGetGallery
{
    public class CloudReportService<TContainer> : IReportService<TContainer>, ICloudStorageStatusDependency
        where TContainer : IReportContainer, new()
    {
        private readonly string _connectionString;
        private readonly bool _readAccessGeoRedundant;
        private readonly CloudBlobContainer _cloudContainer;

        public CloudReportService(IAppConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _connectionString = configuration.AzureStorage_Statistics_ConnectionString;
            _readAccessGeoRedundant = configuration.AzureStorageReadAccessGeoRedundant;
            _cloudContainer = GetCloudBlobContainer();
        }

        private CloudBlobContainer GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (_readAccessGeoRedundant)
            {
                blobClient.DefaultRequestOptions.LocationMode = LocationMode.PrimaryThenSecondary;
            }

            string containerName = new TContainer().Name;
            return blobClient.GetContainerReference(containerName);
        }

        public Task<bool> IsAvailableAsync() => _cloudContainer.ExistsAsync();

        public async Task<ReportBlob> Load(string reportName)
        {
            // In NuGet we always use lowercase names for all blobs in Azure Storage
            reportName = reportName.ToLowerInvariant();
            var blob = _cloudContainer.GetBlockBlobReference(reportName);

            // Check if the report blob is present before processing it.
            if (!blob.Exists())
            {
                throw new ReportNotFoundException();
            }

            await blob.FetchAttributesAsync();
            string content = await blob.DownloadTextAsync();

            return new ReportBlob(content, blob.Properties.LastModified?.UtcDateTime);
        }
    }
}