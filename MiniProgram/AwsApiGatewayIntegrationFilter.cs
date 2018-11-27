using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MiniProgram
{
    public class AwsApiGatewayIntegrationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Extensions.Add("x-amazon-apigateway-integration", new
            {
                type = "aws",
                uri = "arn:aws:lambda:ap-southeast-1:449299978891:function:MiniProgram-AspNetCoreFunction-1LZL7VRPN43UW"
            });
        }
    }
}
