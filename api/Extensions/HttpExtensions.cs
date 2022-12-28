using System.Text.Json;
using api.Helpers;

namespace api.Extensions
{
    public static class HttpExtensions
    {
        // add a custom header for sending pagination informations
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            // header needs to be in JSON format and not C# object
            var jsonOptions = new JsonSerializerOptions
            {
                // we are not in the context of a controller, 
                // so the default for returning JSON is not camel case,
                // and we need to tell it
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }
    }
}