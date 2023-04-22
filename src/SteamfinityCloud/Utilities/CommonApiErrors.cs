using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Models;

namespace Steamfinity.Cloud.Utilities;

public static class CommonApiErrors
{
    public static ObjectResult AccessDenied => ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not authorized to perform this operation.");

    public static ObjectResult CurrentUserNotFound => ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "The account of the currently authenticated user has been deleted.");

    public static ObjectResult UserNotFoundById => ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "The user with the provided identifier does not exist.");

    public static ObjectResult UserNotFoundByName => ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "The user with the provided username does not exist.");

    public static ObjectResult DuplicateUserName => ApiError(StatusCodes.Status409Conflict, "DUPLICATE_USERNAME", "The provided username is already taken by another user.");

    public static ObjectResult InvalidUserName => ApiError(StatusCodes.Status400BadRequest, "INVALID_USERNAME", "The provided username is either too short, too long, or contains illegal characters.");

    public static ObjectResult AdministratorSignUpKeyNotConfigured => ApiError(StatusCodes.Status401Unauthorized, "ADMINISTRATOR_SIGN_UP_KEY_NOT_CONFIGURED", "The administrator sign-up key has not been configured in the application settings.");

    public static ObjectResult IncorrectAdministratorSignUpKey => ApiError(StatusCodes.Status401Unauthorized, "INCORRECT_ADMINISTRATOR_SIGN_UP_KEY", "The provided administrator sign-up key is incorrect.");

    public static ObjectResult UserLockedOut => ApiError(StatusCodes.Status403Forbidden, "USER_LOCKED_OUT", "The user account has been temporarily locked due to multiple failed sign-in attempts.");

    public static ObjectResult SignInNotAllowed => ApiError(StatusCodes.Status403Forbidden, "SIGN_IN_NOT_ALLOWED", "You are not allowed to sign in.");

    public static ObjectResult UserSuspended => ApiError(StatusCodes.Status403Forbidden, "USER_SUSPENDED", "Your user account has been suspended by the administrator.");

    public static ObjectResult PasswordTooWeak => ApiError(StatusCodes.Status400BadRequest, "PASSWORD_TOO_WEAK", "The provided password does not meet the minimum security requirements.");

    public static ObjectResult IncorrectPassword => ApiError(StatusCodes.Status401Unauthorized, "INCORRECT_PASSWORD", "The provided password is incorrect.");

    public static ObjectResult InvalidCredentials => ApiError(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "The provided username and password do not match.");

    public static ObjectResult InvalidToken => ApiError(StatusCodes.Status401Unauthorized, "INVALID_TOKEN", "The provided token has expired or is invalid.");

    public static ObjectResult RoleNotFound => ApiError(StatusCodes.Status404NotFound, "ROLE_NOT_FOUND", "The role with the provided name does not exist.");

    public static ObjectResult UserNotInRole => ApiError(StatusCodes.Status400BadRequest, "USER_NOT_IN_ROLE", "The user is not a member of this role.");

    public static ObjectResult UserAlreadyInRole => ApiError(StatusCodes.Status400BadRequest, "USER_ALREADY_IN_ROLE", "The user has already been added to this role.");

    public static ObjectResult LibraryLimitExceeded => ApiError(StatusCodes.Status403Forbidden, "LIBRARY_LIMIT_EXCEEDED", "The user is already a member of the maximum number of libraries.");

    public static ObjectResult LibraryNotFound => ApiError(StatusCodes.Status404NotFound, "LIBRARY_NOT_FOUND", "The library with the provided identifier does not exist.");

    public static ObjectResult MemberLimitExceeded => ApiError(StatusCodes.Status403Forbidden, "MEMBER_LIMIT_EXCEEDED", "The library has already reached the maximum number of members.");

    public static ObjectResult AccountLimitExceeded => ApiError(StatusCodes.Status403Forbidden, "ACCOUNT_LIMIT_EXCEEDED", "The library has already reached the maximum number of accounts.");

    public static ObjectResult UserNotLibraryMember => ApiError(StatusCodes.Status400BadRequest, "USER_NOT_MEMBER", "The user is not a member of the library.");

    public static ObjectResult AccountNotFound => ApiError(StatusCodes.Status404NotFound, "ACCOUNT_NOT_FOUND", "The account with the provided identifier does not exist.");

    public static ObjectResult InvalidSteamId => ApiError(StatusCodes.Status400BadRequest, "INVALID_STEAM_ID", "The provided Steam ID is invalid.");

    public static ObjectResult HashtagLimitExceeded => ApiError(StatusCodes.Status403Forbidden, "HASHTAG_LIMIT_EXCEEDED", $"The account has already reached the maximum number of hashtags.");

    public static ObjectResult InvalidHashtags => ApiError(StatusCodes.Status400BadRequest, "INVALID_HASHTAGS", "At least one of the provided hashtags is invalid.");

    public static ObjectResult InvalidTransfer => ApiError(StatusCodes.Status400BadRequest, "INVALID_TRANSFER", "The account cannot be moved to the same library that it is currently in.");

    private static ObjectResult ApiError(int statusCode, string errorCode, string? message = null)
    {
        var apiError = new ApiError(errorCode, message);
        var result = new ObjectResult(apiError)
        {
            StatusCode = statusCode
        };

        return result;
    }
}
