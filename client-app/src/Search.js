import React from 'react';
import config from './config';
import { ApiClient } from './api-client';
import { HttpTokenAuthorization } from './httprequest';

const authorization = new HttpTokenAuthorization({ getTokenUrl: config.getTokenUrl })
authorization.credential.put({ username: 'pRpsUKcFzxzupxGyeaPyHx2nN6NBos2iD7peRXme', password: 'Fu6fswGfmgbg9fNA68vmjzUSY2dYK5WtsoA8RyXn' })
const apiClient = new ApiClient({ baseUrl: config.baseUrl, authorization })

export class SearchComponent extends React.Component {
    constructor(props) {
        super(props)
        this.state = { loading: false, response: null, query: { term: null, page: 1, pageSize: 10 } }
        this.handleFormSubmit = this.handleFormSubmit.bind(this);
        this.handleInputChange = this.handleInputChange.bind(this);
    }
    async componentDidMount() {
        const { term, page, pageSize } = this.state.query;
        if (term) {
            this.setState({ loading: true, error: null, response: null });
            var error;
            try{
                var response = await apiClient.search({
                    term: term,
                    pageSize: pageSize,
                    page: page
                })
                var responseData = await response.json();
            }catch(err){
                error = err;
            }
            this.setState({ loading: false, error, response: responseData });
        }
    }
    handleFormSubmit(ev) {
        this.componentDidMount();
        ev.preventDefault();
        return false;
    }
    handleInputChange(ev) {
        var term = ev.target.value;
        if (term == null || term.length === 0) {
            term = null;
        }
        const query = this.state.query;
        query.term = term;
        query.page = 1;
        this.setState({ query })
    }
    handleNavClick(page) {
        const query = this.state.query;
        query.page = page;
        this.setState({ query });
        this.componentDidMount();
    }
    render() {
        const { loading, response } = this.state;
        return (
            <div class="Search">
                <h2>Combined Search</h2>
                <p>Search for a specific term using two different search engines</p>
                <div>
                    <form class="form-inline" onSubmit={this.handleFormSubmit}>
                        <input type="text" onChange={this.handleInputChange} className="form-control" />
                        <button type="submit" disabled={loading} className="btn btn-primary">Search</button>
                    </form>
                </div>
                <div>

                    {loading && this._renderLoading()}
                    {!loading && response && this._renderPagination(response)}
                    {!loading && response && this._renderResponse(response)}
                </div>
            </div>
        )
    }
    _renderPagination(obj) {
        const { id, page, pageSize, total, results } = obj;
        var totalPages = 0;
        if (total > 0 && total < pageSize) {
            totalPages = 1;
        } else if (total > pageSize) {
            totalPages = Math.ceil(total / pageSize);
        }
        const renderNext = () => <button type="button" class="btn btn-light" disabled={!(page < totalPages)} onClick={(ev) => this.handleNavClick(page + 1)}>Next</button>
        const renderPrev = () => <button type="button" class="btn btn-light" disabled={!(page > 1)} onClick={(ev) => this.handleNavClick(page - 1)}>Prev</button>
        return (
            <div>
                <p>Found {total} results. Showing page {page} of {totalPages}. {renderPrev()} | {renderNext()}</p>
                <ul class="list-group">
                    {results.map((x) => this._renderResult(x))}
                </ul>
            </div>
        )
    }
    _renderResponse(obj) {
        const { id, page, pageSize, total, results } = obj;
        return (
            <div>
                <p>Found {total} results</p>
                <ul class="list-group">
                    {results.map((x) => this._renderResult(x))}
                </ul>
            </div>
        )
    }
    _renderResult(obj) {
        const { url, title, description, source } = obj;
        return (
            <li class="list-group-item">
                <h3><a href={url} target='_blank'>{title}</a></h3>
                <span>Source: {source}</span>
                <p>{description}</p>
                <a href={url} target='_blank'>{url}</a>
            </li>
        )
    }

    _renderLoading() {
        return (
            <p>Loading...</p>
        )
    }
}