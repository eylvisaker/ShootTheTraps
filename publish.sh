#!/bin/bash

sourceDir=$1
remoteHost=$FTP_REMOTE_HOST
username=$FTP_USERNAME
password=$FTP_PASSWORD

echo "Uploading to ftp://$FTP_REMOTE_HOST/$FTP_DEST"
echo "Logging in as $FTP_USERNAME"

ftp-upload -h $FTP_REMOTE_HOST -u $FTP_USERNAME --password $FTP_PASSWORD -d $FTP_DEST $1/*
