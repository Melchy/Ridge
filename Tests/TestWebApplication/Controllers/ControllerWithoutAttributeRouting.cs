using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ridge.Results;

namespace TestWebApplication.Controllers
{
    public class ControllerWithoutAttributeRouting : ControllerBase
    {
        public virtual async Task<ControllerResult> HttpGetWithoutBody()
        {
            return await Task.FromResult(Ok());
        }
    }
}
