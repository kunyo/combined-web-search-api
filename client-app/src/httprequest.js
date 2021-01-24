export class HttpRequestError extends Error {
    constructor(message = "HTTP request failed") { super(message) }
  }
  export class AuthorizationRequiredError extends HttpRequestError {
    constructor(message = "Authorization required") { super(message) }
  }
  export class CredentialNotFoundError extends HttpRequestError {
    constructor(message = "Credentials not found") { super(message) }
  }
  export class UnexpectedStatusCodeError extends HttpRequestError {
    constructor(message = "Unexpected HTTP status code") { super(message) }
  }
  
  export class HttpResponse {
    constructor(responseObj) {
      this._responseObj = responseObj
      this.headers = responseObj.headers
      this.status = responseObj.status
    }
  
    async assertStatus(...acceptedCodes) {
      if (acceptedCodes.indexOf(this._responseObj.status) === -1) {
        throw new UnexpectedStatusCodeError()
      }
      return Promise.resolve(this)
    }
    blob() {
      return this._responseObj.blob()
    }
    json() {
      return this._responseObj.json()
    }
    text() {
      return this._responseObj.text()
    }  
  }
  
  export class CredentialProvider {
    constructor(initialValue = null) {
      this._value = initialValue
    }
    get() {
      if (this._value == null) {
        throw new CredentialNotFoundError()
      }
      return this._value;
    }
    put(value) {
      if (value == null) {
        throw new Error("`value` cannot be null")
      }
      this._value = value
    }
    clear() {
      this._value = null;
    }
  }
  
  export class HttpAuthentication {
    constructor({ type }) {
      this.type = type
    }
    async onAuthorization(url, opts) {
      throw new Error('NotImplemented')
    }
  }
  
  export class HttpTokenAuthorization extends HttpAuthentication {
    constructor({ getTokenUrl, scheme = 'Bearer' }) {
      super({ type: 'token' });
      this._getTokenUrl = getTokenUrl;
      this._getTokenDeferred = null;
      this._client = new HttpRequest();
      this._scheme = scheme;
      this._tokenCache = null;
      this.credential = new CredentialProvider()
      this.onAuthorization = this.onAuthorization.bind(this)
    }
  
    async onAuthorization(url, opts) {
      let token = await this.getToken()
      opts.headers['Authorization'] = this._scheme + ' ' + token
    }
  
    async getToken(){
      let token;
      if (this._getTokenDeferred != null) {
        token = await this._getTokenDeferred;
      } else {
        this._getTokenDeferred = this._getToken();
        token = await this._getTokenDeferred;
        this._getTokenDeferred = null;
      }
      return token;
    }
  
    async _getToken(retry = true) {
      if (this._tokenCache != null) {
        if (this._tokenCache.expiresOn - 20000 > new Date(Date.now())) {
          return this._tokenCache.accessToken;
        }
        this._tokenCache = null;
        console.log("token expired");
      }
      let credential = this.credential.get();
      let authFormData = new FormData();
      for (let k of Object.keys(credential)) {
        authFormData.append(k, credential[k]);
      }
      let response = await this._client.post(this._getTokenUrl, { body: authFormData });
      if (response.status === 401 && retry) {
        this._tokenCache = null;
        return await this._getToken(false)
      }
  
      response.assertStatus(200);
  
      let { access_token, expires_on } = await response.json();
      if (access_token == null || expires_on == null) {
        throw new Error("Cannot parse token info");
      }
  
      let expiresOn = Date.parse(expires_on)
      let tokenInfo = { accessToken: access_token, expiresOn: new Date(expiresOn) };
      this._tokenCache = tokenInfo;
      return access_token;
    }
    _getCredential() {
      try {
        return this.credential.get();
      } catch (e) {
        if (e instanceof CredentialNotFoundError) {
          throw new AuthorizationRequiredError()
        }
        throw e;
      }
    }
  }
  
  export class HttpRequest {
    constructor(opts) {
      const { baseUrl, authorization } = opts || {};
      this._baseUrl = baseUrl;
      this._authorization = authorization;
    }
    async post(url, opts) {
      const { headers = {}, params = null, json = null } = opts || {};
      let body = opts['body']
      if (body != null && json != null) {
        throw new Error('Either "body" or "json" must be defined, not both')
      }
      if (json != null) {
        body = JSON.stringify(json)
        if (!('Content-Type' in headers)) {
          headers['Content-Type'] = 'application/json'
        }
      }
      let _url = this._makeUrl(url, params)
      let fetchOpts = {
        method: 'POST',
        body,
        headers,
        mode: 'cors',
        cache: 'no-cache'
      };
  
      if (this._authorization) {
        await this._authorization.onAuthorization(_url, fetchOpts)
      }
  
      return await this._sendRequest(_url, fetchOpts)
    }
    async get(url, opts) {
      const { headers = {}, params = null } = opts || {};
      let _url = this._makeUrl(url, params)
      let fetchOpts = {
        method: 'GET',
        headers,
        params,
        mode: 'cors',
        cache: 'default'
      };
  
      if (this._authorization) {
        await this._authorization.onAuthorization(_url, fetchOpts)
      }
  
      return await this._sendRequest(_url, fetchOpts)
    }
    _makeUrl(url, params) {
      const _url = new URL(this._baseUrl ? this._baseUrl + url : url);
      if (params != null) {
        Object.keys(params)
          .filter(key => params[key] != null && params[key] !== '')
          .forEach(key => _url.searchParams.append(key, params[key]))
      }
      return _url
    }
    async _sendRequest(url, opts) {
      const responseObj = await fetch(url, opts);
      return new HttpResponse(responseObj)
    }
  }
  
  export default new HttpRequest()