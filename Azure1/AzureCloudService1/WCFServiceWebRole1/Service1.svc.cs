using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFServiceWebRole1
{
    // UWAGA: możesz użyć polecenia „Zmień nazwę” w menu „Refaktoryzuj”, aby zmienić nazwę klasy „Service1” w kodzie, usłudze i pliku konfiguracji.
    // UWAGA: aby uruchomić klienta testowego WCF w celu przetestowania tej usługi, wybierz plik Service1.svc lub Service1.svc.cs w eksploratorze rozwiązań i rozpocznij debugowanie.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public bool Create(string login, string password)
        {
            CloudTable table = GetTableFromAzure("users");

            var checkIfExistsOperation = TableOperation.Retrieve<User>(login, password);
            var validationResult = table.Execute(checkIfExistsOperation);

            if (validationResult.Result != null)
            {
                return false;
            }

            var user = new User(login, password)
            {
                Login = login,
                Password = password,
                SessionId = Guid.Empty
            };

            var operation = TableOperation.Insert(user);
            var result = table.Execute(operation);

            if (result.Result == null)
            {
                return false;
            }

            return true;
        }

        public Guid Login(string login, string password)
        {
            CloudTable table = GetTableFromAzure("users");

            var checkIfExistsOperation = TableOperation.Retrieve<User>(login, password);
            var result = table.Execute(checkIfExistsOperation);
            var user = result.Result as User;

            if (user == null)
            {
                return Guid.Empty;
            }

            var sessionId = Guid.NewGuid();
            user.SessionId = sessionId;

            var updateOperation = TableOperation.Replace(user);
            table.Execute(updateOperation);

            return sessionId;
        }

        public bool Logout(string login)
        {
            CloudTable table = GetTableFromAzure("users");

            TableQuery<User> query = new TableQuery<User>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, login));
            var result = table.ExecuteQuery(query);
            var user = result.SingleOrDefault();

            if (user == null)
            {
                return false;
            }

            user.SessionId = Guid.Empty;

            var updateOperation = TableOperation.Replace(user);
            table.Execute(updateOperation);

            return true;
        }

        public bool Put(string name, string value, Guid sessionId)
        {
            var table = GetTableFromAzure("users");
            var blobContainer = GetBlobFromAzure("files");

            TableQuery<User> query = new TableQuery<User>()
                .Where(TableQuery.GenerateFilterConditionForGuid("SessionId", QueryComparisons.Equal, sessionId));
            var result = table.ExecuteQuery(query);
            var user = result.SingleOrDefault();

            if (user == null)
            {
                return false;
            }

            var nameOfBlob = user.Login + "_" + name;
            var blob = blobContainer.GetBlockBlobReference(nameOfBlob);

            var bytes = new ASCIIEncoding().GetBytes(value);
            var stream = new MemoryStream(bytes);
            blob.UploadFromStream(stream);

            return true;
        }

        public string Get(string name, Guid sessionId)
        {
            var table = GetTableFromAzure("users");
            var blobContainer = GetBlobFromAzure("files");

            TableQuery<User> query = new TableQuery<User>()
                .Where(TableQuery.GenerateFilterConditionForGuid("SessionId", QueryComparisons.Equal, sessionId));
            var result = table.ExecuteQuery(query);
            var user = result.SingleOrDefault();

            if (user == null)
            {
                return string.Empty;
            }

            var nameOfBlob = user.Login + "_" + name;
            var blob = blobContainer.GetBlockBlobReference(nameOfBlob);

            if (blob == null)
            {
                return string.Empty;
            }

            var stream = new MemoryStream();
            blob.DownloadToStream(stream);
            string content = Encoding.UTF8.GetString(stream.ToArray());

            return content;
        }

        private static CloudTable GetTableFromAzure(string nameOfTable)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = account.CreateCloudTableClient();
            var table = client.GetTableReference(nameOfTable);
            table.CreateIfNotExists();
            return table;
        }

        private static CloudBlobContainer GetBlobFromAzure(string nameOfBlobContainer)
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(nameOfBlobContainer);
            container.CreateIfNotExists();
            return container;
        }
    }
}
