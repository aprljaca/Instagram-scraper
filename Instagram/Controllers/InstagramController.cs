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
        public async Task<List<User>> NotFollowingBack()
        {
            InstagramService instagramService = new InstagramService();
            return await instagramService.NotFollowingBackAsync();    

        }
    }

}
