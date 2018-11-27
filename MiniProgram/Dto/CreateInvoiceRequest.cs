using System;
using System.Collections.Generic;

namespace MiniProgram.Dto
{
    public class CreateInvoiceRequest
    {
        public CreateInvoiceRequest()
        {
            Data = new List<InvoiceDetailRequest>();
        }

        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerId { get; set; }
        public List<InvoiceDetailRequest> Data;
    }
}