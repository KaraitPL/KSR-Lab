using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WCFServiceWebRole1
{
    public class User : TableEntity
    {
        public User(string rk, string pk)
        {
            this.PartitionKey = pk; // ustawiamy klucz partycji
            this.RowKey = rk; // ustawiamy klucz główny
                                       // this.Timestamp; jest tylko do odczytu
        }
        public User() { }
        public string Login { get; set; }
        public string Password { get; set; }
        public Guid SessionId { get; set; }
    }
}