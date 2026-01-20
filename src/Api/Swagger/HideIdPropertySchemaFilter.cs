using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger
{
    public class HideIdPropertySchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;

            // remove "id" (case-insensitive) de qualquer schema
            var key = schema.Properties.Keys.FirstOrDefault(k =>
                string.Equals(k, "id", StringComparison.OrdinalIgnoreCase));

            if (key is not null)
                schema.Properties.Remove(key);
        }
    }
}
