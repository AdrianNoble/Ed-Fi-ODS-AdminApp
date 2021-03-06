﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Configuration;
using EdFi.Admin.DataAccess.Contexts;
using NUnit.Framework;

namespace EdFi.Ods.AdminApp.Management.Tests
{
    [TestFixture]
    public abstract class AdminDataTestBase : DataTestBase<SqlServerUsersContext>
    {
        protected override string ConnectionString => ConfigurationManager.ConnectionStrings["EdFi_Admin"].ConnectionString;
    }
}