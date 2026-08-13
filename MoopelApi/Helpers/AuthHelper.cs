using MoopelObjects.Enums;
using MoopelObjects.Exceptions;

namespace MoopelApi.Helpers;

public static class AuthHelper
{
    public static void CanAccess(int userId, int? accessUserId, UserRole? accessUserRole, string @do, ILogger? logger = null)
    {
        if (accessUserId is null || accessUserRole is null)
        {
            throw new ForbiddenException($"Unknown User attempted to [{@do}] on user[{userId}]");
        }

        if (userId != accessUserId && accessUserRole != UserRole.Admin)
        {
            logger?.LogWarning("User[{UserId}] attempted to [{@do}] on user[{userId}]",
                    accessUserId, @do, userId);
            throw new ForbiddenException($"User[{accessUserId}] attempted to [{@do}] on user[{userId}]");
        }
    }
}
