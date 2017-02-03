#!/bin/sh
# Utility script to stop and remove a container.
# Require parameter: container-name
if [ -z "$1" ]; then
    echo "Usage: ${0##*/} <container-name>"
    echo " where <container-name> is the name of the container you wish to stop."
    exit 1
fi

containerName=$1

# Is the container running?
containerID=$(docker ps --filter name=$containerName -q -a)
if [ $containerID ]; then
    docker stop $containerID
    docker rm $containerID
else
    echo "$containerName not found.  Nothing to do."
fi