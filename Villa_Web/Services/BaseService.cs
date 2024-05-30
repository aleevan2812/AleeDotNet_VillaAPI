using System.Text;
using Newtonsoft.Json;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Services.IServices;

namespace Villa_Web.Services;

public class BaseService : IBaseService
{
    public APIResponse responseModel { get; set; }
    public IHttpClientFactory httpClient { get; set; }

    public BaseService(IHttpClientFactory httpClient)
    {
        responseModel = new APIResponse();
        this.httpClient = httpClient;
    }

    public async Task<T> SendAsync<T>(APIRequest apiRequest)
    {
        try
        {
            var client = httpClient.CreateClient("MagicAPI");
            HttpRequestMessage message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri(apiRequest.Url);
            
            // Khi apiRequest.Data không null: Đoạn mã này sẽ serial hóa apiRequest.Data thành JSON, mã hóa nó theo UTF-8, và đặt nó vào nội dung của thông điệp yêu cầu (message.Content).
            // Kết quả: Yêu cầu HTTP được gửi đi sẽ chứa dữ liệu JSON này trong phần thân, và server sẽ nhận biết rằng dữ liệu được gửi là JSON nhờ vào Content-Type được thiết lập.
            if (apiRequest.Data != null)
                message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                    Encoding.UTF8, "application/json");

            switch (apiRequest.ApiType)
            {
                case SD.ApiType.POST:
                    message.Method = HttpMethod.Post;
                    break;
                case SD.ApiType.PUT:
                    message.Method = HttpMethod.Put;
                    break;
                case SD.ApiType.DELETE:
                    message.Method = HttpMethod.Delete;
                    break;
                default:
                    message.Method = HttpMethod.Get;
                    break;
            }

            // gửi yêu cầu
            HttpResponseMessage apiResponse = null;
            apiResponse = await client.SendAsync(message);

            // Chức năng: Đọc nội dung của phản hồi HTTP (apiResponse) dưới dạng một chuỗi không đồng bộ.
            var apiContent = await apiResponse.Content.ReadAsStringAsync();
            
            try
            {
                // Chuyển đổi chuỗi JSON apiContent thành một đối tượng thuộc kiểu APIResponse.
                APIResponse ApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                if(apiResponse.StatusCode==System.Net.HttpStatusCode.BadRequest 
                   || apiResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ApiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    ApiResponse.IsSuccess = false;
                    var res = JsonConvert.SerializeObject(ApiResponse);
                    var returnObj = JsonConvert.DeserializeObject<T>(res);
                    return returnObj;
                }
            }
            catch (Exception e)
            {
                var exceptionResponse = JsonConvert.DeserializeObject<T>(apiContent);
                return exceptionResponse;
            }
            
            // Chuyển đổi chuỗi JSON apiContent thành một đối tượng thuộc kiểu T.
            var APIResponse = JsonConvert.DeserializeObject<T>(apiContent);
            return APIResponse;
        }
        catch (Exception e)
        {
            var dto = new APIResponse
            {
                ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                IsSuccess = false
            };
            
            // Chuyển đổi đối tượng APIResponse vừa tạo thành chuỗi JSON.
            var res = JsonConvert.SerializeObject(dto);
            
            // Chuỗi JSON được chuyển đổi thành một đối tượng thuộc kiểu T và trả về cho caller.
            var APIResponse = JsonConvert.DeserializeObject<T>(res);
            return APIResponse;
        }
    }
}