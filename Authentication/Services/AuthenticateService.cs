using Authentication.Exceptions;
using Authentication.Models;
using Authentication.Models.Settings;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Services
{
    public class AuthenticateService
    {
        #region Deps

        private readonly AuthenticateServiceSettings _settings;
        private readonly UserManager<User> _userManager;

        #endregion

        #region Constructors

        public AuthenticateService(IOptions<AuthenticateServiceSettings> settings, UserManager<User> userManager)
        {
            this._settings = settings.Value;
            this._userManager = userManager;
        }

        #endregion

        public async Task SignUpAsync(string username, string email, string password)
        {
            var user = new User
            {
                UserName = username,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new AuthenticationException(result.Errors);
            }
        }

        public async Task<string> SignInAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new AuthenticationException("Username or Password are invalid.");
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, password);
            if (!validPassword)
            {
                throw new AuthenticationException("User or Password are invalid.");
            }

            var JWTAuthClaims = (await _userManager.GetRolesAsync(user)).Select(userRole =>
                new Claim(ClaimTypes.Role, userRole)
            )
            .Concat(new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            });

            var JWTSecrete = _settings.Secret;
            var JWTSecreteBytes = Encoding.UTF8.GetBytes(JWTSecrete);
            var JWTAuthSigningKey = new SymmetricSecurityKey(JWTSecreteBytes);
            var JWTSigningCredential = new SigningCredentials
            (
                JWTAuthSigningKey,
                SecurityAlgorithms.HmacSha256
            );

            var JWTExpires = DateTime.Now.AddHours(3);

            var JWTSecurityToken = new JwtSecurityToken
            (
                expires: JWTExpires,
                claims: JWTAuthClaims,
                signingCredentials: JWTSigningCredential
            );

            var token = new JwtSecurityTokenHandler()
                .WriteToken(JWTSecurityToken);

            return token;
        }

        public async Task<string> CreateResetPasswordTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new AuthenticationException("The provided email are not registered.");
            }

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return passwordResetToken;
        }

        public async Task ResetPasswordAsync(string email, string password, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new AuthenticationException("Invalid Token.");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                throw new AuthenticationException(result.Errors);
            }
        }
    }
}