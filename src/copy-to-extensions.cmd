@echo off

set sourceDir=%1
set SolutionDir=%2

set targetDir=%SolutionDir%..\test-1\LANDIS-II\extensions\

echo %targetDir%

set files=Landis.Extension.SosielHuman.dll CsvHelper.dll Newtonsoft.Json.dll WhatsNew.txt
 
(for %%a in (%files%) do ( 
   xcopy /Q /Y "%sourceDir%\%%a" "%targetDir%"
))

pause

