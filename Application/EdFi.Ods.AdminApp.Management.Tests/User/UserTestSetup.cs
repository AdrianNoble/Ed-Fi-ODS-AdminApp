﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EdFi.Ods.AdminApp.Management.Database;
using EdFi.Ods.AdminApp.Management.Database.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EdFi.Ods.AdminApp.Management.Tests.User
{
    public static class UserTestSetup
    {
        public static void ResetUsers()
        {
            using (var dbContext = new AdminAppDbContext())
            {
                dbContext.Database.ExecuteSqlCommand("DELETE FROM [adminapp].[Users]");
            }
        }

        public static AdminAppUserContext GetMockUserContext(AdminAppUser user = null, Role userRole = null)
        {
            var userContext = new AdminAppUserContext();

            if (user != null)
                userContext.User = user;
            if(userRole != null)
                userContext.Permissions = RolePermission.GetPermissions(userRole.Value.ToString());

            return userContext;
        }

        public static IEnumerable<AdminAppUser> SetupUsers(int userCount = 5, Role userRole = null)
        {
            var testUsers = Enumerable.Range(1, userCount)
                .Select(x => new AdminAppUser
                {
                    Email = $"testuser{Guid.NewGuid():N}@test.com",
                    UserName = $"testuser{Guid.NewGuid():N}@test.com"
                })
                .ToList();

            SaveAdminAppUser(testUsers);

            if(userRole != null)
                SaveUserRoles(testUsers, userRole);

            return testUsers;
        }

        public static void SaveAdminAppUser(AdminAppUser user)
        {
            using (var database = AdminAppIdentityDbContext.Create())
            {
                database.Set<AdminAppUser>().Add(user);
                database.SaveChanges();
            }
        }

        public static void SaveAdminAppUser(List<AdminAppUser> users)
        {
            using (var database = AdminAppIdentityDbContext.Create())
            {
                database.Set<AdminAppUser>().AddRange(users);
                database.SaveChanges();
            }
        }

        public static void EnsureDefaultRoles()
        {
            
            using (var database = AdminAppIdentityDbContext.Create())
            {
                var superAdminRole = database.Roles.SingleOrDefault(x => x.Id.Equals(Role.SuperAdmin.Value.ToString()));
                var adminRole = database.Roles.SingleOrDefault(x => x.Id.Equals(Role.Admin.Value.ToString()));
                var missingRolesToAdd = new List<IdentityRole>();
                if (superAdminRole == null)
                {
                    missingRolesToAdd.Add(new IdentityRole
                    {
                        Id = Role.SuperAdmin.Value.ToString(),
                        Name = Role.SuperAdmin.DisplayName
                    });
                }
                if (adminRole == null)
                {
                    missingRolesToAdd.Add(new IdentityRole
                    {
                        Id = Role.Admin.Value.ToString(),
                        Name = Role.Admin.DisplayName
                    });
                }
                database.Set<IdentityRole>().AddRange(missingRolesToAdd);
                database.SaveChanges();
            }
        }

        public static void SaveUserRoles(List<AdminAppUser> users, Role userRole)
        {
            var userRoleRecords = new List<IdentityUserRole>();
            foreach (var user in users)
            {
                userRoleRecords.Add(new IdentityUserRole()
                {
                    UserId = user.Id,
                    RoleId = userRole.Value.ToString()
                });
            }
            using (var database = AdminAppIdentityDbContext.Create())
            {
                database.Set<IdentityUserRole>().AddRange(userRoleRecords);
                database.SaveChanges();
            }
        }

        public static TResult Query<TResult>(Func<DbContext, TResult> query)
        {
            TResult result;

            using (var database = AdminAppIdentityDbContext.Create())
            {
                result = query(database);
            }

            return result;
        }

        public static AdminAppUser Query(string id)
        {
            return Query(database => database.Set<AdminAppUser>().Find(id));
        }

    }
}