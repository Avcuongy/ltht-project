using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ltht_project.Model
{
    internal class PurchaseOrder
    {
        private string orderId;
        private string productId;
        private int quantity;
        private decimal unitCost;
        private DateTime purchaseDate;
        [JsonConstructor]
        public PurchaseOrder(string orderId, string productId, int quantity, decimal unitCost, DateTime purchaseDate)
        {
            this.orderId = orderId;
            this.productId = productId;
            this.quantity = quantity;
            this.unitCost = unitCost;
            this.purchaseDate = purchaseDate;
        }
        public string OrderId { get => orderId; set => orderId = value; }
        public string ProductId { get => productId; set => productId = value; }
        public int Quantity { get => quantity; set => quantity = value; }
        public decimal UnitCost { get => unitCost; set => unitCost = value; }
        public DateTime PurchaseDate { get => purchaseDate; set => purchaseDate = value; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("orderId", orderId, typeof(string));
            info.AddValue("productId", productId, typeof(string));
            info.AddValue("quantity", quantity, typeof(int));
            info.AddValue("unitCost", unitCost, typeof(decimal));
            info.AddValue("purchaseDate", purchaseDate, typeof(DateTime));
        }
        public PurchaseOrder(SerializationInfo info, StreamingContext context)
        {
            orderId = (string)info.GetValue("orderId", typeof(string));
            productId = (string)info.GetValue("productId", typeof(string));
            quantity = (int)info.GetValue("quantity", typeof(int));
            unitCost = (decimal)info.GetValue("unitCost", typeof(decimal));
            purchaseDate = (DateTime)info.GetValue("purchaseDate", typeof(DateTime));
        }
    }
}
