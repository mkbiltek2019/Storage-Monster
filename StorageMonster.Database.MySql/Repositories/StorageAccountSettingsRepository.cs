﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StorageMonster.Database.Repositories;
using StorageMonster.Domain;
using System.Transactions;

namespace StorageMonster.Database.MySql.Repositories
{
    public class StorageAccountSettingsRepository : IStorageAccountSettingsRepository
    {
        protected IConnectionProvider ConnectionProvider { get; set; }

        protected const string SelectFieldList = "acs.id AS Id, acs.storage_account_id AS StorageAccountId, acs.setting_name AS SettingName, acs.setting_value AS SettingValue,  acs.stamp AS Stamp";
        protected const string InsertFieldList = "(storage_account_id, setting_name, setting_value) VALUES(@StorageAccountId, @SettingName, @SettingValue)";
        protected const string UpdateFieldList = "storage_account_id=@StorageAccountId, setting_name=@SettingName, setting_value=@SettingValue";
        protected const string TableName = "storage_accounts_settings";

        public StorageAccountSettingsRepository(IConnectionProvider connectionprovider)
        {
            ConnectionProvider = connectionprovider;
        }

        public StorageAccountSetting LoadByName(string name, int accountId)
        {
            return SqlQueryExecutor.Execute(() =>
            {
                String query = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} acs WHERE acs.setting_name = @SettingName AND acs.storage_account_id = @StorageAccountId LIMIT 1;", SelectFieldList, TableName);
                return ConnectionProvider.CurrentConnection.Query<StorageAccountSetting>(query, new { SettingName = name, StorageAccountId = accountId }).FirstOrDefault();
            });
        }

        public StorageAccountSetting Update(StorageAccountSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");


            return SqlQueryExecutor.Execute(() =>
            {
                String query = string.Format(CultureInfo.InvariantCulture, "UPDATE {1} SET {0} WHERE Id=@Id", UpdateFieldList, TableName);
                ConnectionProvider.CurrentConnection.Execute(query, new { setting.Id, setting.StorageAccountId, setting.SettingName, setting.SettingValue });

                String idAndStampQuery = string.Format(CultureInfo.InvariantCulture, "SELECT id AS Id, stamp AS Stamp FROM {0} WHERE id=@Id;", TableName);
                IdAndStamp idAndStamp = ConnectionProvider.CurrentConnection.Query<IdAndStamp>(idAndStampQuery, new { setting.Id }).FirstOrDefault();

                if (idAndStamp == null)
                    throw new MonsterDbException("Account setting update failed");

                setting.Id = idAndStamp.Id;
                setting.Stamp = idAndStamp.Stamp;
                return setting;
            });
        }

        public StorageAccountSetting Create(StorageAccountSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            return SqlQueryExecutor.Execute(() =>
            {
                String query = string.Format(CultureInfo.InvariantCulture, "INSERT INTO {1} {0}; SELECT LAST_INSERT_ID();", InsertFieldList, TableName);
                int insertedId = (int)ConnectionProvider.CurrentConnection.Query<long>(query, new { setting.StorageAccountId, setting.SettingName, setting.SettingValue }).FirstOrDefault();
                if (insertedId <= 0)
                    throw new MonsterDbException("Account setting insertion failed");

                String idAndStampQuery = string.Format(CultureInfo.InvariantCulture, "SELECT id AS Id, stamp AS Stamp FROM {0} WHERE id=@Id;", TableName);
                IdAndStamp idAndStamp = ConnectionProvider.CurrentConnection.Query<IdAndStamp>(idAndStampQuery, new { Id = insertedId }).FirstOrDefault();

                if (idAndStamp == null)
                    throw new MonsterDbException("Account insertion failed");

                setting.Id = idAndStamp.Id;
                setting.Stamp = idAndStamp.Stamp;
                return setting;
            });
        }

        public IEnumerable<StorageAccountSetting> GetSettingsForStoargeAccount(int storageAccountId)
        {
            return SqlQueryExecutor.Execute(() =>
            {
                String query = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} acs WHERE acs.storage_account_id = @StorageAccountId;", SelectFieldList, TableName);
                return ConnectionProvider.CurrentConnection.Query<StorageAccountSetting>(query, new { StorageAccountId = storageAccountId });
            });
        }

        public UpdateResult SaveSettings(IDictionary<string, string> storageAccountSettingsList, int storageAccountId, DateTime storageAccountStamp)
        {
            if (storageAccountSettingsList == null)
                throw new ArgumentNullException("accountSettingsList");

            if (storageAccountSettingsList.Count <= 0)
                return UpdateResult.Success;


            return SqlQueryExecutor.Execute(() =>
            {
                using (var scope = new TransactionScope())
                {
                    const string selectAccountWithStampQuery = "SELECT a.id FROM storage_accounts a WHERE a.id = @Id AND a.stamp = @Stamp LIMIT 1;";

                    int? accountIdCheck = ConnectionProvider.CurrentConnection.Query<int?>(selectAccountWithStampQuery, new { Id = storageAccountId, Stamp = storageAccountStamp }).FirstOrDefault();

                    if (accountIdCheck == null)
                    {
                        const string selectAccountQueryWithoutStamp = "SELECT a.id FROM storage_accounts a WHERE a.id = @Id LIMIT 1;";
                        accountIdCheck = ConnectionProvider.CurrentConnection.Query<int?>(selectAccountQueryWithoutStamp, new { Id = storageAccountId }).FirstOrDefault();
                        if (accountIdCheck == null)
                            return UpdateResult.ItemNotExists;
                        return UpdateResult.Stalled;
                    }

                    foreach (var accountSetting in storageAccountSettingsList)
                    {
                        StorageAccountSetting setting = LoadByName(accountSetting.Key, storageAccountId);
                        if (setting == null)
                        {
                            setting = new StorageAccountSetting
                                {
                                    StorageAccountId = storageAccountId,
                                    SettingName = accountSetting.Key,
                                    SettingValue = accountSetting.Value
                                };
                            Create(setting);
                        }
                        else
                        {
                            setting.SettingName = accountSetting.Key;
                            setting.SettingValue = accountSetting.Value;
                            Update(setting);
                        }
                    }

                    const string insertQuery = "UPDATE storage_accounts SET stamp = CURRENT_TIMESTAMP() WHERE id = @Id;";
                    ConnectionProvider.CurrentConnection.Execute(insertQuery, new { Id = storageAccountId });

                    scope.Complete();
                    return UpdateResult.Success;
                }
            });
        }
    }
}