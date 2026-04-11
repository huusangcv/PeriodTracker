using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.Auth;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities;
using PeriodTracker.Entities.Framework;
using PeriodTracker.Services.Framework;


namespace PeriodTracker.Services.MainServices
{
    public interface IAuthService
    {
        Task<BaseResponse> ValidateAccess(AuthRequest request);
        Task<BaseResponse> TakeCurrentVersion();
    }

    public class AuthService(
        ILogService _logService,
        IEncryptionService _encryptionService,
        DataContext _dataContext,
        ISettingService _settingService
    ) : IAuthService
    {
        public Task<BaseResponse> TakeCurrentVersion()
        {
            var result = new BaseResponse();
            try
            {
                result.Data = _settingService.LoadSetting<SystemInfoSetting>().Version;
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logService.Error(ex);
                result.Message = ex.Message;
            }
            return Task.FromResult(result);
        }

        private sealed class SystemInfoSetting
        {
            public string Version { get; set; } = "1.0.0";
        }

        public async Task<BaseResponse> ValidateAccess(AuthRequest request)
        {
            var result = new BaseResponse { };
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username))
                {
                    result.Message = "[AuthService.Login] Error: 404. Request is null";
                    return result;
                }
                var userResponse = await Validate(request.Username, request.Password);
                if (userResponse.Success)
                {
                    var user = userResponse.Data as User;
                    int expiresIn = 24;

                    var userClaim = await _dataContext.UserClaims.FirstOrDefaultAsync(x => x.UserId == user!.Id);
                    #region AccessToken
                    var tokenAccess = JwtService.CreateToken(expiresIn, claims: new Dictionary<string, string>
                    {
                        { "UserId", user!.Id.ToString() },
                        { "UserName", user!.UserName },
                        { "UserDisplayName", user!.DisplayName },
                        { "Role", user?.Role ?? "" },
                        { "Permissions", userClaim?.Claims ?? ""},
                    });
                    #endregion

                    result.Success = true;
                    result.Data = new
                    {
                        AccessToken = tokenAccess,
                        TokenType = "bearer",
                        UserId = user.Id,
                        user.DisplayName,
                        user.UserName,
                        Email = user.Email,
                        ExpiredAt = DateTime.Now.AddHours(expiresIn),
                        PublicKey = Guid.NewGuid().ToString().ToLower(),
                    };
                }
                else
                {
                    result.Data = userResponse.Data;
                    result.Message = userResponse.Message;
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex);
                result.Message = ex.Message;
            }
            return result;
        }
        private async Task<BaseResponse> Validate(string userName, string password)
        {
            var result = new BaseResponse { };
            try
            {
                var user = await _dataContext.Users.FirstOrDefaultAsync(aa => aa.UserName == userName);
                if (user == null || !_encryptionService.IsValidPassword(user.Password, password, user.Salt))
                {
                    result.Message = "Tài khoản hoặc mật khẩu không đúng!";
                    return result;
                }
                if (!user.Active || user.Deleted)
                {
                    result.Message = "Tài khoản bị khoá!";
                    return result;
                }
                user.LastLoginAt = DateTime.Now;
                await _dataContext.SaveChangesAsync();
                result.Success = true;
                result.Data = user;
            }
            catch (Exception ex)
            {
                _logService.Error(ex);
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
