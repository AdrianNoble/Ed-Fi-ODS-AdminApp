// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using EdFi.Ods.AdminApp.Management.Database.Ods;
using EdFi.Admin.DataAccess.Contexts;
using EdFi.Ods.AdminApp.Management.Configuration.Claims;
using EdFi.Ods.AdminApp.Management.Database.Models;
using EdFi.Ods.AdminApp.Management.Database.Ods.Reports;
using EdFi.Ods.AdminApp.Management.Instances;
using EdFi.Ods.AdminApp.Management.OdsInstanceServices;
using EdFi.Ods.AdminApp.Management.Tests.User;
using EdFi.Ods.AdminApp.Management.Services;
using EdFi.Ods.AdminApp.Web.Infrastructure;
using EdFi.Ods.AdminApp.Web.Models.ViewModels.OdsInstances;
using Moq;
using NUnit.Framework;
using Shouldly;
using static EdFi.Ods.AdminApp.Management.Tests.TestingHelper;
using static EdFi.Ods.AdminApp.Management.Tests.Instance.InstanceTestSetup;

namespace EdFi.Ods.AdminApp.Management.Tests.Instance
{
    [TestFixture]
    public class RegisterOdsInstanceCommandTests: AdminAppDataTestBase
    {
        private Mock<IDatabaseValidationService> _databaseValidationService;
        private Mock<ICloudOdsAdminAppSettingsApiModeProvider> _apiModeProvider;
        private Mock<IDatabaseConnectionProvider> _connectionProvider;

        [SetUp]
        public void Init()
        {
            _databaseValidationService = new Mock<IDatabaseValidationService>();
            _databaseValidationService.Setup(x => x.IsValidDatabase(It.IsAny<int>(), It.IsAny<ApiMode>())).Returns(true);
            _apiModeProvider = new Mock<ICloudOdsAdminAppSettingsApiModeProvider>();
            _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);
            _connectionProvider =  new Mock<IDatabaseConnectionProvider>();
        }

        [Test]
        public async Task ShouldRegisterOdsInstance()
        {
            ResetOdsInstanceRegistrations();
            var instanceName = "TestInstance_23456";
            const string Description = "Test Description";
            var encryptedSecretConfigValue = "Encrypted string";

            using (var connection = GetDatabaseConnection(instanceName))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(23456, ApiMode.DistrictSpecific))
                    .Returns(connection);

