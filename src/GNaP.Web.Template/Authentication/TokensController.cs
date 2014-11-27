namespace GNaP.Web.Template.Authentication
{
    using System.Web.Http;
    using Versioning.WebAPI;

    [RoutePrefix("api/tokens")]
    public class TokensController : ApiController
    {
        /// <summary>
        /// Requests a new authentication token (as JWT)
        /// </summary>
        /// <param name="credentials">User credentials to authenticate</param>
        /// <response code="200">JWT to use as Bearer token</response>
        /// <response code="400">Missing or invalid credentials</response>
        [VersionedRoute("")]
        [AllowAnonymous]
        public IHttpActionResult Post(Credentials credentials)
        {
            if (credentials == null)
                return BadRequest("Missing credentials.");

            // TOOD: Replace this in your production application!! Properly go to your authentication system
            if (!(credentials.Username == "gnap" && credentials.Password == "gnap"))
                return BadRequest("Invalid credentials.");

            var token = new JwtTokenService().GenerateToken(credentials.Username);

            return Ok(new { Token = token });
        }
    }

    public class Credentials
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}