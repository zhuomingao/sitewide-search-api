# sitewide-search-svc

## Configuration

The Sitewide Search Service uses a hierarchy of JSON files and environment variables to determine what configuration is
in effect. Settings in each layer replace any matching settings in the previous layer.  The layers are, in order,

1. appsettings.json
1. appsettings.${Environment}.json
1. Environment variables

For the **appsettings.${Environment}.json** file, value of the ASPNETCORE_ENVIRONMENT (e.g. Development, Staging, Production) is subsituted
for ${Environment}. These files are explicitly blocked in .gitignore in order to allow developers to have local json files containing passwords
and server names without worrying about accidentally pushing them to GitHub.

When using an environment variable to override a nested setting (e.g. The Servers property under elasticsearch), use __ (double underscore) to
mark the successive layers of nesting. (Technically, colon is also allowed, however colon is not an allowed character in environment variable
names under the bash shell.)

### Environment-specific Configuration Properties

**Elasticsearch**
```json
{
  "Elasticsearch" : {
    "Servers" : "http://localhost", // The Elasticsearch server
    "Userid" : "no-userid",         // Userid for authenticating 
    "Password" : "no-password"      // Password for authenticating      
  }
}
```
As environment variables

```bash
export Elasticsearch__Servers=http://localhost
export Elasticsearch__Userid=no-userid
export Elasticsearch__Passwordno-password
```
(Use either export or set, depending on the operating system.)

Reference: [Microsoft Configuration introduction](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)