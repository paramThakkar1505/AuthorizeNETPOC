using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizeNETPOC.Filters
{
    public class ReplaceVersionWithExactValueInPath : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var openApiPaths = new OpenApiPaths();

            foreach (var item in swaggerDoc.Paths)
            {
                var key = item.Key.Replace("v{version}"
                    , swaggerDoc.Info.Version
                    , System.StringComparison.OrdinalIgnoreCase);

                openApiPaths.Add(key, item.Value);
            }

            swaggerDoc.Paths = openApiPaths;
        }
    }
}
