using Application.Exceptions;
using Contract.DTOs;
using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
  
        public class AuthorizationService : IAuthorizeService
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;
           
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ILogger<AuthorizationService> _logger;
            private readonly ITokenHandler _tokenHandler;
             private readonly IUserRepository _userRepository;


        
            public AuthorizationService(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
             
                RoleManager<IdentityRole> roleManager,
                ILogger<AuthorizationService> logger,
                ITokenHandler tokenHandler,
                IUserRepository userRepository)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                
                _roleManager = roleManager;
                _logger = logger;
                _tokenHandler = tokenHandler;
                _userRepository = userRepository;
            }

            public async Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO)
            {
                if (registerDTO is null)
                    throw new NotNullExceptions("Can not be Null");

                ApplicationUser user = new ApplicationUser
                {
                    UserName = registerDTO.Email.Split("@")[0],
                    Email = registerDTO.Email,
                    PhoneNumber = registerDTO.PhoneNumber
                };


                var result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (!result.Succeeded)
                    return result;
                await _userManager.AddToRoleAsync(user, "User");

                _logger.LogInformation($"New user registered: {user.Email}");




                return result;
            }


          



            public async Task<string> LoginAsync(LoginDTO loginDTO)
            {
                var user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user == null)
                    throw new NotFoundException("User not found.");


                if (!user.EmailConfirmed)
                    throw new UnauthorizedException("Email təsdiqlənməyib! Zəhmət olmasa emailinizi təsdiqləyin.");

                var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User Login in: {user.Email}");
                    var token = await _tokenHandler.CreateAccessTokenAsync(user);
                    return token;

                }
                else
                {
                    _logger.LogWarning($"Invalid login attempt for user: {loginDTO.Email}");
                    throw new Exception("Invalid login attempt ");

                }
            }


            public async Task LogoutAsync()
            {

                await _signInManager.SignOutAsync();
            }

            public async Task<List<ApplicationUser>> GetAllUsersAsync()
            {
                var users = await _userRepository.GetAllUsersAsync();
                return users;
            }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User tapılmadı");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
        }

      
        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User tapılmadı");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result;
        }

    }
    }

