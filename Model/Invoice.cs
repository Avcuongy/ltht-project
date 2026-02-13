using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ltht_project.Model
{
    internal class Invoice
    {
        private string invoiceId;
        private string orderId;
        private int quantity;
        private decimal unitSellingPrice;
        private DateTime invoiceDate;
        [JsonConstructor]
        public Invoice(string invoiceId, string orderId, int quantity, decimal unitSellingPrice, DateTime invoiceDate)
        {
            this.invoiceId = invoiceId;
            this.orderId = orderId;
            this.quantity = quantity;
            this.unitSellingPrice = unitSellingPrice;
            this.invoiceDate = invoiceDate;
        }
        public string InvoiceId { get => invoiceId; set => invoiceId = value; }
        public string OrderId { get => orderId; set => orderId = value; }
        public int Quantity { get => quantity; set => quantity = value; }
        public decimal UnitSellingPrice { get => unitSellingPrice; set => unitSellingPrice = value; }
        public DateTime InvoiceDate { get => invoiceDate; set => invoiceDate = value; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("invoiceId", invoiceId, typeof(string));
            info.AddValue("orderId", orderId, typeof(string));
            info.AddValue("quantity", quantity, typeof(int));
            info.AddValue("unitSellingPrice", unitSellingPrice, typeof(decimal));
            info.AddValue("invoiceDate", invoiceDate, typeof(DateTime));
        }
        public Invoice(SerializationInfo info, StreamingContext context)
        {
            invoiceId = (string)info.GetValue("invoiceId", typeof(string));
            orderId = (string)info.GetValue("orderId", typeof(string));
            quantity = (int)info.GetValue("quantity", typeof(int));
            unitSellingPrice = (decimal)info.GetValue("unitSellingPrice", typeof(decimal));
            invoiceDate = (DateTime)info.GetValue("invoiceDate", typeof(DateTime));
        }
    }
}
