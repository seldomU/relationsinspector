SET riPath=I:\code\RelationsInspector
SET unityPath=C:\Program Files (x86)\Unity4.3.0\Editor\Unity.exe

REM export the package. the fist quote is the command window title
START "" "%unityPath%" -batchmode -projectPath %riPath% -executeMethod BuildPackage.Release -quit

