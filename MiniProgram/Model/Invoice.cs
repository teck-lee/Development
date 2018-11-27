using System;
using System.Collections.Generic;

namespace MiniProgram.Model
{
    public class Invoice
    {
        public Invoice()
        {
            Data = new List<InvoiceDetail>();
        }

        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public string CustomerName { get; set; }
        public string Address { get; set; }

        public List<InvoiceDetail> Data;
    }
}