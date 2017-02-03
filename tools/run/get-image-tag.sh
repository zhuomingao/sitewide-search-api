#!/bin/sh
# Retrieves a list of image names and tags matching a given image name
# Require parameter: image-name
if [ -z "$1" ]; then
    echo "Usage: ${0##*/} <image-name>"
    echo " where <image-name> is the name of the image you wish to find tags for."
    exit 1
fi
basename=$1

# Create array of image names with tags.
tagarray=($(docker images --format "{{.Repository}}:{{.Tag}}" $basename))

# Convert the array to a comma-separated list.
arraySize=${#tagarray[@]}
output=""
if [ $arraySize -gt 0 ]; then
    for item in "${tagarray[@]}";
    do
        if [ ! -z "$output" ]; then
            output="${output},${item}"
        else
            output="${item}"
        fi
    done
fi
echo $output