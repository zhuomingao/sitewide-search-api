# Managing Best Bets Containers

**Scripts in this directory should be deployed together and on the path for the resource account responsbile for running the system.**

The Best Bets start up scripts each take a single parameter consisting of the name of a configuration
file. One of the values in the configuration files is the container name to use for that particular
instance. This allows multiple containers to run from a single image.

For example:
```bash
bestbets.api.sh bestbets.api.config.live
bestbets.api.sh bestbets.api.config.preview
```

## Scripts

* **halt-container.sh -** Halts the named container and removes it from memory.  
    ```./halt-container.sh bestbets-api-live```
* **pull-image.sh -** Pulls the named image from the Docker repository. Requires
    Docker login credentials.  
    ```./pull-image.sh nciwebcomm/bestbets-api:release <USERID> <PASSWORD>```
* **stop-api.sh -** Stops all instances of the API from running (Currently a hard-coded list).  
    ```./stop-api.sh```
* **stop-indexers.sh -** Prevents any new scheduled instances of the Best Bets indexer from starting.
    Does not return until all currently running instances have completed.  
    ```./stop-indexers.sh```
