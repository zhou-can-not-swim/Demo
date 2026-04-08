@echo off
echo Installing Playwright CLI...
dotnet tool install --global Microsoft.Playwright.CLI

echo Installing Playwright browsers...
playwright install

echo Done!
pause