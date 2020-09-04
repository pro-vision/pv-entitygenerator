@echo off
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" PVEntityGenerator-schema.xsd /classes /language:CS /namespace:PVEntityGenerator.XMLSchema /out:..\..\XMLSchema
