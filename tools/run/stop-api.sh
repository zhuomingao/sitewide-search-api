#!/bin/sh
# Stops API instances from running.
export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# TODO: Make this list configurable.
apiInstances=("sitewidesearch-api-live" "sitewidesearch-api-preview")
for container in "${apiInstances[@]}"
do
    $SCRIPT_PATH/halt-container.sh $container
done