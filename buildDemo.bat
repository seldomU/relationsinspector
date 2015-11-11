SET riPath=I:\code\RelationsInspector
SET msbuildPath=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
SET unityPath=C:\Program Files (x86)\Unity4.6.1\Editor\Unity.exe

REM compile the dll
call "%msbuildPath%" %riPath%\RIDLLProject\RelationsInspectorLib.csproj /target:Rebuild /property:Configuration=Release;DefineConstants="RELEASE;RIDEMO"

REM export the package. the fist quote is the command window title
START "" "%unityPath%" -batchmode -projectPath %riPath% -executeMethod ExportPackage.DoExportDemoPackage -quit