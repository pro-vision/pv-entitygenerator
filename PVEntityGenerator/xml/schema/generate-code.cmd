@echo off
"C:\Program Files (x86)\Microsoft Visual Studio 8\SDK\v2.0\Bin\xsd.exe" PVEntityGenerator-schema.xsd /classes /language:CS /namespace:PVEntityGenerator.XMLSchema /out:..\..\PVEntityGenerator\XMLSchema
