set dst=%ustore%
set src=%~dp0..
set depl=%src%\..\uStoreNoobClearingPluginSetup\App\

rem xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%dst%\AdminApp\bin\
rem xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%dst%\CustomerApp\bin\

xcopy /Y /D %src%\bin\LogicBase.dll		%depl%\AdminApp\bin\
xcopy /Y /D %src%\bin\LogicBase.dll		%depl%\CustomerApp\bin\

rem pause
