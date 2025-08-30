#!/bin/bash

# Build script for FoodprintApi

echo "🌱 Building Foodprint API..."

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build the project
echo "🔨 Building project..."
dotnet build --configuration Release --no-restore

# Run tests
echo "🧪 Running tests..."
dotnet test Tests/FoodprintApi.Tests.csproj --configuration Release --no-build --verbosity normal

# Check if tests passed
if [ $? -eq 0 ]; then
    echo "✅ All tests passed!"
    
    # Publish the application
    echo "📦 Publishing application..."
    dotnet publish --configuration Release --no-build --output ./publish
    
    echo "🎉 Build completed successfully!"
    echo "📁 Published files are in: ./publish"
    echo ""
    echo "🚀 To run the application:"
    echo "   dotnet ./publish/FoodprintApi.dll"
    echo ""
    echo "🐳 To build Docker image:"
    echo "   docker build -t foodprint-api ."
else
    echo "❌ Tests failed! Build aborted."
    exit 1
fi