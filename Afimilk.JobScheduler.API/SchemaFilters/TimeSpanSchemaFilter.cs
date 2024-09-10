using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Afimilk.JobScheduler.API
{
    public class TimeSpanSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(TimeSpan))
            {
                schema.Type = "string";
                schema.Format = "time"; // Custom format to represent time
                schema.Example = new OpenApiString(DateTime.Now.ToString("HH:mm:ss")); // Example time in "HH:mm:ss" format
            }
        }
    }

}
