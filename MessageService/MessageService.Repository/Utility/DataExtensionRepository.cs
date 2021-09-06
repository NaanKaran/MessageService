using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using FuelSDK;
using MessageService.InfraStructure.Helpers;
using MessageService.Models.AzureStorageModels;
using MessageService.Models.DataExtensionModel;

namespace MessageService.Repository.Utility
{
    public static class DataExtensionRepository
    {

        public static void CreateDataExtension(Type type, int folderId, string soapEndPointUrl, string oauthToken, string deName = null)
        {
            var soapClient = GetSoapClient(soapEndPointUrl);

            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);
                var de = GetDataExtensionCreateObject(type, folderId, deName);
               var obj = soapClient.Create(new CreateOptions(), new APIObject[] { de }, out var cRequestId,out var cStatus);
               foreach (CreateResult cr in obj)
               {
                   Console.WriteLine("Status Message: " + cr.StatusMessage);
               }
            }
        }

        public static void CreateDataExtension<T>(T obj, int folderId, string soapEndPointUrl, string oauthToken, string deName = null) where T:class
        {
            var soapClient = GetSoapClient(soapEndPointUrl);

            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);
                var de = GetDataExtensionCreateObject(obj.GetType(), folderId, deName);
                var results = soapClient.Create(new CreateOptions(), new APIObject[] { de }, out var cRequestId, out var cStatus);
                foreach (CreateResult cr in results)
                {
                    Console.WriteLine("Status Message: " + cr.StatusMessage);
                }
            }
        }
        public static void InsertToDataExtenstion<T>(T obj, string oauthToken, string soapEndPointUrl, int orgId, string deName, bool isSharedDE, string parentOrgId = null) where T: class
        {
            var propInfo = typeof(T);
            var customerKey = GetDataExtensionKey(orgId, oauthToken, deName, soapEndPointUrl, isSharedDE);
            var soapClient = GetSoapClient(soapEndPointUrl);

            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);

                var deo = CreateDEObject(customerKey, 20, parentOrgId);
                deo= GetDataExtensionModel<T>(deo, obj);
                try
                {
                    var cResults = soapClient.Create(new CreateOptions(), new APIObject[] { deo }, out _, out _);
                    foreach (var cr in cResults)
                    {
                        Console.WriteLine(cr.StatusMessage);
                    }
                }
                catch (Exception exCreate)
                {
                    Console.WriteLine(exCreate.Message);
                }
            }
        }

        public static void AddOrUpdateDataExtenstion<T>(List<T> objectList, string accessToken, string oauthToken, string soapEndPointUrl, string restDomainUrl, int orgId, string deName, bool isSharedDE) where T : class
        {
            var customerKey = GetDataExtensionKey(orgId, oauthToken, deName, soapEndPointUrl, isSharedDE);
            var data = objectList.Select(GetRestDEObject);
            var url = restDomainUrl + @"hub/v1/dataevents/key:" + customerKey + @"/rowset";
            var header = new Dictionary<string, string>() {{"Authorization", "Bearer "+ accessToken } };
            var result = HttpHelper<T>.HttpPost(url, data.ToJsonString(),header);

        }

        // due to soap client binding not supported by .net core2 its not useable currently 
        public static void AddOrUpdateDataExtenstion<T>(List<T> objectList, SalesForceAuthenticationModel sfModel, string deName, bool isSharedDE) where T : class
        {
            var customerKey = GetDataExtensionKey(sfModel.OrganizationId.ToInt(), sfModel.LegacyToken, deName, sfModel.SoapEndPointUrl, isSharedDE);
            var data = objectList.Select(GetRestDEObject);
            var url = sfModel.RestDomainUrl + @"hub/v1/dataevents/key:" + customerKey + @"/rowset";
            var header = new Dictionary<string, string>() { { "Authorization", "Bearer " + sfModel.AccessToken } };
            var result = HttpHelper<T>.HttpPost(url, data.ToJsonString(), header);

        }

        public static List<T> AddOrUpdateDataExtenstionByKey<T>(List<T> objectList, SalesForceAuthenticationModel sfModel, string dataExtensionKey, bool isSharedDE) where T : class
        {
            var data = objectList.Select(GetRestDEObject);
            var url = sfModel.RestDomainUrl + @"hub/v1/dataevents/key:" + dataExtensionKey + @"/rowset";
            var header = new Dictionary<string, string>() { { "Authorization", "Bearer " + sfModel.AccessToken } };

            var result = HttpHelper<List<T>>.HttpPost(url, data.ToJsonString(), header);
            return result;
        }



        public static async Task<List<T>> AddOrUpdateDataExtenstionByKeyAsync<T>(List<T> objectList, SalesForceAuthenticationModel sfModel, string dataExtensionKey, bool isSharedDE) where T : class
        {
            var data = objectList.Select(GetRestDEObject);
            var url = sfModel.RestDomainUrl + @"hub/v1/dataevents/key:" + dataExtensionKey + @"/rowset";
            var header = new Dictionary<string, string>() { { "Authorization", "Bearer " + sfModel.AccessToken } };
            var result =await HttpHelper<List<T>>.HttpPostAsync(url, data.ToJsonString(), header, useHttpHandler: false).ConfigureAwait(false);
            return result;
        }

        public static async Task<List<T>> AddOrUpdateDataExtenstionHasTwoKeysAsync<T>(List<T> objectList, SalesForceAuthenticationModel sfModel, string dataExtensionKey, bool isSharedDE) where T : class
        {
            var data = objectList.Select(GetRestDEObjectWithTwoKey);
            var url = sfModel.RestDomainUrl + @"hub/v1/dataevents/key:" + dataExtensionKey + @"/rowset";
            var header = new Dictionary<string, string>() { { "Authorization", "Bearer " + sfModel.AccessToken } };
            var result = await HttpHelper<List<T>>.HttpPostAsync(url, data.ToJsonString(), header, useHttpHandler: false).ConfigureAwait(false);
            return result;
        }
        public static void DeleteDataExtension(int orgId, string oauthToken, string soapEndPointUrl, string deName, bool isSharedDE)
        {
            var customerKey = GetDataExtensionKey(orgId, oauthToken, deName, soapEndPointUrl, isSharedDE);

            var soapClient = GetSoapClient(soapEndPointUrl);
          
            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);

                DeleteRequest del = new DeleteRequest();
                DataExtension obj = new DataExtension()
                {

                    CustomerKey = customerKey
                };
                del.Objects = new APIObject[] { obj };

                var resultData = soapClient.Delete(new DeleteOptions(), new APIObject[] { obj }, out var cRequestId, out var cStatus);
            }
        }

        public static string GetDataExtensionKey(int orgId, string oauthToken, string deName, string soapEndPointUrl,bool isSharedDE)
        {
            var soapClient = GetSoapClient(soapEndPointUrl);

            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);
                var sfp = new SimpleFilterPart { Property = "Name", SimpleOperator = SimpleOperators.equals, Value = new[] { deName } };
                var rr = DERequestObject(orgId,isSharedDE);
                rr.Filter = sfp;
                soapClient.Retrieve(rr, out _, out var results);
                return results.FirstOrDefault()?.CustomerKey;
            }
        }

        private static RetrieveRequest DERequestObject(int orgId,bool isSharedDE)
        {
            RetrieveRequest request = new RetrieveRequest()
            {
                ObjectType = "DataExtension",
                Properties = new[]
                {
                    "ObjectID", "PartnerKey", "CustomerKey", "Name"
                }
            };
            if (isSharedDE)
            {
                request.QueryAllAccounts = true;
                request.QueryAllAccountsSpecified = true;
            }
            else
                request.ClientIDs = new ClientID[] { new ClientID { ID = orgId, IDSpecified = true } };
            return request;
        }

        public static int GetDataExtenstionRootFolderId(int orgId, string oauthToken, string soapEndPointUrl, bool isSharedDE, string folderName = null, string parentOrgId = null)
        {
            var soapClient = GetSoapClient(soapEndPointUrl);

            using (var scope = new OperationContextScope(soapClient.InnerChannel))
            {
                GetAuthorization(oauthToken);
                var sfp = new SimpleFilterPart { Property = "Name", SimpleOperator = SimpleOperators.@equals, Value = new[] { string.IsNullOrEmpty(folderName) ? "Data Extensions" : folderName } };
                var sfpContentType = new SimpleFilterPart { Property = "ContentType", SimpleOperator = SimpleOperators.@equals, Value = new string[] { isSharedDE ? "shared_dataextension" : "dataextension" } };
                var cf = new ComplexFilterPart { LeftOperand = sfp, LogicalOperator = LogicalOperators.AND, RightOperand = sfpContentType };
                var rr = new RetrieveRequest
                {
                    ObjectType = "DataFolder",
                    Properties = new[] { "Name", "Description", "ContentType", "ID", "ObjectID", "CustomerKey", "ParentFolder.Name", "ParentFolder.Description", "ParentFolder.ContentType", "ParentFolder.ID", "ParentFolder.ObjectID", "ParentFolder.CustomerKey" },
                    Filter = cf
                };
                if (isSharedDE)
                {
                    rr.QueryAllAccounts = true;
                    rr.QueryAllAccountsSpecified = true;
                }
                else
                {
                    rr.ClientIDs = CreateClientIds(orgId, parentOrgId);
                }
                soapClient.Retrieve(rr, out _, out var results);
                return results[0].ID;
            }
        }

        private static SoapClient GetSoapClient(string soapEndPointUrl)
        {
            var binding = BasicHttpBinding();
            var soapClient = new SoapClient(binding, new EndpointAddress(new Uri(soapEndPointUrl)))
            {
                ClientCredentials = { UserName = { UserName = "*", Password = "*" } }
            };
            return soapClient;
        }

        private static void GetAuthorization(string oauthToken)
        {
            XNamespace ns = "http://exacttarget.com";
            var oauthElement = new XElement(ns + "oAuthToken", oauthToken);
            var xmlHeader = MessageHeader.CreateHeader("oAuth", "http://exacttarget.com", oauthElement);
            OperationContext.Current.OutgoingMessageHeaders.Add(xmlHeader);
            var httpRequest = new HttpRequestMessageProperty();
            OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, httpRequest);
            httpRequest.Headers.Add(HttpRequestHeader.UserAgent, ET_Client.SDKVersion);
        }

        private static DataExtensionObject CreateDEObject(string customerKey, int propCount, string parentOrgId)
        {
            return parentOrgId.IsNotNullOrWhiteSpace() ? new DataExtensionObject { CustomerKey = customerKey, Client = new ClientID() { ID = Convert.ToInt32(parentOrgId) }, Properties = new APIProperty[propCount] } : new DataExtensionObject { CustomerKey = customerKey, Properties = new APIProperty[propCount] };
        }

        private static DataExtensionObject GetDataExtensionModel<T>(DataExtensionObject deo, T data)
        {
            var propInfo = typeof(T).GetProperties();
            for (var i = 0; i < propInfo.Length; i++)
            {
                deo.Properties[i] = new APIProperty(){Name = propInfo[i].Name ,Value = propInfo[i].GetValue(data,null).ToString()};
            }

            return deo;
        }
        private static ClientID[] CreateClientIds(int orgId,string parentOrgId)
        {
            return new[] { new ClientID { ID = (!(string.IsNullOrEmpty(parentOrgId)) ? Convert.ToInt32(parentOrgId) : orgId), IDSpecified = true } };
        }
        private static BasicHttpBinding BasicHttpBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                Name = "UserNameSoapBinding",
                Security = { Mode = BasicHttpSecurityMode.TransportWithMessageCredential },
                ReceiveTimeout = new TimeSpan(0, 5, 0),
                OpenTimeout = new TimeSpan(0, 5, 0),
                CloseTimeout = new TimeSpan(0, 5, 0),
                SendTimeout = new TimeSpan(0, 5, 0),
                MaxReceivedMessageSize = 20000000,
                MaxBufferPoolSize = 20000000,
                MaxBufferSize = 20000000
            };
            return binding;
        }
        private static DataExtensionFieldType GetFieldType(string type)
        {
            switch (type)
            {
                case "System.String":
                    return DataExtensionFieldType.Text;
                case "System.Int32":
                case "System.Int64":
                    return DataExtensionFieldType.Number;
                case "System.Boolean":
                    return DataExtensionFieldType.Boolean;
                case "System.DateTime":
                    return DataExtensionFieldType.Date;
                case "System.Decimal":
                    return DataExtensionFieldType.Decimal;
            }
            return DataExtensionFieldType.Text;
        }
        private static DataExtension GetDataExtensionCreateObject(Type type, int folderId, string deName = null)
        {
            var propInfo = type.GetProperties();
            var count = propInfo.Length + 1;
            var de = new DataExtension
            {
                CustomerKey = Guid.NewGuid().ToString(),
                Name = deName ?? type.Name,
                Fields = new DataExtensionField[count],
                CategoryID = folderId,
                CategoryIDSpecified = true,
                IsSendable = true,
                IsSendableSpecified = true,
                SendableDataExtensionField = new DataExtensionField { FieldType = DataExtensionFieldType.Text, Name = propInfo.First().Name },
                SendableSubscriberField = new FuelSDK.Attribute { Name = "Subscriber Key" }
            };
            de.Fields[0] = new DataExtensionField
            {
                Name = propInfo.First().Name,
                FieldType = DataExtensionFieldType.Text,
                FieldTypeSpecified = true,
                IsRequired = true,
                IsRequiredSpecified = true,
                IsPrimaryKey = true,
                IsPrimaryKeySpecified = true,
                MaxLength = 200,
                MaxLengthSpecified = true
            };

            for (var i = 1; i < propInfo.Length; i++)
            {
                var fieldType = GetFieldType(propInfo[i].PropertyType.ToString());
                de.Fields[i] = new DataExtensionField
                {
                    Name = propInfo[i].Name,
                    FieldType = fieldType,
                    FieldTypeSpecified = true,
                    IsRequired = true,
                    IsRequiredSpecified = true,
                    IsPrimaryKey = false,
                    IsPrimaryKeySpecified = true,
                    MaxLength = fieldType == DataExtensionFieldType.Text ? 200 : 0,
                    MaxLengthSpecified = fieldType == DataExtensionFieldType.Text
                };
            }
            return de;
        }

        private static DataExtensionRestModel GetRestDEObject<T>(T obj)
        {
            var type = obj.GetType();
            var propInfo = type.GetProperties();
            var tt = propInfo.First().Name;
            var data = new DataExtensionRestModel
            {
                keys = DataExtensionHelper.GetDynamicObject(
                    new Dictionary<string, object>() {
                        {tt, propInfo.First().GetValue(obj,null) ?? ""}
                    }),
                values = obj
           };
            return data;
        }

        private static DataExtensionRestModel GetRestDEObjectWithTwoKey<T>(T obj)
        {
            var type = obj.GetType();
            var propInfo = type.GetProperties();
            var firstKey = propInfo.First().Name;
            var secondKey = propInfo[1].Name;
            var data = new DataExtensionRestModel
            {
                keys = DataExtensionHelper.GetDynamicObject(
                    new Dictionary<string, object>() {
                        {firstKey, propInfo.First().GetValue(obj,null) ?? ""},
                        {secondKey, propInfo[1].GetValue(obj,null) ?? ""}
                    }),
                values = obj
            };
            return data;
        }
    }
}
