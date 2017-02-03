#!/bin/sh
# Stops API instances from running.
export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# TODO: Make this list configurable.
apiInstances=("bestbets-api-live" "bestbets-api-preview")
for container in "${apiInstances[@]}"
do
    $SCRIPT_PATH/halt-container.sh $container
done