#!/bin/sh
# Encapsulate functionality for pushing an image to a Docker repository.
# Parameters
#  1 - Image name
#  2 - Tag name
IMAGE=$1
TAG=$2

if [[ "$IMAGE" = "" || "$TAG" = "" ]]; then
    echo "Usage: $0 <Image> <TAG>"
    exit 1
fi

docker push $IMAGE:$TAG
