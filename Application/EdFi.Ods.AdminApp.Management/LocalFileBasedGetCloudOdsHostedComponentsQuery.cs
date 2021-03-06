﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Hosting;

namespace EdFi.Ods.AdminApp.Management
{
    public class LocalFileBasedGetCloudOdsHostedComponentsQuery : IGetCloudOdsHostedComponentsQuery
    {
        public Task<IEnumerable<CloudOdsWebsite>> Execute(ICloudOdsOperationContext context)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<OdsComponent>> Execute(CloudOdsInstance instance)
        {
            var fileName = ConfigurationManager.AppSettings["HostedComponentsFile"];
            var filePath = HostingEnvironment.MapPath(fileName);

            if (filePath == null)
                throw new FileNotFoundException("Can't locate HostedComponents file");

            using (var sr = File.OpenText(filePath))
            {
                var componentsString = await sr.ReadToEndAsync();
                var odsComponents = Json.Decode<List<OdsComponent>>(componentsString);

                return odsComponents;
            }
        }
    }
}