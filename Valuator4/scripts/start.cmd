@echo off
cd ../nginx
start nginx
cd ../nats
start nats-server
cd ./RankCalculator
start cmd /c dotnet run --urls "http://0.0.0.0:5003"
start cmd /c dotnet run --urls "http://0.0.0.0:5004"
cd ../../EventsLogger
start cmd /c dotnet run --urls "http://0.0.0.0:5005"
cd ../Valuator
start cmd /c dotnet run --urls "http://0.0.0.0:5001"
dotnet run --urls "http://0.0.0.0:5002"