using Villa_Utility;

namespace AleeDotNet_VillaWeb.Models;

public class APIRequest
{
    public SD.ApiType ApiType { get; set; } =  SD.ApiType.GET;
    public string Url { get; set; }
    public object Data { get; set; }
}