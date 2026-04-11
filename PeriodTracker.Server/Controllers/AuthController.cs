using Microsoft.AspNetCore.Mvc;
using PeriodTracker.ApiGateways.Auth;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Services.MainServices;

namespace Phoenix.Hemisphere.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService _authServices) : ControllerBase
    {
        [HttpPost("validate-access")]
        public async Task<BaseResponse> ValidateAccess([FromBody] AuthRequest request) => await _authServices.ValidateAccess(request);
        [HttpPost("take-current-version")]
        public async Task<BaseResponse> TakeCurrentVersion() => await _authServices.TakeCurrentVersion();
    }
}
