import { HttpRequest } from './httprequest'

export class ResponseValidationError extends Error { }

export class AddResponse {
    constructor(ok, { key = null, errors = null }) {
        this.ok = ok;
        this.key = key;
        this.errors = errors
    }
}

function _bytesToBase64(bytes) {
    if (!(bytes instanceof Uint8Array)) {
        throw new Error('"buffer" must be an instance of "Uint8Array"')
    }
    var binary = '';
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

function _base64ToBytes(str) {
    if (typeof str !== 'string') {
        throw new Error('"str" must be an instance of "string"')
    }
    var raw = window.atob(str);
    var rawLength = raw.length;
    var array = new Uint8Array(new ArrayBuffer(rawLength));

    for (let i = 0; i < rawLength; i++) {
        array[i] = raw.charCodeAt(i);
    }
    return array;
}

function _utf8Encode(input) {
    return new TextEncoder().encode(input)
}

export class ApiClient {
    constructor({ baseUrl, authorization = null }) {
        this.props = { baseUrl }
        this._client = new HttpRequest({ baseUrl, authorization })
    }
    getKeyId({ publicKey, sign, digest }) {
        const { baseUrl } = this.props;
        const client = new HttpRequest({ baseUrl })
        return client.post(`/authorize/challenge`, {
            json: { publicKey },
            headers: { 'Content-Type': 'application/jwk+json' }
        })
            .then((r) => r.assertStatus(200))
            .then((r) => r.json())
            .then(async ({ challenge }) => {
                const dgst = await digest(_utf8Encode(challenge));
                const signature = _bytesToBase64(await sign(dgst));
                return { challenge, signature }
            })
            .then((json) => client.post('/authorize/key', { json }))
            .then((r) => r.assertStatus(200))
            .then((r) => r.json())
            .then(({ key_id }) => key_id)
    }
    search({ term, pageSize, page }) {
        return this._client.get('/api/v1/search', { params: { term, pageSize, page } })
    }
}