# Combined Web Search
This app exposes a web API to perform combined search on multiple engines.


Requests to the `GET /api/v1/search` endpoint must be authenticated in order to work. Authentication is implemented using JWT Tokens.
## Run
```
dotnet run -p Bds.TechTest
```
## Test
```
dotnet test
```
## Single Page Application
There is a ReactJS single page application located at [Client App](./client-app).
```sh
cd ./client-app
npm start
```
## API Endpoints
### POST /api/v1/authorize/token
Requests must be encoded using the `multipart/form-data` content type.
|Param name|Type|Optional|Default|From|
|---|---|---|---|---|
|username|`string`|N|`null`|body|
|password|`string`|N|`null`|body|

### GET /api/v1/search
|Param name|Type|Optional|Default|From|
|---|---|---|---|---|
|term|`string`|N|`null`|query string|
|page|`int`|N|`null`|query string|
|pageSize|`int`|N|`null`|query string|

## Caveats
- At the moment only `Bing` and `Google` providers are supported
- The app fetches only the first few pages of results from the search engines.
- The requests to fetch data from the search engines happen in-thread during an HTTP request hence there might be occasional errors. A polling system where the server gets polled for the search status would be more adequate.
- Authentication is performed using hardcoded credentials. Environment variables or a secret manager would be a smarter solution.