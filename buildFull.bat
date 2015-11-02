REM the fist quote is the command window title
call "%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" RIDLLProject\RelationsInspectorLib.csproj /target:Rebuild /property:Configuration=Release;DefineConstants="RELEASE"
START "" "C:\Program Files (x86)\Unity4.6.1\Editor\Unity.exe" -batchmode -projectPath I:\code\RelationsInspector -executeMethod ExportPackage.DoExportPackage -quit

