export function getQueryParam(name) {
    const params = new URLSearchParams(window.location.search);
    const val = params.get(name);

    return val;
}