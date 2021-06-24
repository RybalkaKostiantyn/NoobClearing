set dst=%ustore%
set src=%~dp0..
set depl=%src%\..\uStoreCustomHtmlClearingPluginSetup\App\

xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%dst%\AdminApp\bin\
xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%dst%\CustomerApp\bin\

xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%depl%\AdminApp\bin\
xcopy /Y /D %src%\bin\Debug\uStoreCustomHtmlClearingPluginLogicBase.dll		%depl%\CustomerApp\bin\

rem pause
