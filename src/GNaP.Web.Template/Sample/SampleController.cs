namespace GNaP.Web.Template.Sample
{
    using System.Collections.Generic;
    using System.Web.Http;
    using Versioning.WebAPI;

    [RoutePrefix("sample")]
    public class SampleController : ApiController
    {
        [VersionedRoute("")]
        public IHttpActionResult Get()
        {
            return Ok(new List<string> { "Hello", "World" });
        }
    }
}