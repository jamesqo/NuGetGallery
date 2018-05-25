﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NuGetGallery
{
    public class LocalReportService : IReportService
    {
        private const string FolderName = "Reports";

        private readonly IFileStorageService _fileStorage;

        public LocalReportService(
            IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public Task<IReportContainer> GetContainer(string containerName)
        {
            return Task.FromResult<IReportContainer>(new Container(this, containerName));
        }

        private class Container : IReportContainer
        {
            private readonly LocalReportService _parent;
            private readonly string _containerName;

            public Container(LocalReportService parent, string containerName)
            {
                _parent = parent;
                _containerName = containerName;
            }

            public Task<bool> IsAvailableAsync()
            {
                throw new NotImplementedException();
            }

            public async Task<ReportBlob> Load(string reportName)
            {
                using (var stream = await _parent._fileStorage.GetFileAsync(FolderName, fileName: reportName))
                {
                    if (stream == null)
                    {
                        throw new ReportNotFoundException();
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        string content = await reader.ReadToEndAsync();
                        return new ReportBlob(content);
                    }
                }
            }
        }
    }
}