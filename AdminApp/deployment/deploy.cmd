set dst=%ustore%
set src=%~dp0..
set depl=%src%\..\uStoreCustomHtmlClearingPluginSetup\App\

xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginAdminApp.dll		%dst%\AdminApp\bin\
xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%dst%\AdminApp\bin\
xcopy /Y /D %src%\NoobClearingConfig.ascx								%dst%\AdminApp\ClearingModels\ClearingConfigControls\
xcopy /Y /D %src%\DataEntryConfig.ascx									%dst%\AdminApp\ClearingModels\UserDataCollection\

xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginAdminApp.dll		%depl%\AdminApp\bin\
xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%depl%\AdminApp\bin\
xcopy /Y /D %src%\NoobClearingConfig.ascx								%depl%\AdminApp\ClearingModels\ClearingConfigControls\
xcopy /Y /D %src%\DataEntryConfig.ascx									%depl%\AdminApp\ClearingModels\UserDataCollection\

rem pause