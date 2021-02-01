using Microsoft.AspNetCore.Mvc;

namespace Ridge
{
    public interface IResultWrapper
    {
        public ActionResult GetInnerActionResult();
    }
}
