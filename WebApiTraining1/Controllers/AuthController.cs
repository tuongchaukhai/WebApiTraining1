using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiTraining1.Models;
using WebApiTraining1.ViewModels;

namespace WebApiTraining1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly MyDbContext _context;
        private readonly AppSetting _appSetting;
        private readonly ILogger<AuthController> _logger;
        public AuthController(MyDbContext context, IOptionsMonitor<AppSetting> optionsMonitor, ILogger<AuthController> logger)
        {
            _context = context;
            _appSetting = optionsMonitor.CurrentValue;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Validate(LoginDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidOperationException();
                }

                var user = _context.Users.Include(x => x.Role).SingleOrDefault(x => x.Email == request.Email);

                if (user == null)
                {
                    throw new Exception("This user doesn't exist");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    throw new Exception("Invalid password");
                }

                var token = await GenerateToken(user);

                _logger.LogInformation("An account with email: {email} has just been logged at {datetime}, token user: {token}", request.Email, DateTime.Now.ToString(), token.AccessToken);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Authenticate Success",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = true,
                    Message = ex.Message
                });
            }
        }


        [HttpPost("register")]
        public ActionResult Register(RegisterDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidOperationException();
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Email = request.Email,
                    Password = passwordHash,
                    FullName = request.FullName,
                    RoleId = 2 //set default is a customer
                };

                if (user == null)
                {
                    throw new Exception("This user does not exist");
                }

                _context.Add(user);
                _context.SaveChanges();

                _logger.LogInformation("An account with email: {email} has just been registed at {datetime}", request.Email, DateTime.Now.ToString());
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Sign up success"

                });
            }
            catch(Exception ex)
            {
                //_logger.LogError("Resigter's error: {ex}", ex.Message);
                return BadRequest(new ApiResponse
                {
                    Success = true,
                    Message = ex.Message
                });
            }
        }

        private async Task<TokenModel> GenerateToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSetting.SecretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FullName", user.FullName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.Role.RoleName),
                    new Claim("Id", user.UserId.ToString()),

                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey
                (secretKeyBytes), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            //Save the refreshtoken into the DB
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                JwtId = token.Id,
                Token = refreshToken,
                IsUsed = false,
                IsRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(1),
            };

            await _context.AddAsync(refreshTokenEntity);
            _context.SaveChanges();

            SetJWTCookie(accessToken); //Set token in a cookie

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }

        private void SetJWTCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(1),
            };
            Response.Cookies.Append("jwtCookie", token, cookieOptions);
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSetting.SecretKey);
            //Validate the token before renewing a new one
            var tokenValidateParam = new TokenValidationParameters
            {
                //Copy form the program.cs
                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                ClockSkew = TimeSpan.Zero,

                //because during renewing, we only need to check if the key is valid, so there is no need to check if it has expired
                //to avoid the situation where the token has expired and jumps to the catch block
                //default is true
                ValidateLifetime = false
            };
            try
            {
                //Check accesstoken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);

                //Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals
                        (SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return Ok(new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid Token"
                        });
                    }
                }

                //Check Access Token expired
                var expireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var dateConverted = ConvertUnixTimeToDateTime(expireDate);
                if (dateConverted > DateTime.UtcNow) //còn hạn
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired"
                    });
                }

                //Check refresh token exist in DB
                var storedToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == model.RefreshToken);
                if (storedToken == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token doesn't exist"
                    });
                }

                //Check refresh token used/revoked
                if (storedToken.IsUsed)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token is used"
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token is revoked"
                    });
                }

                //Check accesstoken id == JwtId in refreshtoken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type ==
                 JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Token doesn't matched in stored"
                    });
                }

                //Update token is used
                storedToken.IsRevoked = true;
                storedToken.IsUsed = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();

                //create new token
                var user = await _context.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.UserId == storedToken.UserId);
                var token = await GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Renew Success",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Something went wrong"
                });
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long unixTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTime = unixEpoch.AddSeconds(unixTime).ToUniversalTime();
            return dateTime;
        }
    }
}
