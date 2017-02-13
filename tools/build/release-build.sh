#!/bin/sh
# Build for release, and if all unit tests pass, create a GitHub release,
# upload the binaries, and build a Docker container.

# Required Enviroment Variables
# GH_ORGANIZATION_NAME - The GitHub organization (or username) the repository belongs to. 
# GH_REPO_NAME - The repository where the build should be created.
# GITHUB_TOKEN -  GitHub security token for automated builds.
# VERSION_NUMBER - Semantic version number.
# PROJECT_NAME - Project name
# DOCKER_USERNAME - Docker login ID for publishing images
# DOCKER_PASSWORD - Docker password for publishing images
# DOCKER_REGISTRY - Hostname of the NCI Docker registry.

if [ -z "$GH_ORGANIZATION_NAME" ]; then echo GH_ORGANIZATION_NAME not set; exit 1; fi
if [ -z "$GH_REPO_NAME" ]; then echo GH_REPO_NAME not set; exit 1; fi
if [ -z "$GITHUB_TOKEN" ]; then echo GITHUB_TOKEN not set; exit 1; fi
if [ -z "$VERSION_NUMBER" ]; then echo VERSION_NUMBER not set; exit 1; fi
if [ -z "$PROJECT_NAME" ]; then echo PROJECT_NAME not set; exit 1; fi
if [ -z "$DOCKER_USERNAME" ]; then echo DOCKER_USERNAME not set; exit 1; fi
if [ -z "$DOCKER_PASSWORD" ]; then echo DOCKER_PASSWORD not set; exit 1; fi
if [ -z "$DOCKER_REGISTRY" ]; then echo DOCKER_REGISTRY not set; exit 1; fi

export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export PROJECT_HOME="$(cd $SCRIPT_PATH/../.. && pwd)"
export TEST_ROOT=${PROJECT_HOME}/test
export CURDIR=`pwd`

export IMAGE_NAME="${DOCKER_REGISTRY}/ocpl/sitewide-search-api"

echo Creating Release Build.

# Go to the project home foldder and restore packages
cd $PROJECT_HOME
echo Restoring packages
dotnet restore


# Build and run unit tests.
ERRORS=0
echo Executing unit tests
for test in $(ls -d ${TEST_ROOT}/*/); do
    dotnet test $test

    # Check for errors
    if [ $? != 0 ]; then
        export ERRORS=1
    fi
done

# If any unit tests failed, abort the operation.
if [ $ERRORS == 1 ]; then
    echo Errors have occured.
    exit 127
fi

# Publish individual projects to temporary location and create archives for uploading to GitHub
TMPDIR=`mktemp -d` || exit 1
dotnet publish src/NCI.OCPL.Api.SiteWideSearch/ -o $TMPDIR

# Creating the archive in the publishing folder prevents the parent directory being included
# in the archive.
echo "Creating release archive"
cd $TMPDIR
zip -r project-release.zip .
cd $PROJECT_HOME

## Create GitHub release with build artifacts.
echo "Creating release '${VERSION_NUMBER}' in github"
github-release release --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${VERSION_NUMBER}"

echo "Uploading the artifacts into github"
github-release upload --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${PROJECT_NAME}-${VERSION_NUMBER}.zip" --file $TMPDIR/project-release.zip

# Clean up
rm -rf $TMPDIR

docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD

# Create SDK Docker image
export IMG_ID=$(docker build -q --build-arg version_number=${VERSION_NUMBER} -t ${IMAGE_NAME}:sdk -t ${IMAGE_NAME}:sdk-${VERSION_NUMBER} -f src/NCI.OCPL.Api.SiteWideSearch/Dockerfile/Dockerfile.SDK .)
eval $SCRIPT_PATH/publish-docker-image.sh nciwebcomm/sitewide-search-api sdk
eval $SCRIPT_PATH/publish-docker-image.sh nciwebcomm/sitewide-search-api sdk-${VERSION_NUMBER}


# Create Release Docker image
export IMG_ID=$(docker build -q --build-arg version_number=${VERSION_NUMBER} -t ${IMAGE_NAME}:runtime -t ${IMAGE_NAME}:runtime-${VERSION_NUMBER} -f src/NCI.OCPL.Api.SiteWideSearch/Dockerfile/Dockerfile.Runtime .)
eval $SCRIPT_PATH/publish-docker-image.sh nciwebcomm/sitewide-search-api runtime
eval $SCRIPT_PATH/publish-docker-image.sh nciwebcomm/sitewide-search-api runtime-${VERSION_NUMBER}
