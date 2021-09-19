using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TestWebApplication.ViewComponents
{
    [ViewComponent(Name = "Test")]
    public class TestViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(
            int maxPriority,
            bool isDone)
        {
            return await Task.FromResult(View((maxPriority, isDone)));
        }
    }
}
