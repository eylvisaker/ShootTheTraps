#!/bin/bash

version=$1
sourceDir=$2
destDir=$3

projectName=ShootTheTraps
projectRoot=`pwd`
tmpRoot=tmp
tmpDir="$tmpRoot/$projectName"

echo "Packaging $projectName v$version"
echo "Using source directory $sourceDir"
echo "and destination directory $destDir"

if [ -n "$version" ]; then
  version="-$version"
fi

mkdir -p $destDir
mkdir -p $tmpDir/lib

unzip -o "$sourceDir/${projectName}_Desktop${version}.zip" -d "$tmpDir/lib"

cp Linux/* $tmpDir

cd $tmpRoot

tar zcvf "$projectRoot/$destDir/${projectName}_Linux${version}.tar.gz" ./$projectName