                var odsInstanceFirstTimeSetupService = GetOdsInstanceFirstTimeSetupService(encryptedSecretConfigValue, instanceName);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 23456,
                    Description = Description
                };
                var testUsername = UserTestSetup.SetupUsers(1).Single().Id;
             
                var command = new RegisterOdsInstanceCommand(odsInstanceFirstTimeSetupService, _connectionProvider.Object);
                var newInstanceId = await command.Execute(newInstance, ApiMode.DistrictSpecific, testUsername, new CloudOdsClaimSet());

                var addedInstance = Query<OdsInstanceRegistration>(newInstanceId);
                var secretConfiguration =
                    SetupContext.SecretConfigurations.FirstOrDefault(x => x.OdsInstanceRegistrationId == newInstanceId);
                secretConfiguration.ShouldNotBeNull();
                secretConfiguration.EncryptedData.ShouldBe(encryptedSecretConfigValue);
                addedInstance.Name.ShouldBe(instanceName);
                addedInstance.Description.ShouldBe(newInstance.Description);
            }
        }

        private static SqlConnection GetDatabaseConnection(string instanceName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["EdFi_Ods_Empty"].ConnectionString;

            var sqlConnectionBuilder =
                new SqlConnectionStringBuilder(connectionString) {InitialCatalog = instanceName};

            var connection = new SqlConnection(sqlConnectionBuilder.ConnectionString);
            return connection;
        }

        private OdsInstanceFirstTimeSetupService GetOdsInstanceFirstTimeSetupService(string encryptedSecretConfigValue,
            string instanceName)
        {
            var mockStringEncryptorService = new Mock<IStringEncryptorService>();
            mockStringEncryptorService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(encryptedSecretConfigValue);
            var odsSecretConfigurationProvider = new OdsSecretConfigurationProvider(mockStringEncryptorService.Object);

            var mockFirstTimeSetupService = new Mock<IFirstTimeSetupService>();
            var mockReportViewsSetUp = new Mock<IReportViewsSetUp>();
            var mockUsersContext = new Mock<IUsersContext>();
            mockFirstTimeSetupService.Setup(x => x.CreateAdminAppInAdminDatabase(It.IsAny<string>(), instanceName,
                It.IsAny<string>(), ApiMode.DistrictSpecific)).ReturnsAsync(new ApplicationCreateResult());
            var odsInstanceFirstTimeSetupService = new OdsInstanceFirstTimeSetupService(odsSecretConfigurationProvider,
                mockFirstTimeSetupService.Object, mockUsersContext.Object, mockReportViewsSetUp.Object, SetupContext);
            return odsInstanceFirstTimeSetupService;
        }

        [Test]
        public void ShouldNotRegisterInstanceIfRequiredFieldsEmpty()
        {
            ResetOdsInstanceRegistrations();

            var newInstance = new RegisterOdsInstanceModel();

            _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);

            new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                .ShouldNotValidate(newInstance,
                    "'ODS Instance District / EdOrg Id' must not be empty.",
                    "'ODS Instance Description' must not be empty.");
        }

        [TestCase(null)]
        [TestCase(0)]
        [TestCase(-1)]
        public void ShouldBeInvalidDistrictOrEdOrgId(int invalidId)
        {
            using (var connection = GetDatabaseConnection("Test_Ods_" + invalidId))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(invalidId, ApiMode.DistrictSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = invalidId,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance,
                        "'ODS Instance District / EdOrg Id' must be a valid positive integer.");
            }
        }
      
        [TestCase(null)]
        [TestCase(3099)]
        [TestCase(1800)]
        public void ShouldBeInvalidSchoolYear(int? invalidSchoolYear)
        {
            using (var connection = GetDatabaseConnection("Test_Ods_" + invalidSchoolYear))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(invalidSchoolYear ?? 0, ApiMode.YearSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.YearSpecific);
                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = invalidSchoolYear,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance,
                        invalidSchoolYear == null
                        ? "'ODS Instance School Year' must not be empty."
                        : "'ODS Instance School Year' must be between 1900 and 2099.");
            }
        }

        [Test]
        public void ShouldNotRegisterInstanceIfOdsInstanceIdentifierNotUniqueOnYearSpecificMode()
        {
            var instanceName = "Test_Ods_2020";
            ResetOdsInstanceRegistrations();
            SetupOdsInstanceRegistration(instanceName);
            _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.YearSpecific);

            using (var connection = GetDatabaseConnection(instanceName))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(2020, ApiMode.YearSpecific))
                    .Returns(connection);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 2020,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance,
                        "An instance for this school year already exists.");
            }
        }

        [Test]
        public void ShouldNotRegisterInstanceIfOdsInstanceIdentifierNotUniqueOnDistrictSpecificMode()
        {
            var instanceName = "Test_Ods_8787877";
            ResetOdsInstanceRegistrations();
            SetupOdsInstanceRegistration(instanceName);

            using (var connection = GetDatabaseConnection(instanceName))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(8787877, ApiMode.DistrictSpecific))
                    .Returns(connection);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 8787877,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(
                        SetupContext, _apiModeProvider.Object, _databaseValidationService.Object,
                        _connectionProvider.Object)
                    .ShouldNotValidate(
                        newInstance,
                        "An instance for this Education Organization / District Id already exists.");
            }
        }

        [Test]
        public void ShouldNotRegisterInstanceIfOdsInstanceIdentifierAssociatedDbDoesNotExistsOnDistrictSpecificMode()
        {
            const int OdsInstanceNumericSuffix = 8787877;
            const string InstanceName = "Test_Ods_8787877";

            var mockDatabaseValidationService = new Mock<IDatabaseValidationService>();
            mockDatabaseValidationService.Setup(x => x.IsValidDatabase(OdsInstanceNumericSuffix, ApiMode.DistrictSpecific))
                .Returns(false);

            using (var connection = GetDatabaseConnection(InstanceName))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(OdsInstanceNumericSuffix, ApiMode.DistrictSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = OdsInstanceNumericSuffix,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, mockDatabaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance,
                        "Could not connect to an ODS instance database for this Education Organization / District Id.");
            }
        }

        [Test]
        public void ShouldNotRegisterInstanceIfOdsInstanceIdentifierAssociatedDbDoesNotExistsOnYearSpecificMode()
        {
            const int OdsInstanceNumericSuffix = 2020;
            const string InstanceName = "Test_Ods_2020";

            var mockDatabaseValidationService = new Mock<IDatabaseValidationService>();
            mockDatabaseValidationService.Setup(x => x.IsValidDatabase(OdsInstanceNumericSuffix, ApiMode.YearSpecific))
                .Returns(false);

            using (var connection = GetDatabaseConnection(InstanceName))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(OdsInstanceNumericSuffix, ApiMode.YearSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.YearSpecific);
                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = OdsInstanceNumericSuffix,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, mockDatabaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance,
                        "Could not connect to an ODS instance database for this school year.");
            }
        }

        [Test]
        public void ShouldNotRegisterInstanceIfOdsInstanceDescriptionNotUnique()
        {
            ResetOdsInstanceRegistrations();
            var instance = SetupOdsInstanceRegistration("Test_Ods_8787877");

            using (var connection = GetDatabaseConnection("Test_Ods_7878787"))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(7878787, ApiMode.DistrictSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 7878787,
                    Description = instance.Description
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldNotValidate(newInstance, "An instance with this description already exists.");
            }
        }

        [Test]
        public void ShouldValidateValidInstanceOnDistrictSpecificMode()
        {
            ResetOdsInstanceRegistrations();

            using (var connection = GetDatabaseConnection("TestInstance_7878787"))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(7878787, ApiMode.DistrictSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.DistrictSpecific);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 7878787,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldValidate(newInstance);
            }
        }

        [Test]
        public void ShouldValidateValidInstanceOnYearSpecificMode()
        {
            ResetOdsInstanceRegistrations();

            using (var connection = GetDatabaseConnection("TestInstance_2020"))
            {
                _connectionProvider.Setup(x => x.CreateNewConnection(2020, ApiMode.YearSpecific))
                    .Returns(connection);

                _apiModeProvider.Setup(x => x.GetApiMode()).Returns(ApiMode.YearSpecific);

                var newInstance = new RegisterOdsInstanceModel
                {
                    NumericSuffix = 2020,
                    Description = Sample("Description")
                };

                new RegisterOdsInstanceModelValidator(SetupContext, _apiModeProvider.Object, _databaseValidationService.Object, _connectionProvider.Object)
                    .ShouldValidate(newInstance);
            }
        }
    }
}
