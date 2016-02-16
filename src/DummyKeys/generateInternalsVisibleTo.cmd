@echo off
setlocal EnableDelayedExpansion

set TestAssemblyName=%1%
set DetailsFile=%2%
set OutputFile=%3%

:GenerateInternalsVisibleTo
echo Generating an InternalsVisibleTo file in '%OutputFile%'...
set PublickKey=
for /F "eol=P" %%l in (%DetailsFile%) do ( set PublickKey=!PublickKey!%%l )
set PublickKey=%PublickKey: =%
echo [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("%TestAssemblyName%, PublicKey=%PublickKey%")] > %OutputFile%
