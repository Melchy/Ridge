using System.Net.Http;
using System.Threading.Tasks;

namespace Ridge.Middlewares.Public
{
    public delegate Task<HttpResponseMessage> CallMiddlewareDelegate();
}
