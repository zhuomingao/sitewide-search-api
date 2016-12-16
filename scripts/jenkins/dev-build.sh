#!/bin/sh
# This is the template for a development (integration) build.
# Command Line Parameters:
#   $1 - Source path
#   $2 - Unit Test path

SRC_PATH=$1      # $1 - Source path
TEST_PATH=$2     # $2 - Unit Test path

echo Running Integration Build.
dotnet restore
dotnet build $TEST_PATH # Build unit test and dependencies.
dotnet test $TEST_PATH
