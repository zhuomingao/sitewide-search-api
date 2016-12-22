#!/bin/sh
# This is the template for a release build.
# Command Line Parameters:
#   $1 - Source path
#   $2 - Unit Test path
#   $3 - GitHub token for creating a release and uploading artifacts.

# Required Enviroment Variables
# GH_ORGANIZATION_NAME - The GitHub organization (or username) the repository belongs to. 
# GH_REPO_NAME - The repository where the build should be created.
# VERSION_NUMBER - Semantic version number.
# PROJECT_NAME - Project name

echo Creating Release Build.

SRC_PATH=$1      # $1 - Source path
TEST_PATH=$2     # $2 - Unit Test path
export GITHUB_TOKEN=$3  # Make GitHub security token available to release tool.


# Create temporary location for publishing output
TMPDIR=`mktemp -d` || exit 1

# Main project directory
PROJECT_DIR=`pwd`

# Publish to temporary location.
dotnet restore
dotnet build $TEST_PATH # Build unit test and dependencies.
dotnet test $TEST_PATH
dotnet publish -o $TMPDIR

# Create archive for uploading to GitHub.  Creating it in the
# Publishing folder eliminates the parent directory path.
echo "Creating release archive"
cd $TMPDIR
zip -r project-release.zip .
cd $PROJECT_DIR

echo "Creating a new release in github"
github-release release --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${VERSION_NUMBER}"

echo "Uploading the artifacts into github"
github-release upload --user ${GH_ORGANIZATION_NAME} --repo ${GH_REPO_NAME} --tag ${VERSION_NUMBER} --name "${PROJECT_NAME}-${VERSION_NUMBER}.zip" --file $TMPDIR/project-release.zip

# Clean up
rm -rf $TMPDIR