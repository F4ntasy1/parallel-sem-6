@echo off

start cmd /c redis-server ./rus.conf
start cmd /c redis-server ./eu.conf
start cmd /c redis-server ./other.conf
start cmd /c redis-server ./segmenter.conf

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
start cmd /c dotnet run --urls "http://0.0.0.0:5004"
start cmd /c dotnet run --urls "http://0.0.0.0:5005"