#!/bin/sh
# Development (integration) build.

export SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export PROJECT_HOME="$(cd $SCRIPT_PATH/../.. && pwd)"
export TEST_ROOT=${PROJECT_HOME}/test
export CURDIR=`pwd`


echo Running Integration Build.

# Go to the project home foldder and restore packages
cd $PROJECT_HOME
echo Restoring packages
dotnet restore

# Build and run unit tests.
echo Executing unit tests
for test in $(ls -d ${TEST_ROOT}/*/); do
    dotnet test $test
done

# Put things back the way we found them.
cd $CURDIR