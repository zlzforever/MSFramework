#!/bin/bash
export NUGET_SERVER=$PT_NUGET
export NUGET_KEY=$PT_GITEA_NUGET_KEY
sh publish.sh
