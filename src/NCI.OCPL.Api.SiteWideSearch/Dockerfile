##
## This docker file is used for testing the application within a container
## and should *not* be used to create production containers.
##

FROM nciwebcomm/dotnet-docker:sdk

COPY . /app

WORKDIR /app

RUN ["dotnet", "restore"]

RUN ["dotnet", "build"]

EXPOSE 5000/tcp

CMD ["dotnet", "run", "--server.urls", "http://*:5000"]
