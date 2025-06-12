// wwwroot/js/localstorage.js
window.localStorageSet = (key, value) => {
    localStorage.setItem(key, value);
};
window.localStorageGet = (key) => {
    return localStorage.getItem(key);
};