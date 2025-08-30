@echo off
REM Build script for FoodprintApi (Windows)

echo 🌱 Building Foodprint API...

REM Restore dependencies
echo 📦 Restoring dependencies...
dotnet restore
if %ERRORLEVEL% neq 0 goto :error

REM Build the project
echo 🔨 Building project...
dotnet build --configuration Release --no-restore
if %ERRORLEVEL% neq 0 goto :error

REM Run tests
echo 🧪 Running tests...
dotnet test Tests/FoodprintApi.Tests.csproj --configuration Release --no-build --verbosity normal
if %ERRORLEVEL% neq 0 goto :test_error

REM Publish the application
echo 📦 Publishing application...
dotnet publish --configuration Release --no-build --output ./publish
if %ERRORLEVEL% neq 0 goto :error

echo ✅ Build completed successfully!
echo 📁 Published files are in: ./publish
echo.
echo 🚀 To run the application:
echo    dotnet ./publish/FoodprintApi.dll
echo.
echo 🐳 To build Docker image:
echo    docker build -t foodprint-api .
goto :end

:test_error
echo ❌ Tests failed! Build aborted.
exit /b 1

:error
echo ❌ Build failed!
exit /b 1

:end