using Identity.Application.DTOs.Users;
﻿using Identity.Application.DTOs;

namespace Identity.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserAsync(Guid userId);
    }
}
