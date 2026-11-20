@echo off
rem Generates an ES2019 compatible version of MudBlazor, which is loaded for older Android versions.
rem 1) Compile the project for Android with the current MudBlazor NuGet.
rem 2) Extract the current version from <NuGetDirectory>\packages\mudblazor\<version>\staticwebassets\MudBlazor.min.js
rem 3) Execute this batch file, it creates the file src\SilentNotes.Blazor\wwwroot\MudBlazor.es2019.js

call npm install --save-dev @babel/core @babel/cli @babel/preset-env
call npx babel in\MudBlazor.min.js --config-file ./babel.config.json --out-file ..\SilentNotes.Blazor\wwwroot\MudBlazor.es2019.js

pause