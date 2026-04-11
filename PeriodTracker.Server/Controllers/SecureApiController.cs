using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.Hemisphere.Server.Middlewares;

namespace Phoenix.Hemisphere.Server.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SecureApiController : ControllerBase
    {
    }

    [SecureAuthorize]
    public class SecureVer2Controller : SecureApiController
    {
    }
}
