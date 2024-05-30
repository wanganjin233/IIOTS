using System.Security.Claims;

namespace IIOTS.WebRMS.Models 
{ 
    public class UserDto
    { 
        public string? Token { get; set; }
        public Dictionary<string, string>?    Claims { get; set; }
    }
}
