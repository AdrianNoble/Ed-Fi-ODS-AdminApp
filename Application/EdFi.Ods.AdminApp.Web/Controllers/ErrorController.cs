﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Web.Mvc;
using EdFi.Ods.AdminApp.Web.ActionFilters;

namespace EdFi.Ods.AdminApp.Web.Controllers
{
    [BypassSetupRequiredFilter, BypassInstanceContextFilter]
    public class ErrorController : ControllerBase
    {
        public ActionResult NotYetImplemented()
        {
            return View();
        }

        public ActionResult MultiInstanceError()
        {
            return View();
        }
    }
}