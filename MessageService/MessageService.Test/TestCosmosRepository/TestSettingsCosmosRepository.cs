using System;
using System.Collections.Generic;
using Autofac;
using MessageService.CosmosRepository.Interface;
using MessageService.Models.CosmosModel;
using MessageService.Models.Enum;
using MessageService.Models.SMSModels;
using MessageService.Service.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageService.Test.TestCosmosRepository
{
    [TestClass]
    public class TestSettingsCosmosRepository
    {
        private readonly IContainer _container;

        public TestSettingsCosmosRepository()
        {
            _container = new ContainerResolver().Container;
        }

        [TestMethod]
        public void AddOrUpdateSMSVendorSettings_IsTrue()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            SMSVendorSettingsDocumentModel model = new SMSVendorSettingsDocumentModel()
            {
                AccountId = 2700,
                AppId = "Test",
                AppKey = "Test",
                Id = Guid.NewGuid().ToString(),
                SignatureText = "Test",
                TemplateId = "Test",
                Type = CosmosDocumentType.VendorSetting,
                UnSubscribeText = "T,TD,F",
                VendorId = (short)VendorType.Submail,
                VerificationTemplateId = "Test"
            };
            var result = con.AddOrUpdateSMSVendorSettingsAsync(model).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CheckSMSVendorSettingsExists()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var result = con.CheckSMSVendorSettingsExistsAsync(2700).Result;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetAndSetSMSSFInteractions_Test()
        {
            long accountId = 2700;
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var result = con.GetSMSSFInteractionsAsync(accountId).Result;
            Assert.IsNotNull(result);
            result.Add(new SMSSFInteractionModel() {
                AccountId = accountId,
                Id = "Test"
            });
            var inserted = con.AddOrUpdateSMSSFInteractionsAsync(result, accountId).Result;
            Assert.IsTrue(inserted);
        }

        [TestMethod]
        public void IsSMSVendorSettingsValid()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            SMSVendorSettingsDocumentModel model = new SMSVendorSettingsDocumentModel()
            {
                AccountId = 482,
                AppId = "Test",
                AppKey = "Test",
                Id = Guid.NewGuid().ToString(),
                SignatureText = "Test",
                TemplateId = "Test",
                Type = CosmosDocumentType.VendorSetting,
                UnSubscribeText = "T,TD,F",
                VendorId = (short)VendorType.Submail,
                VerificationTemplateId = "Test"
            };
            var result = con.IsSMSVendorSettingsValidAsync(model).Result;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetSMSVendorSettingsByAppId()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var result = con.GetSMSVendorSettingsAsync("Test").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetSMSVendorSettingsByAccountId()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var result = con.GetSMSVendorSettingsAsync(481).Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetCategories()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var result = con.GetCategoriesAsync().Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AddCategory()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            CategoryDocumentModel model = new CategoryDocumentModel()
            {
                Type = CosmosDocumentType.TopicSubscriptionCategory,
                CategoryName = "Category 2",
                Id = Guid.NewGuid().ToString()
            };
            var isValid = con.IsCategoryValidAsync(model).Result;
            if (isValid)
            {
                Assert.IsTrue(con.AddCategoryAsync(model).Result);
            }
            else
            {
                // category already exists              
            }
        }


        [TestMethod]
        public void AddInventoryAlerts_RecordShouldInsertIntoDocument_true()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();
            var doc = new InventoryAndAlertSettingDocumentModel()
            {
                AccountId = 482,
                AlertThreshold = 1000,
                NotificationUsers = new List<EmailNotificationUserDocumentModel>()
                {
                    new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser1"
                    },
                     new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser2"
                    },
                      new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser3"
                    },
                    new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser4"
                    }
                }
            };

            var result = con.AddOrUpdateInventoryAndAlertSettingAsync(doc).Result;

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void AddUsersInInventory_InsertIntoDocument_true()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();

            var accountId = 482;

            var NotificationUsers = new List<EmailNotificationUserDocumentModel>()
                {
                    new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser4"
                    },
                     new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = Guid.NewGuid().ToString(),
                        UserName = "TestUser5"
                    }
            };

            var result = con.AddUserToInventoryAndAlertSettingAsync(accountId,NotificationUsers).Result;

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void RemoveUsersInInventory_InsertIntoDocument_true()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();

            var accountId = 482;

            var NotificationUsers = new List<EmailNotificationUserDocumentModel>()
                {
                    new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = "33ff6857-0f44-4f40-a9cc-0101804fbdc9",
                        UserName = "TestUser1"
                    },
                     new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "Test@gmail.com",
                        Role="Admin",
                        UserId = "40d442c8-d533-4e13-832a-6a968874dc73",
                        UserName = "TestUser2"
                    }
            };

            var result = con.RemoveUserFromInventoryAndAlertSettingAsync(accountId, NotificationUsers).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetInInventory_FetchAlltheDocument_true()
        {
            var con = _container.Resolve<ISettingsCosmosRepository>();

            var accountId = 482;

            var result = con.GetInventoryAndAlertSettingAsync(accountId).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetUserForAdd_FetchAlltheUser_true()
        {
            var con = _container.Resolve<ISettingsService>();

            var accountId = 482;

            var result = con.GetUserToAddInventoryAndAlertSettingAsync(accountId).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Add_RequstTopUpForUser_true()
        {
            var con = _container.Resolve<ISettingsService>();

            var accountId = 482;

            var model = new TopUpRequestDocumentModel()
            {
                AccountId = accountId,
                RaisedByUserEmailId = "Test@gmail.com",
                RaisedByUserName = "Test",
                RequestedToUsers = new List<EmailNotificationUserDocumentModel>()
                {
                    new EmailNotificationUserDocumentModel()
                    {
                        EmailId = "karunakaran@tmgworldwide.com",
                        UserName = "Karuna",
                        Role = "Admin",
                        UserId = Guid.NewGuid().ToString()
                    }
                }
            };

            var result = con.AddTopUpRequestAsync(model).Result;

            Assert.IsNotNull(result);
        }


    }

}
