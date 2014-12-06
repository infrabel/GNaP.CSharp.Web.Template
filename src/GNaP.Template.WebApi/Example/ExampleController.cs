namespace Template.Example
{
    using System.Collections.Generic;
    using System.Web.Http;
    using GNaP.WebApi.Versioning;

    [RoutePrefix("sample")]
    public class ExampleController : ApiController
    {
        [VersionedRoute("")]
        public IHttpActionResult Get()
        {
            return Ok(new List<string> { "Hello", "World" });
        }
    }
}