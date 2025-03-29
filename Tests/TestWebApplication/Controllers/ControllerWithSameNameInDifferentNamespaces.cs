using Microsoft.AspNetCore.Mvc;
using Ridge.AspNetCore.GeneratorAttributes;

namespace TestWebApplication.Controllers
{
    [GenerateClient]
    [Route("SameName1")]
    public class ControllersWithSameName
    {
        [HttpGet]
        public int AndWithSameMethod()
        {
            return 1;
        }
    }
}

namespace TestWebApplication.Controllers2
{
    [GenerateClient]
    [Route("SameName2")]
    public class ControllersWithSameName
    {
        [HttpGet]
        public int AndWithSameMethod()
        {
            return 2;
        }
    }
}
