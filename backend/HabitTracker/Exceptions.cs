using System.Net;
using static System.Net.HttpStatusCode;
namespace HabitTracker.Exceptions;

// propagated back to client
public class UserVisibleException(HttpStatusCode code, string errorMessage) : Exception
{
    public readonly HttpStatusCode Code = code;
    public readonly String ErrorMessage = errorMessage;
};

public class DuplicateUsernameException() : UserVisibleException(Conflict, "duplicate username");

public class InvalidUsernameOrPasswordException() : UserVisibleException(Unauthorized, "invalid username or password");

public class InvalidTokenException() : UserVisibleException(Unauthorized, "invalid token");

public class NoSuchHabitException() : UserVisibleException(NotFound, "habit not found");
