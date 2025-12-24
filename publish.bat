@echo off
setlocal

set "PUBLISH_DIR=%~dp0publish\portable"
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"

echo Publishing TreeOverlay (self-contained win-x64)...
dotnet publish "TreeOverlay\TreeOverlay.csproj" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%" /p:PublishSingleFile=true

echo Portable build output: %PUBLISH_DIR%
endlocal
