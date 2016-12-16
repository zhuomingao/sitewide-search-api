# Build Scripts
This is the collection of scripts needed for performing various CI tasks (test, build, tag, release) for the
site wide search project.

## Pre-requisites
* [Jenkins](https://jenkins.io/)
* [Job DSL Plugin](https://wiki.jenkins-ci.org/display/JENKINS/Job+DSL+Plugin)
* [Git Plugin](https://wiki.jenkins-ci.org/display/JENKINS/Git+Plugin)
* [Credentials Binding Plugin](https://wiki.jenkins-ci.org/display/JENKINS/Credentials+Binding+Plugin) (Included w/ Jenkins'
    recommended plugins)
* [EnvInject Plugin](https://wiki.jenkins-ci.org/display/JENKINS/EnvInject+Plugin)


## Files
* **setup.job** - Job definition file, written in the [Job DSL](https://wiki.jenkins-ci.org/display/JENKINS/Job+DSL+Plugin).
    This is the "Seed Job" responsible for installing the other jobs in Jenkins.
* **<TASK>.sh** - The shell script responsible for performing the work needed for a specific task.

## Installing the seed job.
1. Create a new FreeStyle job.
2. In the General section:
    * Specify a project name.
3. In the "Source Code Management" section:
  1. Select "Git"
  2. Specify the GitHub project (e.g. [https://github.com/NCIOCPL/sitewide-search-api](https://github.com/NCIOCPL/sitewide-search-api)).
  3. Verify the value for "Branch Specifier"
3. In the Build section:
  1. Click the "Add build step" button.
  2. Select "Process Job DSLs."
  3. Verify that "Look on Filesystem" is selected.
  4. For "DSL Scripts," specify "scripts/build-system/setup.job" (the actual script file will be downloaded automatically)
4. Click the "Save" button.

## GitHub Credentials
The jobs which build release executables and upload them to Github require an access token. This is a string which
is used in place of a password and should be treated as one.

1. Create an access token.
    1. Login to GitHub as the userid which will create the release.
    2. In the top *right* corner of any GitHub page, click the profile photo, then click Settings.
    3. In the "Developer Settings" block (left navigation), click "Personal access tokens."
    4. Click the "Generate new token" button.
    5. Enter a token description (e.g. "Access for release builds").
    6. Select the "repo" scope.
    7. At the bottom of the page, click the "Generate token" button.
    8. Copy the token value. **This is the only time the token will *ever* be visible in GitHub.**
2. Install GitHub credentials in Jenkins.
    1. Login to the Jenkins server.
    2. On the main page, click "Credentials."
    3. Click the "global" store (the linked word in any of the displayed lists).
    4. Click "Add Credentials" on the menu.
    5. Fill out the form:
        * **Kind:** select "Secret text"
        * **Scope:** Leave this as "Global"
        * **Secret:** The token value generated on GitHub.
        * **ID:** "NCIOCPL-Github" (this is the name for identifying the key).
        * **Description** Description of the key (e.g. "GitHub userid for creating releases"). 
    6. Click "Add Credentials" on the menu again
    7. Fill out the form:
        * **Kind:** select "Secret text"
        * **Scope:** Leave this as "Global"
        * **Secret:** The token value generated on GitHub.
        * **ID:** "NCIOCPL-Github-Token" (this is the name for identifying the key).
        * **Description** Description of the key (e.g. "GitHub token for creating releases"). 
