set dst=%ustore%
set src=%~dp0
set src1=%~1
set depl=%src%\..\NoobClearingSetup\App

xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCustomerApp.dll		%dst%\CustomerApp\bin\
xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%dst%\CustomerApp\bin\
xcopy /Y /D %src%\DataEntry.ascx										%dst%\CustomerApp\Clearing\

xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCustomerApp.dll		%depl%\CustomerApp\bin\
xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%depl%\CustomerApp\bin\
xcopy /Y /D %src%\DataEntry.ascx										%depl%\CustomerApp\Clearing\

echo archive CustomHtmlClearing files
echo cd /D %src1%..\uStoreCustomHtmlClearingPluginSetup\App

echo cd /D %src1%..\uStoreCustomHtmlClearingPluginSetup

rem 7z a  -tzip %src%\..\uStoreCustomHtmlClearingPluginSetup\App.zip  "%src1%..\uStoreCustomHtmlClearingPluginSetup\App"

7z a -tzip "%src1%..\uStoreCustomHtmlClearingPluginSetup\App.zip" "%src1%..\uStoreCustomHtmlClearingPluginSetup\App"

echo %src1%..\uStoreCustomHtmlClearingPluginSetup

echo unlock hangged file
unlocker /S %src%\..\uStoreCustomHtmlClearingPluginSetup\App.zip

rem pause
