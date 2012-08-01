﻿using System.Web.Security;
using System;
using StorageMonster.Domain;

namespace StorageMonster.Web.Services.Security
{
    public interface IMembershipService
    {
        int MinPasswordLength { get; }
        bool ValidateUser(string email, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string userEmail, string locale);
        User UpdateUser(int userId, string userName, string locale, DateTime stamp);
    }
}