using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ltht_project.Model
{
    internal class Invoice
    {
        private string invoiceId;   // Mã hóa đơn
        private string productId;   // Mã sản phẩm
        private int quantity;   // Số lượng bán
        private decimal unitSellingPrice;   // Đơn giá bán
        private DateTime invoiceDate;   // Ngày bán hàng

        [JsonConstructor]
        public Invoice(string invoiceId, string productId, int quantity, decimal unitSellingPrice, DateTime invoiceDate)
        {
            this.invoiceId = invoiceId;
            this.productId = productId;
            this.quantity = quantity;
            this.unitSellingPrice = unitSellingPrice;
            this.invoiceDate = invoiceDate;
        }
        public string InvoiceId { get => invoiceId; set => invoiceId = value; }
        public string ProductId { get => productId; set => productId = value; }
        public int Quantity { get => quantity; set => quantity = value; }
        public decimal UnitSellingPrice { get => unitSellingPrice; set => unitSellingPrice = value; }
        public DateTime InvoiceDate { get => invoiceDate; set => invoiceDate = value; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("invoiceId", invoiceId, typeof(string));
            info.AddValue("productId", productId, typeof(string));
            info.AddValue("quantity", quantity, typeof(int));
            info.AddValue("unitSellingPrice", unitSellingPrice, typeof(decimal));
            info.AddValue("invoiceDate", invoiceDate, typeof(DateTime));
        }
        public Invoice(SerializationInfo info, StreamingContext context)
        {
            invoiceId = (string)info.GetValue("invoiceId", typeof(string));
            productId = (string)info.GetValue("productId", typeof(string));
            quantity = (int)info.GetValue("quantity", typeof(int));
            unitSellingPrice = (decimal)info.GetValue("unitSellingPrice", typeof(decimal));
            invoiceDate = (DateTime)info.GetValue("invoiceDate", typeof(DateTime));
        }
    }
}
