@echo off

cd ../nginx
start nginx

cd ../nats
start nats-server

cd ./RankCalculator
start cmd /c dotnet run --urls "http://0.0.0.0:5003"

cd ../../Valuator
start cmd /c dotnet run --urls "http://0.0.0.0:5001"
start cmd /c dotnet run --urls "http://0.0.0.0:5002"

cd ../EventsLogger
start "" dotnet run
start "" dotnet run