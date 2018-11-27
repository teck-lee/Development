using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniProgram.Model
{
    public class InvoiceDetail
    {
        public int InvoiceDetailId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }


        [DatabaseGenerated(DatabaseGeneratedOption.Computed), Category("NoMap")]
        public string ProductCode { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed), Category("NoMap")]
        public string ProductName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed), Category("NoMap")]
        public decimal SubTotal { get; set; }
    }
}