function API(func, url, fetch_params) {
    APIPromise(url, fetch_params)
        .then((json) => {
            func(json);
        })
}
function APIOp(func, url, fetch_params) {
    APIPromise(url, fetch_params)
        .then((json) => {
            console.log(json);
            if (json.ok)
                func(json.result);
            else
                if (json.error['InnerException'] == undefined)
                    showError(`На стороне сервера возникла ошибка: ${json.error['Message']}`);
                else
                    showError(`На стороне сервера возникла ошибка: ${json.error['InnerException']}`);
        })
}
function APIPromise(url, fetch_params) {
    return fetch(url, fetch_params)
        .then((response) => {
            if (!response.ok) {
                throw new Error(`${response.status} ${response.statusText}`);
            }
            return response.json();
        })
        .catch((error) => {
            showError(`При запросе произошла ошибка ${error}`);
        });
}
function APITable(callback, url, fetch_params) {
    return APIPromise(url, fetch_params)
        .then((json) => {
            console.log(json);
            if (json.ok)
                callback(json.result);
            else
                if (json.error['InnerException'] == undefined)
                    showError(`На стороне сервера возникла ошибка: ${json.error['Message']}`);
                else
                    showError(`На стороне сервера возникла ошибка: ${json.error['InnerException']}`);
        });
}
function APITableWithPreload(preload, callback, url, fetch_params) {
    return APIPromise(url, fetch_params)
        .then((json) => {
            console.log(json);
            preload();
            if (json.ok)
                callback(json.result);
            else
                if (json.error['InnerException'] == undefined)
                    showError(`На стороне сервера возникла ошибка: ${json.error['Message']}`);
                else
                    showError(`На стороне сервера возникла ошибка: ${json.error['InnerException']}`);
        });
}