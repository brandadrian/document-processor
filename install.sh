#!/bin/bash

if [ -z "$1" ];
then
  echo ----
  echo No version specified! Please specify a valid version like 1.2.3 or 1.2.3-rc1!
  exit 1
fi

echo ----
echo Starting building version $1

echo ----
echo Cleaning up
rm -r ./artifacts
dotnet tool uninstall -g DocumentProcessor

echo ----
echo Restore solution
dotnet restore src/DocumentProcessor

echo ----
echo Packaging solution with Version = $1
dotnet pack src/DocumentProcessor -c Release -p:PackageVersion=$1 -p:Version=$1 -o ./artifacts/nupkgs/

echo ----
echo Installing DocumentProcessor globally with Version = $1
dotnet tool install --global --add-source ./artifacts/nupkgs/ DocumentProcessor

echo ----
echo Done