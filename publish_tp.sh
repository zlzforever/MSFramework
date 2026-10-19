#!/bin/bash
export NUGET_SERVER=$TP_NUGET
export NUGET_KEY=$TP_GITEA_NUGET_KEY
sh publish.sh
