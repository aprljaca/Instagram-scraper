using Instagram.Models;
using Instagram.Services;
using Microsoft.AspNetCore.Mvc;

namespace Instagram.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InstagramController : ControllerBase
    {
        InstagramService instagramService;
        public InstagramController()    
        {
        instagramService = new InstagramService();
        }

        [HttpGet("NotFollowingBack")]
        public List<UserResponseModel> NotFollowingBack()
        {
            return instagramService.NotFollowingBackAsync();    
        }

        [HttpGet("FollowBack")]
        public List<UserResponseModel> FollowBack()
        {
            return instagramService.FollowBackAsync();
        }
    }

}
