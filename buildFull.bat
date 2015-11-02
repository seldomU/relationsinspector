REM compile the dll
call "%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" RIDLLProject\RelationsInspectorLib.csproj /target:Rebuild /property:Configuration=Release;DefineConstants="RELEASE"
REM zip the source code
cd RIDLLProject
call "C:\Program Files\7-Zip\7z" a -tzip ..\SourceCodeRI.zip -xr!obj -xr!bin -xr!.vs -x!*.csproj -x!*.sln -x!*.suo -x!*.user -x!DemoRestriction.cs
cd..
REM move the zip into the unity project
copy SourceCodeRI.zip Assets\RelationsInspector\Editor\SourceCodeRI.zip
REM export the package. the fist quote is the command window title
START "" "C:\Program Files (x86)\Unity4.6.1\Editor\Unity.exe" -batchmode -projectPath I:\code\RelationsInspector -executeMethod ExportPackage.DoExportPackage -quit

