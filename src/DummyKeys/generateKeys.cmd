@echo off
setlocal

set OutputPath=%1%
set SnExe=%2%
set KeyPairFile=%OutputPath%\dummy.snk
set PublicKeyFile=%KeyPairFile%.pub
set DetailsFile=%KeyPairFile%.details.txt

echo Creating directory '%OutputPath%'...
mkdir %OutputPath%

echo Using sn.exe from '%SnExe%'.

echo Generating random valid public/private key pair into '%KeyPairFile%'...
%SnExe% -q -k %KeyPairFile%

echo Extracting public key into '%PublicKeyFile%'...
%SnExe% -q -p %KeyPairFile% %PublicKeyFile%

echo Storing human readable public key details in '%DetailsFile%'...
%SnExe% -q -tp %PublicKeyFile% > %DetailsFile%

echo.
echo Key pair creation successful. Public key details:
echo.
type %DetailsFile%
echo.
