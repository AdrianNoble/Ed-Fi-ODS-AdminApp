﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace EdFi.Ods.AdminApp.Management
{
    public class CloudOdsEnvironment : Enumeration<CloudOdsEnvironment>
    {
        public static readonly CloudOdsEnvironment Production = new CloudOdsEnvironment(1, "Production");

        private CloudOdsEnvironment(int value, string displayName) : base(value, displayName)
        {
        }
    }
}