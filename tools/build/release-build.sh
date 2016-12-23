#!/bin/sh
# Build for release, and if all unit tests pass, create a GitHub release,
# upload the binaries, and build a Docker container.

# Required Enviroment Variables
# GH_ORGANIZATION_NAME - The GitHub organization (or username) the repository belongs to. 
# GH_REPO_NAME - The repository where the build should be created.
# VERSION_NUMBER - Semantic version number.
# PROJECT_NAME - Project name

export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export PROJECT_HOME="$(cd $SCRIPT_PATH/../.. && pwd)"
export TEST_ROOT=${PROJECT_HOME}/test
export CURDIR=`pwd`


echo Creating Release Build.

export GITHUB_TOKEN=$1  # Make GitHub security token available to release tool.


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

# Create and push SDK image
#docker build --build-arg version_number=${VERSION_NUMBER} -t nciwebcomm/sitewide-search-svc:sdk -f src/NCI.OCPL.Api.SiteWideSearch/Dockerfile/Dockerfile.SDK .

# Create and push Release image
#docker build --build-arg version_number=${VERSION_NUMBER} -t nciwebcomm/sitewide-search-svc:sdk -f src/NCI.OCPL.Api.SiteWideSearch/Dockerfile/Dockerfile.Release .
