set dst=%ustore%
set src=%~dp0..
set src1=%~1
set depl=%src%\..\uStoreNoobClearingPluginSetup\App

rem xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCustomerApp.dll		%dst%\CustomerApp\bin\
rem xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%dst%\CustomerApp\bin\
rem xcopy /Y /D %src%\DataEntry.ascx										%dst%\CustomerApp\Clearing\

echo %src1% > "textfile.txt"
xcopy /Y /D %src%\bin\ClientApp.dll										%depl%\CustomerApp\bin\
rem xcopy /Y /D %src%\bin\uStoreCustomHtmlClearingPluginCommonLogic.dll		%depl%\CustomerApp\bin\
xcopy /Y /D %src%\DataEntry.ascx										%depl%\CustomerApp\Clearing\

echo archive CustomHtmlClearing files
echo cd /D %src1%..\uStoreNoobClearingPluginSetup\App

echo cd /D %src1%..\uStoreNoobClearingPluginSetup

rem 7z a  -tzip %src%\..\uStoreNoobClearingPluginSetup\App.zip  "%src1%..\uStoreNoobClearingPluginSetup\App"

7z a -tzip "%src1%..\uStoreNoobClearingPluginSetup\App.zip" "%src1%..\uStoreNoobClearingPluginSetup\App"

echo %src1%..\uStoreNoobClearingPluginSetup

echo unlock hangged file
unlocker /S %src%\uStoreNoobClearingPluginSetup\App.zip

rem pause