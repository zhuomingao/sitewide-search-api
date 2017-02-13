#!/bin/sh
# Deploy to staging

########################################################################################
# 
# Required environment variables
# 
#   SERVER_LIST - comma separated list of servers to deploy to
#   RELEASE_VERSION - Version number to deploy (e.g 0.1.23)
#   SSH_USER - User id for SSH to deployment server
#   LIVE_API_HOST_PORT - Host port number to connect to the Live API instance
#   PREVIEW_API_HOST_PORT - Host port number to connect to the Preview API instance
# 
# Credentials for pulling the container image from the central repository.
#   (NOTE: This is an NCI internal repository, not Docker Hub)
#   DOCKER_USER - Userid for 
#   DOCKER_PASS - Password for pulling the container image from the central repository.
#   DOCKER_REGISTRY - Hostname of the NCI Docker registry.
# 
# Elastic Search credentials. Assumed to be the same for both live and preview.
#   ELASTICSEARCH_SERVERS - Comma separated list of ES servers.
#   ELASTICSEARCH_SEARCH_USER - User with read-only access
#   ELASTICSEARCH_SEARCH_PASSWORD - Password for search user.
# 
########################################################################################


export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export RUN_SCRIPTS="$( cd "${SCRIPT_PATH}" && cd "../run" && pwd )"
export RUN_LOCATION="sitewidesearch-run"
export OLD_RUN_LOCATION="${RUN_LOCATION}-"`date +%Y%m%d-%H%M`


# Check for required environment variables
if [ -z "$SERVER_LIST" ]; then echo "SERVER_LIST not set, aborting."; exit 1; fi
if [ -z "$RELEASE_VERSION" ]; then echo "RELEASE_VERSION not set, aborting."; exit 1; fi
if [ -z "$LIVE_API_HOST_PORT" ]; then echo "LIVE_API_HOST_PORT not set, aborting."; exit 1; fi
if [ -z "$PREVIEW_API_HOST_PORT" ]; then echo "PREVIEW_API_HOST_PORT not set, aborting."; exit 1; fi
if [ -z "$DOCKER_USER" ]; then echo "DOCKER_USER not set, aborting."; exit 1; fi
if [ -z "$DOCKER_PASS" ]; then echo "DOCKER_PASS not set, aborting."; exit 1; fi
if [ -z "$DOCKER_REGISTRY" ]; then echo "DOCKER_REGISTRY not set, aborting."; exit 1; fi
if [ -z "$ELASTICSEARCH_SERVERS" ]; then echo "ELASTICSEARCH_SERVERS not set, aborting."; exit 1; fi
if [ -z "$ELASTICSEARCH_SEARCH_USER" ]; then echo "ELASTICSEARCH_SEARCH_USER not set, aborting."; exit 1; fi
if [ -z "$ELASTICSEARCH_SEARCH_PASSWORD" ]; then echo "ELASTICSEARCH_SEARCH_PASSWORD not set, aborting."; exit 1; fi
if [ -z "$SSH_USER" ]; then echo "SSH_USER not set, aborting."; exit 1; fi


IFS=', ' read -r -a server_list <<< "$SERVER_LIST"

export IMAGE_NAME="${DOCKER_REGISTRY}/ocpl/sitewide-search-api"

api_instance_list=("live" "preview")

declare -A host_port
host_port['live']=$LIVE_API_HOST_PORT
host_port['preview']=$PREVIEW_API_HOST_PORT

# Deploy support script collection.
for server in "${server_list[@]}"
do
    echo "Copying run scripts to ${server}"
    ssh -q ${SSH_USER}@${server} "[ -e ${RUN_LOCATION} ] && mv ${RUN_LOCATION} ${OLD_RUN_LOCATION}" # Backup existing files.
    ssh -q ${SSH_USER}@${server} mkdir -p ${RUN_LOCATION}
    scp -q ${RUN_SCRIPTS}/* ${SSH_USER}@${server}:${RUN_LOCATION}
done

##################################################################
#   Per server steps.
##################################################################
for server in "${server_list[@]}"
do

    # Find out what images are already deployed for eventual cleanup.
    oldImageList=$(ssh -q ${SSH_USER}@${server} ${RUN_LOCATION}/get-image-tag.sh ${IMAGE_NAME})

    # Stop existing API container
    ssh -q ${SSH_USER}@${server} ${RUN_LOCATION}/stop-api.sh

    # Pull image for new version (pull version-specific tag)
    imageName="${IMAGE_NAME}:runtime-${RELEASE_VERSION}"
    ssh -q ${SSH_USER}@${server} ${RUN_LOCATION}/pull-image.sh $imageName $DOCKER_USER $DOCKER_PASS $DOCKER_REGISTRY

    # Create scripts for running each instance of the API (useful for manual restarts).
    for instance in "${api_instance_list[@]}"
    do
        echo "Starting $instance API instance"

        apiCommand="docker run --name sitewidesearch-api-${instance} \
            -d -p ${host_port[$instance]}:5008 \
            --restart always \
            -e Elasticsearch__Servers=\"${ELASTICSEARCH_SERVERS}\" \
            -e Elasticsearch__Userid=\"${ELASTICSEARCH_SEARCH_USER}\" \
            -e Elasticsearch__Password=\"${ELASTICSEARCH_SEARCH_PASSWORD}\" \
            ${imageName}"

        # Create and launch script for running the API
        scriptName="${RUN_LOCATION}/sitewidesearch-api-${instance}.sh"
        echo "${apiCommand}" | ssh -q ${SSH_USER}@${server} "cat > $scriptName \
&& chmod u+x $scriptName \
&& ./${scriptName}"

    done

#    Test API availability by performing a search with at least one result.
#
#    NOTE: This code is from the bestbets-api project and needs to be updated.
#
#    sleep 10 # Wait for the API to finish spinning up before querying.
#    testdata=$(curl -f --silent --write-out 'RESULT_CODE:%{http_code}' -XGET http://${server}:5006/bestbets/en/treatment)
#
#    statusCode=${testdata:${#testdata}-3}  # Get HTTP Status from end of output.
#    testdata=${testdata:0:${#testdata}-15} # Trim the HTTP Status from output
#    dataLength=${#testdata}
#
#    # Check for statusCode other than 200 or short testdata.
#    if [ "$statusCode" = "200" -a $dataLength -gt 100 -a "${testdata:0:1}" = "[" ]; then
#        echo "Successfully deployed to ${server}"
#        # All is well,
#        #   Remove old image
#        #   Continue on next server.
#    else
#        echo "Failed deploying to ${server}"
#        [ "$statusCode" != 200 ] && echo "HTTP status: ${statusCode}"
#        [ $dataLength -lt 101 ] && echo "Short data length (${dataLength})"
#        [ "${testdata:0:1}" = "[" ] && echo "Incorrect starting character, expected '[', got '${testdata:0:1}'."
#        echo "TestData '${testdata}'"
#        # Error:
#        #   Roll back to previous image
#        exit 1
#    fi

done
