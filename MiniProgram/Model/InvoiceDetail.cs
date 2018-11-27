namespace MiniProgram.Model
{
    public class InvoiceDetail
    {
        public int InvoiceDetailId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        
        public decimal SubTotal { get; set; }
    }
}