namespace HabitTracker.Services;

// propagated back to client
// wonder if i'll have to change it much

public class UserVisibleException : Exception;
public class DuplicateUsernameException : UserVisibleException;
public class InvalidUsernameOrPasswordException : UserVisibleException;
