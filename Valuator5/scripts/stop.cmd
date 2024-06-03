@echo off
cd ../nginx
nginx -s stop
taskkill /IM dotnet.exe /F
taskkill /IM nats-server.exe /F
taskkill /IM redis-server.exe /F