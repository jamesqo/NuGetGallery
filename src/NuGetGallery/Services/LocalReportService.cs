// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NuGetGallery
{
    public class LocalReportService<TContainer> : IReportService<TContainer>
        where TContainer : IReportContainer, new()
    {
        private const string RootFolder = "Reports";

        private readonly IFileStorageService _fileStorage;

        public LocalReportService(
            IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public async Task<ReportBlob> Load(string reportName)
        {
            string containerName = new TContainer().Name;
            string folderName = Path.Combine(RootFolder, containerName);

            using (var stream = await _fileStorage.GetFileAsync(folderName, fileName: reportName))
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