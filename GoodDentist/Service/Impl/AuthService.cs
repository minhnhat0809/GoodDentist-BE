﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BusinessObject;
using BusinessObject.DTO;
using BusinessObject.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;

namespace Services.Impl;

public class AuthService : IAuthService
{
    private const int saltSize = 128 / 8;
    private const int keySize = 256 / 8;
    private const int iterations = 10000;
    private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
    
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ResponseLoginDTO _responseLogin;
    private readonly ResponseDTO _responseDto;

    public AuthService(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _responseLogin = new ResponseLoginDTO();
        _responseDto = new ResponseDTO("",200,true,null);
    }

    public async Task<ResponseLoginDTO> Authenticate(LoginDTO loginDto)
    {
        
        try
        {
            // check if account exist
            var existAccount =  _unitOfWork.userRepo.getUser(loginDto.UserName);
            if (existAccount != null)
            {
                // hash and salt
                byte[] inputHashPassword = this.hashPassword(loginDto.Password, existAccount.Salt);
                // input password with existAccount password
                if (inputHashPassword.SequenceEqual(existAccount.Password))
                {
                    // get token
                    _responseLogin.AccessToken = GenerateJwtToken(existAccount);
                }
                else
                {
                    _responseLogin.Message = "Password is not correct!";
                    _responseLogin.IsSuccess = false;
                }
            }
            else
            {
                _responseLogin.Message = "Username is not correct!";
                _responseLogin.IsSuccess = false;
            }
        }
        catch (Exception e)
        {
            _responseLogin.Message = e.Message;
            _responseLogin.IsSuccess = false;
        }

        return _responseLogin;
    }

    public async Task<ResponseDTO> GetAccountByEmailOrPhone(string emailOrPhone)
    {
        
        try
        {
            // check if account exist
            var existAccount = await _unitOfWork.userRepo.FindByConditionAsync(ac => ac.Email == emailOrPhone || ac.PhoneNumber == emailOrPhone);
            if (existAccount != null)
            {
                _responseDto.Result = existAccount;
            }
            else
            {
                _responseDto.Message = "Account is not found!";
                _responseDto.IsSuccess = false;
            }
        }
        catch (Exception e)
        {
            _responseDto.Message = e.Message;
            _responseDto.IsSuccess = false;
        }

        return _responseDto;
    }

    public Task<ResponseLoginDTO> ResetPassword(Guid userId, string password)
    {
        throw new NotImplementedException();
    }

    public async Task<ResponseDTO> GetUserAsync(Guid userId)
    {
        try
        {
            _responseDto.Result = await _unitOfWork.userRepo.GetByIdAsync(userId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return _responseDto;
    }


    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, GenerateRole(user.RoleId.Value))
        };

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private byte[] hashPassword(string password, byte[] salt)
    {
        var hashedPassword = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);

        var pwdString = string.Join(Convert.ToBase64String(salt), Convert.ToBase64String(hashedPassword));
        return Convert.FromBase64String(pwdString);
    }

    private byte[] salting()
    {
        return RandomNumberGenerator.GetBytes(saltSize);
    }

    private string GenerateRole(int roleId)
    {
        var existRole = _unitOfWork.roleRepo.GetRole(roleId).Result;
        if (existRole == null)
        {
            _responseLogin.Message = "Role is not found!";
            _responseLogin.IsSuccess = false;
        }

        return existRole.RoleName;
    }
    
}