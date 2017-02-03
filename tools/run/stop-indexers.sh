#!/bin/sh
# Stops indexers from running. When this script exists, no indexers are running
# and the crontab file has been removed.

export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Stop new indexer instances from launching.
# Note: The user running this script must be included in cron.allow
crontab -r


# Wait for existing indexer instances to finish.
# TODO: Make this list configurable.
indexers=("bestbets.indexer.live" "bestbets.indexer.preview")
for container in "${indexers[@]}"
do
    containerID=$(docker ps --filter name=$container -q -a)
    while [ "$containerID" != "" ]
    do
        echo "Waiting for ${container}"
        sleep 30
        containerID=$(docker ps --filter name=$container -q -a)
    done
done
