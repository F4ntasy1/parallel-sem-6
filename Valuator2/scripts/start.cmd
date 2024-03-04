@echo off
cd ../nginx
start nginx
cd ../Valuator
start cmd /c dotnet run --urls "http://0.0.0.0:5001"
dotnet run --urls "http://0.0.0.0:5002"