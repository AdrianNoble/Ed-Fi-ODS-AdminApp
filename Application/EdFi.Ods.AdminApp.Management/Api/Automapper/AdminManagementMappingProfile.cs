﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using LocalEducationAgency = EdFi.Ods.AdminApp.Management.Api.Models.LocalEducationAgency;
using School = EdFi.Ods.AdminApp.Management.Api.Models.School;

namespace EdFi.Ods.AdminApp.Management.Api.Automapper
{
    public class AdminManagementMappingProfile : Profile
    {
        public AdminManagementMappingProfile()
        {
            CreateMap<DomainModels.EdFiSchool, School>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.NameOfInstitution))
                .ForMember(dst => dst.EducationOrganizationId, opt => opt.MapFrom(src => src.SchoolId))
                .ForMember(dst => dst.LocalEducationAgencyId, opt => opt.MapFrom(src => src.LocalEducationAgencyReference == null ? null : src.LocalEducationAgencyReference.LocalEducationAgencyId))
                .ForMember(dst => dst.StreetNumberName, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any()? src.Addresses.First().StreetNumberName : null))
                .ForMember(dst => dst.ApartmentRoomSuiteNumber, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().ApartmentRoomSuiteNumber : null))
                .ForMember(dst => dst.State, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().StateAbbreviationDescriptor : null))
                .ForMember(dst => dst.City, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().City : null))
                .ForMember(dst => dst.ZipCode, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().PostalCode : null))
                .ForMember(dst => dst.GradeLevels, opt => opt.MapFrom(src => src.GradeLevels != null  ? src.GradeLevels.Select(g => g.GradeLevelDescriptor) : new List<string>()))
                .ForMember(dst => dst.EducationOrganizationCategory, opt => opt.Ignore()); //this value is currently stored in web.config

            CreateMap<DomainModels.EdFiLocalEducationAgency, LocalEducationAgency>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.LocalEducationAgencyId, opt => opt.MapFrom(src => src.LocalEducationAgencyId))
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.NameOfInstitution))
                .ForMember(dst => dst.EducationOrganizationId, opt => opt.MapFrom(src => src.LocalEducationAgencyId ?? 0))
                .ForMember(dst => dst.StreetNumberName, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().StreetNumberName : null))
                .ForMember(dst => dst.ApartmentRoomSuiteNumber, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().ApartmentRoomSuiteNumber : null))
                .ForMember(dst => dst.State, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().StateAbbreviationDescriptor : null))
                .ForMember(dst => dst.City, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().City : null))
                .ForMember(dst => dst.ZipCode, opt => opt.MapFrom(src => src.Addresses != null && src.Addresses.Any() ? src.Addresses.First().PostalCode : null))
                .ForMember(dst => dst.LocalEducationAgencyCategoryType, opt => opt.MapFrom(src => src.LocalEducationAgencyCategoryDescriptor))
                .ForMember(dst => dst.EducationOrganizationCategory, opt => opt.Ignore()) //this value is currently stored in web.config
                .ForMember(dst => dst.StateOrganizationId, opt => opt.MapFrom(src => src.StateEducationAgencyReference != null ? src.StateEducationAgencyReference.StateEducationAgencyId : 0));

            CreateMap<School, DomainModels.EdFiSchool>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.SchoolId, opt => opt.MapFrom(src => src.EducationOrganizationId))
                .ForMember(dst => dst.NameOfInstitution, opt => opt.MapFrom(src => src.Name))
                .ForMember(dst => dst.Addresses, opt => opt.ResolveUsing<SchoolAddressResolver>())
                .ForMember(dst => dst.EducationOrganizationCategories,
                    opt => opt.ResolveUsing<SchoolCategoryResolver>())
                .ForMember(dst => dst.GradeLevels, opt => opt.ResolveUsing<SchoolGradeLevelResolver>())
                .ForMember(dst => dst.LocalEducationAgencyReference,
                    opt => opt.ResolveUsing<LocalEducationAgencyReferenceResolver>())
                .ForCtorParam("id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("schoolId", opt => opt.MapFrom(src => src.EducationOrganizationId))
                .ForCtorParam("nameOfInstitution", opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("addresses", opt => opt.ResolveUsing(EducationOrganizationAddressResolver.Resolve))
                .ForCtorParam("educationOrganizationCategories", opt => opt.ResolveUsing(EducationOrganizationCategoryResolver.Resolve))
                .ForCtorParam("gradeLevels", opt => opt.ResolveUsing(SchoolGradeLevelResolver.Resolve));

            CreateMap<LocalEducationAgency, DomainModels.EdFiLocalEducationAgency>()
                .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.LocalEducationAgencyId, opt => opt.MapFrom(src =>src.LocalEducationAgencyId))
                .ForMember(dst => dst.NameOfInstitution, opt => opt.MapFrom(src => src.Name))
                .ForMember(dst => dst.LocalEducationAgencyCategoryDescriptor, opt => opt.MapFrom(src => src.LocalEducationAgencyCategoryType))
                .ForMember(dst => dst.Addresses, opt => opt.ResolveUsing<LocalEducationAgencyAddressResolver>())
                .ForMember(dst => dst.StateEducationAgencyReference, opt => opt.ResolveUsing<StateEducationAgencyReferenceResolver>())
                .ForMember(dst => dst.Categories, opt => opt.ResolveUsing<LocalEducationAgencyCategoryResolver>())
                .ForCtorParam("id", opt => opt.MapFrom(src => src.Id))
                .ForCtorParam("addresses", opt => opt.ResolveUsing(EducationOrganizationAddressResolver.Resolve))
                .ForCtorParam("categories", opt => opt.ResolveUsing(EducationOrganizationCategoryResolver.Resolve))
                .ForCtorParam("localEducationAgencyId", opt => opt.MapFrom(src => src.LocalEducationAgencyId))
                .ForCtorParam("localEducationAgencyCategoryDescriptor", opt => opt.MapFrom(src => src.LocalEducationAgencyCategoryType))
                .ForCtorParam("nameOfInstitution", opt => opt.MapFrom(src => src.Name));
        }
    }
}
