SET riPath=I:\code\RelationsInspector
SET sourceArchivePath=%riPath%\Assets\RelationsInspector\Editor\SourceCodeRI.zip
SET msbuildPath=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
SET zipPath=C:\Program Files\7-Zip\7z
SET unityPath=C:\Program Files (x86)\Unity4.6.1\Editor\Unity.exe

REM compile the dll
call "%msbuildPath%" %riPath%\RIDLLProject\RelationsInspectorLib.csproj /target:Rebuild /property:Configuration=Release;DefineConstants="RELEASE"

REM zip the source code
call "%zipPath%" a -tzip %sourceArchivePath% %riPath%\RIDLLProject\* -xr!obj -xr!bin -xr!.vs -x!*.csproj -x!*.sln -x!*.suo -x!*.user -x!DemoRestriction.cs

REM export the package. the fist quote is the command window title
START "" "%unityPath%" -batchmode -projectPath %riPath% -executeMethod ExportPackage.DoExportPackage -quit

