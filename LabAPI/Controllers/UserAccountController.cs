using Azure;
using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LabAPI.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class UserAccountController : ControllerBase
    {
        private readonly IOptions<DBSettings> dbSettings;
        private readonly JwtSettings jwtSettings;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        public UserAccountController(IOptions<DBSettings> _dbSettings, UserManager<IdentityUser> _userManager, SignInManager<IdentityUser> _signInManager, IOptionsMonitor<JwtSettings> _JwtOptions)
        {
            this.dbSettings = _dbSettings;
            this.userManager = _userManager;
            this.signInManager = _signInManager;
            this.jwtSettings = _JwtOptions.CurrentValue;
        }

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<ActionResult<ResponseObject<bool>>> RegisterUser(EX_UserRegister user)
        {
            ResponseObject<Boolean> result = new ResponseObject<Boolean>();

            try
            {
                var userAvailable = await this.userManager.FindByNameAsync(user.UserName);

                if (userAvailable != null)
                {
                    result.SetResponeData(false, ResultCode.Failure, "User Name already occupied.");
                    return StatusCode(StatusCodes.Status500InternalServerError, result);
                }

                // Implement custom password validation
                var passwordValidator = new PasswordValidator<IdentityUser>();
                var passwordValidationResult = await passwordValidator.ValidateAsync(this.userManager, null, user.Password);

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var error in passwordValidationResult.Errors)
                {
                    stringBuilder.Append(error.Description);

                }
                if (stringBuilder.Length > 0)
                {
                    result.SetResponeData(false, ResultCode.Failure, stringBuilder.ToString());
                    return StatusCode(StatusCodes.Status406NotAcceptable, result);
                }

                IdentityUser identityUser = new IdentityUser() { UserName = user.UserName, Email = user.Email, Id = user.UserID };

                var hasher = new PasswordHasher<IdentityUser>();
                var hashedPassword = hasher.HashPassword(identityUser, user.Password);
                identityUser.PasswordHash = hashedPassword;

                
                var data = await this.userManager.CreateAsync(identityUser);

                if (data.Succeeded)
                {
                    //await signInManager.SignInAsync(identityUser, isPersistent: false);

                    var result1 = await this.userManager.AddToRoleAsync(identityUser, user.Role);
                }   
                result.SetResponeData(true, ResultCode.Success, "User Registered Successfully");
            }
            catch (Exception ex)
            {
                result.SetResponeData(false, ResultCode.Failure, ex.Message);
                return StatusCode(StatusCodes.Status417ExpectationFailed, result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<ResponseObject<EX_TokenResult>>> Login(EX_Login user)
        {
            ResponseObject<EX_TokenResult> result = new ResponseObject<EX_TokenResult>();

            try
            {
                var userExists = await this.userManager.FindByNameAsync(user.UserName);

                if (userExists != null)
                {
                    var pwdCorrect = this.userManager.PasswordHasher.VerifyHashedPassword(userExists, userExists.PasswordHash, user.Password);

                    if (pwdCorrect==PasswordVerificationResult.Success)
                    {
                        var roles = await this.userManager.GetRolesAsync(userExists);
                        if(roles.Count > 0)
                        {
                            var jwtToken = GenerateJwtToken(userExists, roles.FirstOrDefault().ToString());
                            var data = new EX_TokenResult()
                            {
                                Status = true,
                                Token = jwtToken
                            };
                            result.SetResponeData(data, ResultCode.Success, "Token is Generated.");
                            return Ok(result);
                        }
                        else
                        {
                            var jwtToken = GenerateJwtToken(userExists, "Guest");
                            var data = new EX_TokenResult()
                            {
                                Status = true,
                                Token = jwtToken
                            };
                            result.SetResponeData(data, ResultCode.Success, "Token is Generated with Guest Role.");
                            return Ok(result);
                        }
                        
                    }
                    else
                    {
                        result.SetResponeData(null, ResultCode.Failure, "Password not match.");
                        return StatusCode(StatusCodes.Status401Unauthorized, result);
                    }
                }
                else
                {
                    result.SetResponeData(null, ResultCode.Failure, "User name not exists.");
                    return StatusCode(StatusCodes.Status401Unauthorized, result);
                }

            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.Message);
                return StatusCode(StatusCodes.Status417ExpectationFailed, result);
            }
        }

        private string GenerateJwtToken(IdentityUser user, string userRole)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(this.jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Id", user.Id),
                new Claim("Role", userRole),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // the JTI is used for our refresh token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
