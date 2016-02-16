@echo off
setlocal EnableDelayedExpansion

set OutputPath=%1%
set SnExe=%2%
set KeyPairFile=%OutputPath%\dummy.snk
set PublicKeyFile=%KeyPairFile%.pub
set DetailsFile=%KeyPairFile%.details.txt
set InternalsVisibleToTemplateFile=%KeyPairFile%.InternalsVisibleTo.template.cs

:CreateOutoutPath
echo Creating directory '%OutputPath%'...
mkdir %OutputPath%

:FindSnExe
echo Using sn.exe from '%SnExe%'.

:GenerateKeyPair
echo Generating random valid public/private key pair into '%KeyPairFile%'...
%SnExe% -q -k %KeyPairFile%

:ExtractPublicKeyData
echo Extracting public key into '%PublicKeyFile%'...
%SnExe% -q -p %KeyPairFile% %PublicKeyFile%

:ExtractPublicKeyDetails
echo Storing human readable public key details in '%DetailsFile%'...
%SnExe% -q -tp %PublicKeyFile% > %DetailsFile%

:Success
echo.
echo Key pair creation successful. Public key details:
echo.
type %DetailsFile%
echo.
