using System;
using System.Collections.Generic;

namespace MiniProgram.Dto
{
    public class UpdateInvoiceRequest
    {
        public UpdateInvoiceRequest()
        {
            Data = new List<InvoiceDetailRequest>();
        }

        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerId { get; set; }
        public List<InvoiceDetailRequest> Data;
    }
}