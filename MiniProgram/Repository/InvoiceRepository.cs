using Dapper;
using FizzWare.NBuilder;
using Microsoft.Extensions.Configuration;
using MiniProgram.Dto;
using MiniProgram.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniProgram.Repository
{
    public class InvoiceRepository
    {
       private static string connectionstring = "Server=sql2014.cduvbt2aasqk.ap-southeast-1.rds.amazonaws.com;Database=RanstadDb;User Id=sa; Password=Password;Connection Timeout=1200;MultipleActiveResultSets=true;";
       //private static string connectionstring = "Server=localhost;Database=RanstadDb;User Id=sa; Password=P@ssw0rd;Connection Timeout=100;MultipleActiveResultSets=true;";

        public InvoiceRepository()
        {

        }

        public InvoiceRepository(string connStr)
        {
           
        }

        internal async Task<ResponseBase> DebugConnection()
        {
            var result = new ResponseBase();

            using (var connection = new SqlConnection(connectionstring))
            {              

                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(@"SELECT * FROM dbo.product");

                    await connection.QueryAsync(sb.ToString());
                }
                catch (Exception ex)
                {

                    result.Message = ex.InnerException + " :: " + ex.StackTrace;
                }
            }
            return result;
        }

        internal async Task<Invoice> GetInvoiceByInvoiceNo(string invoice)
        {
            var output = new Invoice();

            StringBuilder sb = new StringBuilder();

            try
            {
                sb.Append(
                        @"SELECT
                            i.InvoiceId,
                            i.InvoiceNo,
                            i.InvoiceDate,
                            c.CustomerId,
                            c.CustomerName,
                            c.Address,
							id.invoiceDetailId,
                            id.invoiceId,
                            id.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            p.UnitPrice,
                            id.Quantity,
                            (p.UnitPrice * id.Quantity) As SubTotal
                          FROM dbo.invoice i
                          INNER JOIN dbo.InvoiceDetail id
                            ON i.invoiceId = id.InvoiceId
                          INNER JOIN dbo.Customer c
                            ON i.CustomerId = c.CustomerId
                          INNER JOIN dbo.Product p
                            ON id.ProductId = p.ProductId
                          WHERE i.InvoiceNo = @invNo"
                );

                using (var connection = new SqlConnection(connectionstring))
                {
                    var invoiceDictionary = new Dictionary<int, Invoice>();

                    object parameters = new
                    {
                        invNo = invoice
                    };

                    var invList = await connection.QueryAsync<Invoice, InvoiceDetail, Invoice>(
                    sb.ToString(),
                    (inv, invDetail) =>
                    {
                        if (!invoiceDictionary.TryGetValue(inv.InvoiceId, out Invoice invEntry))
                        {
                            invEntry = inv;
                            invEntry.Data = new List<InvoiceDetail>();
                            invoiceDictionary.Add(invEntry.InvoiceId, invEntry);
                        }

                        invEntry.Data.Add(invDetail);
                        return invEntry;
                    },
                    splitOn: "Address", param: parameters);
                    output = invList.FirstOrDefault();
                }
            }
            catch (Exception )
            {
            }
            return output;
        }

        internal async Task<List<Invoice>> GetInvoices()
        {
            var output = new List<Invoice>();

            StringBuilder sb = new StringBuilder();

            try
            {
                sb.Append(
                        @"SELECT
                            i.InvoiceId,
                            i.InvoiceNo,
                            i.InvoiceDate,
                            c.CustomerId,
                            c.CustomerName,
                            c.Address,
							id.invoiceDetailId,
                            id.invoiceId,
                            id.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            p.UnitPrice,
                            id.Quantity,
                            (p.UnitPrice * id.Quantity) As SubTotal
                          FROM dbo.invoice i
                          INNER JOIN dbo.InvoiceDetail id
                            ON i.invoiceId = id.InvoiceId
                          INNER JOIN dbo.Customer c
                            ON i.CustomerId = c.CustomerId
                          INNER JOIN dbo.Product p
                            ON id.ProductId = p.ProductId"
                );

                using (var connection = new SqlConnection(connectionstring))
                {
                    var invoiceDictionary = new Dictionary<int, Invoice>();

                    var invList = await connection.QueryAsync<Invoice, InvoiceDetail, Invoice>(
                    sb.ToString(),
                    (inv, invDetail) =>
                    {
                        if (!invoiceDictionary.TryGetValue(inv.InvoiceId, out Invoice invEntry))
                        {
                            invEntry = inv;
                            invEntry.Data = new List<InvoiceDetail>();
                            invoiceDictionary.Add(invEntry.InvoiceId, invEntry);
                        }

                        invEntry.Data.Add(invDetail);
                        return invEntry;
                    },
                    splitOn: "invoiceDetailId");

                    output = invList.Distinct().ToList();
                }
            }
            catch (Exception )
            {
            }
            return output;
        }

        internal async Task<ResponseBase> CreateInvoice(Invoice request)
        {
            var result = new ResponseBase();
            StringBuilder sb = new StringBuilder();

            int invoiceId = 0;

            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("CreateInvoice");

                try
                {
                    sb.Append(@" INSERT INTO dbo.Invoice (InvoiceNo, InvoiceDate, CustomerId, CreatedDatetime)
                         VALUES (@invoiceNo, @invDate, @customerId, GetDate()) ;
                         SELECT CAST(SCOPE_IDENTITY() AS int)");

                    object param = new
                    {
                        invoiceNo = request.InvoiceNo,
                        invDate = request.InvoiceDate,
                        customerId = request.CustomerId
                    };

                    invoiceId = (int)await connection.ExecuteScalarAsync(sb.ToString(), param, transaction);

                    foreach (InvoiceDetail id in request.Data)
                    {
                        sb.Clear();

                        sb.Append(@" INSERT INTO dbo.InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice)
                             VALUES (@invoiceId, @productId, @qty, @unitPrice);");

                        object param1 = new
                        {
                            invoiceId = invoiceId,
                            productId = id.ProductId,
                            qty = id.Quantity,
                            unitPrice = id.UnitPrice
                        };

                        await connection.ExecuteScalarAsync(sb.ToString(), param1, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception )
                {
                    transaction.Rollback();
                    result.Message = "Error inserting record";
                }
            }

            return result;
        }

        internal async Task<ResponseBase> CreateInvoice(List<Invoice> requestList)
        {
            var result = new ResponseBase();
            StringBuilder sb = new StringBuilder();
            
            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                foreach (Invoice request in requestList)
                {
                    int invoiceId = 0;

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        //    SqlTransaction transaction;
                        //transaction = connection.BeginTransaction("CreateInvoice");

                        try
                        {
                            sb.Clear();

                            sb.Append(@" INSERT INTO dbo.Invoice (InvoiceNo, InvoiceDate, CustomerId, CreatedDatetime)
                         VALUES (@invoiceNo, @invDate, @customerId, GetDate()) ;
                         SELECT CAST(SCOPE_IDENTITY() AS int)");

                            object param = new
                            {
                                invoiceNo = request.InvoiceNo,
                                invDate = request.InvoiceDate,
                                customerId = request.CustomerId
                            };

                            invoiceId = (int)await connection.ExecuteScalarAsync(sb.ToString(), param, transaction);
                                                        
                            foreach (InvoiceDetail id in request.Data)
                            {
                                id.InvoiceId = invoiceId;
                             //   sb.Clear();

                             //   sb.Append(@" INSERT INTO dbo.InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice)
                             //VALUES (@invoiceId, @productId, @qty, @unitPrice);");

                                //object param1 = new
                                //{
                                //    invoiceId = invoiceId,
                                //    productId = id.ProductId,
                                //    qty = id.Quantity,
                                //    unitPrice = id.UnitPrice
                                //};

                                //await connection.ExecuteScalarAsync(sb.ToString(), param1, transaction);
                            }

                            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.BatchSize = 100;
                                bulkCopy.DestinationTableName = "dbo.InvoiceDetail";
                                try
                                {
                                   var dt = request.Data.AsDataTable();

                                    MapColumns(dt, bulkCopy);
                                    

                                    bulkCopy.WriteToServer(request.Data.AsDataTable());
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    connection.Close();
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception )
                        {
                            transaction.Rollback();
                            result.Message = "Error inserting record";
                        }
                    }
                }

                connection.Close();
            }

            return result;
        }

        internal async Task<ResponseBase> GenerateInvoice(Invoice request)
        {
            var result = new ResponseBase();
            StringBuilder sb = new StringBuilder();

            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
               
                    int invoiceId = 0;

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        //    SqlTransaction transaction;
                        //transaction = connection.BeginTransaction("CreateInvoice");

                        try
                        {
                            sb.Clear();

                            sb.Append(@" INSERT INTO dbo.Invoice (InvoiceNo, InvoiceDate, CustomerId, CreatedDatetime)
                         VALUES (@invoiceNo, @invDate, @customerId, GetDate()) ;
                         SELECT CAST(SCOPE_IDENTITY() AS int)");

                            object param = new
                            {
                                invoiceNo = request.InvoiceNo,
                                invDate = request.InvoiceDate,
                                customerId = request.CustomerId
                            };

                            invoiceId = (int)await connection.ExecuteScalarAsync(sb.ToString(), param, transaction);

                            foreach (InvoiceDetail id in request.Data)
                            {
                            //  id.InvoiceId = invoiceId;
                                    sb.Clear();

                                    sb.Append(@" INSERT INTO dbo.InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice)
                                        VALUES (@invId, @productId, @qty, @unitPrice);");

                                    object param1 = new
                                    {
                                        invId = invoiceId,
                                        productId = id.ProductId,
                                        qty = id.Quantity,
                                        unitPrice = id.UnitPrice
                                    };

                                 await connection.ExecuteScalarAsync(sb.ToString(), param1, transaction);
                            }

                            //using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            //{
                            //    bulkCopy.BatchSize = 100;
                            //    bulkCopy.DestinationTableName = "dbo.InvoiceDetail";
                            //    try
                            //    {
                            //        var dt = request.Data.AsDataTable();

                            //        MapColumns(dt, bulkCopy);


                            //        bulkCopy.WriteToServer(request.Data.AsDataTable());
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        transaction.Rollback();
                            //        connection.Close();
                            //    }
                            //}

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            result.Message = "Error inserting record";
                        }
                    }
              

                connection.Close();
            }

            return result;
        }

        internal ResponseBase AutoCreateInvoice(CreateInvoiceRequest request)
        {
            var result = new ResponseBase();
            StringBuilder sb = new StringBuilder();

            int invoiceId = 0;

            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("CreateInvoice");

                try
                {
                    sb.Append(@" INSERT INTO dbo.Invoice (InvoiceNo, InvoiceDate, CustomerId, CreatedDatetime)
                         VALUES (@invoiceNo, @invDate, @customerId, GetDate()) ;
                         SELECT CAST(SCOPE_IDENTITY() AS int)");

                    object param = new
                    {
                        invoiceNo = request.InvoiceNo,
                        invDate = request.InvoiceDate,
                        customerId = request.CustomerId
                    };

                    invoiceId = (int)connection.ExecuteScalar(sb.ToString(), param, transaction);

                    foreach (InvoiceDetailRequest id in request.Data)
                    {
                        sb.Clear();

                        sb.Append(@" INSERT INTO dbo.InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice)
                             VALUES (@invoiceId, @productId, @qty, @unitPrice);");

                        object param1 = new
                        {
                            invoiceId = invoiceId,
                            productId = id.ProductId,
                            qty = id.Quantity,
                            unitPrice = id.UnitPrice
                        };

                        connection.ExecuteScalar(sb.ToString(), param1, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception )
                {
                    transaction.Rollback();
                    result.Message = "Error inserting record";
                }
            }

            return result;
        }

        internal async Task<ResponseBase> UpdateInvoice(UpdateInvoiceRequest request)
        {
            var result = new ResponseBase();
            StringBuilder sb = new StringBuilder();

            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                SqlTransaction transaction;
                transaction = connection.BeginTransaction("UpdateInvoice");

                try
                {
                    sb.Append(@"UPDATE dbo.Invoice SET InvoiceNo = @invNo, InvoiceDate = @invDate, CustomerId = @custId, ModifiedDateTime = GetDate()
                                WHERE InvoiceId = @invId");

                    object param = new
                    {
                        invNo = request.InvoiceNo,
                        invDate = request.InvoiceDate,
                        custId = request.CustomerId,
                        invId = request.InvoiceId
                    };

                    await connection.ExecuteScalarAsync(sb.ToString(), param, transaction);

                    //Delete from invoice Detail
                    sb.Clear();
                    sb.Append(@"Delete FROM dbo.invoiceDetail WHERE invoiceId = @invId");
                    await connection.ExecuteScalarAsync(sb.ToString(), new { invId = request.InvoiceId }, transaction);

                    foreach (InvoiceDetailRequest id in request.Data)
                    {
                        sb.Clear();

                        sb.Append(@" INSERT INTO dbo.InvoiceDetail (InvoiceId, ProductId, Quantity, UnitPrice)
                             VALUES (@invoiceId, @productId, @qty, @unitPrice);");

                        object param1 = new
                        {
                            invoiceId = request.InvoiceId,
                            productId = id.ProductId,
                            qty = id.Quantity,
                            unitPrice = id.UnitPrice
                        };

                        await connection.ExecuteScalarAsync(sb.ToString(), param1, transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception )
                {
                    transaction.Rollback();
                    result.Message = "Error Updating record";
                }
            }

            return result;
        }

        internal async Task<bool> IsInvoiceNoExist(string invoiceNo)
        {
            bool result = false;
            StringBuilder sb = new StringBuilder();

            using (var connection = new SqlConnection(connectionstring))
            {
                try
                {
                    sb.Append(@"SELECT count(invoiceNo) FROM dbo.Invoice WHERE invoiceNo = @invNo");

                    object param = new
                    {
                        invNo = invoiceNo
                    };

                    result = await connection.ExecuteScalarAsync<bool>(sb.ToString(), param);
                }
                catch (Exception )
                {
                }
                return result;
            }
        }

        internal async Task<bool> IsInvoiceExist(string invoiceNo, int invoiceId)
        {
            bool result = false;
            StringBuilder sb = new StringBuilder();

            using (var connection = new SqlConnection(connectionstring))
            {
                try
                {
                    sb.Append(@"SELECT count(invoiceNo) FROM dbo.Invoice WHERE invoiceNo = @invNo AND invoiceId != @invId");

                    object param = new
                    {
                        invNo = invoiceNo,
                        invId = invoiceId
                    };

                    result = await connection.ExecuteScalarAsync<bool>(sb.ToString(), param);
                }
                catch (Exception )
                {
                }
                return result;
            }
        }

        internal async Task<ResponseBase> AutoGenerateInvoice(int totalInvoice)
        {
            var output = new ResponseBase();

            try
            {
                var invoices = GenerateRandomInvoice(totalInvoice);

                 Parallel.ForEach(invoices, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, async invoice => await GenerateInvoice(invoice));
              //  await Task.Factory.StartNew(() => Parallel.ForEach(invoices, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, async inv => await GenerateInvoice(inv)));

                //await Task.Factory.StartNew(() => CreateInvoice(invoices));

            }
            catch (Exception )
            {
            }

            return output;
        }

        #region Auto Generate Test Invoice

        private List<Invoice> GenerateRandomInvoice(int totalInvoice)
        {
            var output = new List<Invoice>();
            var generator = new RandomGenerator();
            try
            {
                var invoices = Builder<Invoice>.CreateListOfSize(totalInvoice)
                .All()
                .With(o => o.InvoiceNo = "Inv" + generator.Next(1, 10000000))
                .With(o => o.CustomerId = generator.Next(1, 3))
                .With(o => o.InvoiceDate = DateTime.Now)
                .Build();

                invoices.ToList().ForEach
                (i =>
                {
                    var invoiceItems = Builder<InvoiceDetail>.CreateListOfSize(generator.Next(1, 5))
                          .All()
                          .With(ii => ii.ProductId = generator.Next(1, 10))
                          .With(ii => ii.Quantity = generator.Next(1, 10))
                          .With(ii => ii.UnitPrice = generator.Next(50, 500))
                          .Build().ToList();

                    i.Data = invoiceItems;
                });

                output = (List<Invoice>)invoices;
            }
            catch (Exception )
            {
            }

            return output;
        }


        private void MapColumns(DataTable infoTable, SqlBulkCopy bulkCopy)
        {

            foreach (DataColumn dc in infoTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(dc.ColumnName,
                  dc.ColumnName);
            }
        }


        #endregion Auto Generate Test Invoice
    }
}