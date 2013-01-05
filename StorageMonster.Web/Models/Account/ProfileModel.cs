﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StorageMonster.Web.Properties;
using StorageMonster.Web.Services.Validation;

namespace StorageMonster.Web.Models.Account
{
#warning add version
    public class ProfileModel
    {
        private String _userName;

        [Display(Name = "ProfileEmail", ResourceType = typeof(DisplayNameResources))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "RequiredFieldFormat", ErrorMessageResourceType = typeof(ValidationResources))]
        [Display(Name = "ProfileUserName", ResourceType = typeof(DisplayNameResources))]
        [UserNameLength(ErrorMessageResourceName = "StringLengthLessThanFormat", ErrorMessageResourceType = typeof(ValidationResources))]
        [UserNameRegex(ErrorMessageResourceName = "InvalidFieldFormat", ErrorMessageResourceType = typeof(ValidationResources))]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value != null ? value.Trim() : null; }
        }

        [Required(ErrorMessageResourceName = "RequiredFieldFormat", ErrorMessageResourceType = typeof(ValidationResources))]
        [Display(Name = "ProfileLocale", ResourceType = typeof(DisplayNameResources))]
        public string Locale { get; set; }

        //[Required(ErrorMessageResourceName = "StampRequired", ErrorMessageResourceType = typeof(ValidationResources))]
        //public long Stamp { get; set; }

        [Required(ErrorMessageResourceName = "RequiredFieldFormat", ErrorMessageResourceType = typeof(ValidationResources))]
        [Display(Name = "ProfileTimeZone", ResourceType = typeof(DisplayNameResources))]
        public int TimeZone { get; set; }

        public IEnumerable<SelectListItem> SupportedLocales { get; set; }
        public IEnumerable<SelectListItem> SupportedTimeZones { get; set; }

        public void Init(IEnumerable<SelectListItem> supportedLocales, IEnumerable<SelectListItem> supportedTimeZones)
        {
            SupportedLocales = supportedLocales;
            SupportedTimeZones = supportedTimeZones;
        }
    }
}