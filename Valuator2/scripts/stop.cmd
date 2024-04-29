@echo off
cd ../nginx
nginx -s stop
taskkill /IM dotnet.exe /F