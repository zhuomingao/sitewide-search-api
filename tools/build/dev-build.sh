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

# Put things back the way we found them.
cd $CURDIR
