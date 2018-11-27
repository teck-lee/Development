using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MiniProgram.Dto;
using MiniProgram.Model;
using MiniProgram.Services;
using System.Threading.Tasks;

namespace MiniProgram.Controllers
{
    [Route("/[controller]")]
    [Produces("application/json")]
    public class InvoicesController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly InvoiceServices _invoiceService;
       
        public InvoicesController(IHostingEnvironment env,
            InvoiceServices invoiceService)
        {
            _env = env;
            _invoiceService = invoiceService;          
        }

        [HttpGet("test")]
        public string TestAPI()
        {
           return "Success";
        }

        //[HttpGet("DebugConnection")]
        //public async Task<IActionResult> DebugConnection()
        //{

        //    var obj = await _invoiceService.DebugConnection();

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(obj);
        //    }
        //    else
        //    {
        //        return Ok(obj);
        //    }
        //}


        [HttpGet("GetInvoiceByInvoiceNo")]
        public async Task<IActionResult> GetInvoiceByInvoiceNo(string invoiceNo)
        {
            if (string.IsNullOrEmpty(invoiceNo))
            {
                return BadRequest("Invoice No. cannot be null or empty");
            }
            var invoiceObj = await _invoiceService.GetInvoiceByInvoiceNo(invoiceNo);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                return Ok(invoiceObj);
            }
        }

        [HttpGet("GetInvoices")]
        public async Task<IActionResult> GetInvoices()
        {
            var invoiceList = await _invoiceService.GetInvoices();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                return Ok(invoiceList);
            }
        }

        [HttpPost("CreateInvoice")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request object cannot be empty.");
            }

           var requestObj =  TypeAdapter.Adapt<CreateInvoiceRequest, Invoice>(request);

            var result = await _invoiceService.CreateInvoice(requestObj);

            if (!ModelState.IsValid || !string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] UpdateInvoiceRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var result = await _invoiceService.UpdateInvoice(request);

            if (!ModelState.IsValid || !string.IsNullOrEmpty(result.Message))
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [HttpGet("AutoGenerateInvoice")]
        public async Task<IActionResult> AutoGenerateInvoice(int totalInvoice)
        {
            if (totalInvoice.Equals(0))
            {
                return BadRequest("Total Invoice cannot be Zero");
            }

            var output = await _invoiceService.AutoGenerateInvoice(totalInvoice);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                return Ok(output);
            }
        }
    }
}