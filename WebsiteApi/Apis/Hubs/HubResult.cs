using System.Text.Json.Nodes;

namespace WebsiteApi.Apis.Hubs;

public class HubResult
{
    public HubResult( JsonNode? result = null)
    {
     
        Result = result;
    }
  
        
    public JsonNode? Result { get; set; }
}