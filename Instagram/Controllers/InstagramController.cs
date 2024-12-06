using Instagram.Models;
using Instagram.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InstagramController : ControllerBase
    {
        [HttpGet("NotFollowingBack")]
        public async Task<List<UserResponseModel>> NotFollowingBack()
        {
            InstagramService instagramService = new InstagramService();
            return await instagramService.NotFollowingBackAsync();    
        }

        [HttpGet("FollowBack")]
        public async Task<List<UserResponseModel>> FollowBack()
        {
            InstagramService instagramService = new InstagramService();
            return await instagramService.FollowBackAsync();
        }
    }

}
