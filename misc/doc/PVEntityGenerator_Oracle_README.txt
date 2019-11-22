PVEntityGenerator for Oracle Install
------------------------------------

PVEntityGenerator Version 4.6 and above use the Oracle 11g "ODAC" .NET Providers with "XCopy" Deploy.

Installation Instructions:

1. Download Oracle 11g ODAC (XCopy Deploy)
http://download.oracle.com/otn/other/ole-oo4o/ODAC1110621Xcopy.zip

2. Extract ZIP File

3. Execute install batch file
>install.bat odp.net20 C:\Programme\Oracle_ODAC odac

first parameter installs only ODP.NET 2.x provider into the path (second parameter) and oracle home "odac" (third parameters).
other values for path and oracle home can be chosen.


see ODAC readme for full details
http://www.oracle.com/technology/docs/tech/windows/odpnet/odac11.1.0.6.21_ic_readme.txt
