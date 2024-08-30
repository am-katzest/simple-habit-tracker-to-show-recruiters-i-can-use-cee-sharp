# Backend

Backend is an asp.net app of fairly standard architecture.

## DTOs

IDs used for communication between services and controllers are typed to reduce chance of certain mistakes and because a lot of them are composite (eg. `HabitId` contains `UserId`).

## Errors

when service layer encounters error it throws `UserVisibleException`, containing http error code and error code in a form of descriptive string. 
These Exceptions are caught by `backend/HabitTracker/Middleware/ErrorTranslation.cs` and appropriate http response is sent to client.

## Authentication/Authorization

In endpoints requiring authorization (all except creating new user) `backend/HabitTracker/Auth/Auth.cs` is used to determine if request's `Authorization` Header contains a valid session token matching some user, if it does `UserId` is attached to `HttpContext`.

`backend/HabitTracker/Auth/UserModelBindingFromContext.cs` contains a model binder extracting attached `UserId` for use by controllers.

`UserId` isn't part of api url, instead it is always accessed via this binder. 

## Multitenancy

To prevent users from accessing others' data despite "sharing" id "namespace" each id in chain (`CompletionId` -> `HabitId` -> `UserId`) is verified.

## Tests

Most tests depend on postgresql docker container (see `backend/HabitTracker.Tests/DbContainer`) to run.

A lot of tests are duplicated between controller and service layers, should probably fix that with a spy mock...

to run backend tests:

``` sh
cd backend
dotnet test
```
