echo off
if [%1%]==[Debug] (
	nunit3-console %SOLUTION_DIR%\Debug\bin\WebServicesIntergrationTests.dll
)
if [%1%]==[Release] (
	nunit3-console %SOLUTION_DIR%\Release\bin\WebServicesIntergrationTests.dll
)