//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;using System;
//using System.Net.Http;
//using System.Text;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using Microsoft.IdentityModel.Tokens;
//using Newtonsoft.Json.Linq;
//using Microsoft.EntityFrameworkCore;

//namespace Learning_Space.Controllers
//{
//    public class tryzoom : Controller
//    {



//class Program
//    {
//        static void Main(string[] args)
//        {
//            var apiKey = "YOUR_ZOOM_API_KEY";
//            var apiSecret = "YOUR_ZOOM_API_SECRET";
//            var userId = "YOUR_ZOOM_USER_ID";

//            var jwtToken = GenerateJWTToken(apiKey, apiSecret);
//            var meetingLink = CreateZoomMeeting(jwtToken, userId);

//            Console.WriteLine($"Meeting Link: {meetingLink}");
//        }

//        static string GenerateJWTToken(string apiKey, string apiSecret)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(apiSecret);

//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                new Claim("iss", apiKey),
//                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString())
//            }),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            return tokenHandler.WriteToken(token);
//        }

//        static string CreateZoomMeeting(string jwtToken, string userId)
//        {
//            using (var httpClient = new HttpClient())
//            {
//                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");

//                var requestUri = $"https://api.zoom.us/v2/users/{userId}/meetings";
//                var requestData = new
//                {
//                    topic = "New Video Meeting",
//                    type = 1 // 1 for instant meetings
//                };

//                var response = httpClient.PostAsJsonAsync(requestUri, requestData).Result;
//                var responseBody = response.Content.ReadAsStringAsync().Result;
//                var meetingId = JObject.Parse(responseBody)["id"].ToString();

//                var meetingLink = $"https://zoom.us/j/{meetingId}";
//                return meetingLink;
                   
//                }
//        }
//    }
//}
