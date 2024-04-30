FROM mcr.microsoft.com/dotnet/sdk:8.0 as sdk
RUN mkdir /opt/build
COPY ./HabitTracker /opt/build
WORKDIR /opt/build
RUN dotnet build -o /opt/build/sht
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as asp
COPY --from=sdk /opt/build/sht /opt/app
CMD /opt/app/HabitTracker
