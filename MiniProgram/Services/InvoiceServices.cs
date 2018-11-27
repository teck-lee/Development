using Microsoft.Extensions.Configuration;
using MiniProgram.Dto;
using MiniProgram.Model;
using MiniProgram.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniProgram.Services
{
    public class InvoiceServices
    {
        private InvoiceRepository _invRepository;

        public InvoiceServices(string connStr)
        {
            _invRepository = new InvoiceRepository(connStr);
        }

        public InvoiceServices()
        {
            _invRepository = new InvoiceRepository();
        }

        public async Task<ResponseBase> DebugConnection()
        {
            return await _invRepository.DebugConnection();
        }

        public async Task<Invoice> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            return await _invRepository.GetInvoiceByInvoiceNo(invoiceNo);
        }

        public async Task<List<Invoice>> GetInvoices()
        {
            return await _invRepository.GetInvoices();
        }

        public async Task<ResponseBase> CreateInvoice(CreateInvoiceRequest request)
        {
            var response = new ResponseBase();
            //validate invoice no

            bool isExist = await _invRepository.IsInvoiceNoExist(request.InvoiceNo);
            if (isExist)
            {
                response.Message = "Duplicate Invoice Number.";

                return response;
            }

            return await _invRepository.CreateInvoice(request);
        }

        public async Task<ResponseBase> UpdateInvoice(UpdateInvoiceRequest request)
        {
            var response = new ResponseBase();
            //validate invoice no

            bool isExist = await _invRepository.IsInvoiceExist(request.InvoiceNo, request.InvoiceId);
            if (isExist)
            {
                response.Message = "Duplicate Invoice Number.";

                return response;
            }

            return await _invRepository.UpdateInvoice(request);
        }

        public async Task<ResponseBase> AutoGenerateInvoice(int totalInvoice)
        {
            var output = new ResponseBase();

            output.Message = "The invoices are generating .. Notification will be sent once completed.";
            await _invRepository.AutoGenerateInvoice(totalInvoice);

            return output;
        }
    }
}