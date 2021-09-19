using Microsoft.AspNetCore.Mvc;

namespace Ridge
{
    internal interface IResultWrapper
    {
        public ActionResult GetInnerActionResult();
    }
}
