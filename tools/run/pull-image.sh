#!/bin/sh
# Downloads a specific docker image from the central repository
IMAGE_NAME=$1
DOCKER_USER=$2
DOCKER_PASS=$3
if [ -z "$IMAGE_NAME" ]; then echo "Argument 1 must be the image name to retrieive, aborting."; exit 1; fi
if [ -z "$DOCKER_USER" ]; then echo "Argument 2 must be the docker repository userid, aborting."; exit 1; fi
if [ -z "$DOCKER_PASS" ]; then echo "Argument 3 must be the docker repository password, aborting."; exit 1; fi

docker login -u $DOCKER_USER -p $DOCKER_PASS

# Future: If docker adds a --quiet switch to pull, this would be a good place to use it.
# See issue 13588: https://github.com/docker/docker/issues/13588
# Sending the output to /dev/null is an available option, but it comes with the downside of losing
# error messages at the same time.
docker pull $IMAGE_NAME
