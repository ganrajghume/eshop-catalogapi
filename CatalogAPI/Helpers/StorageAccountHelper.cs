using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Helpers
{
    public class StorageAccountHelper
    {
        private string storageConnectionString;

        private string tableConnectionString; //for CosmosDb Table API connection string

        private CloudStorageAccount storageAccount;

        private CloudStorageAccount tableStorageAccount;  //for CosmosDb Table API connection

        private CloudBlobClient blobClient;

        private CloudTableClient tableClient;

        public string StorageConnectionString
        {
            get
            {
                return storageConnectionString;
            }
            set
            {
                this.storageConnectionString = value;
                storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            }
        }
        public string TableConnectionString //  for CosmosDB Table API connection string
        {
            get
            {
                return tableConnectionString;
            }
            set
            {
                this.tableConnectionString = value;
                tableStorageAccount = CloudStorageAccount.Parse(this.tableConnectionString);
            }
        }

        public async Task<string> UploadFileToBlobAsync(string filepath, string containername)
        {
            blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containername);
            await container.CreateIfNotExistsAsync();


            BlobContainerPermissions permissions = new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            await container.SetPermissionsAsync(permissions);

            var filename = Path.GetFileName(filepath);
            var blob = container.GetBlockBlobReference(filename);
            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filepath);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<CatalogEntity> SaveToTableAsync(CatalogItem item)
        {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                ImageUrl = item.ImageUrl,
                ReorderLevel = item.ReorderLevel,
                Quantity = item.Quantity,
                Price = item.Price,
                ManufacturingDate= item.ManufacturingDate
            };
            // for save into blob storage
            //tableClient = storageAccount.CreateCloudTableClient();
            
            //for store in CosmosDB table storage account
            tableClient = tableStorageAccount.CreateCloudTableClient();
            var catalogtable = tableClient.GetTableReference("catalog");
            await catalogtable.CreateIfNotExistsAsync();
            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);
            var tableResult = await catalogtable.ExecuteAsync(operation);
            return tableResult.Result as CatalogEntity;
        }
    }
}
