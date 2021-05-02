using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApplication.ViewComponents
{
    [ViewComponent(Name = "Test")]
    public class TestViewComponent : ViewComponent
    {
        public TestViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(
            int maxPriority, bool isDone)
        {
            return await Task.FromResult(View((maxPriority, isDone)));
        }
    }
}
