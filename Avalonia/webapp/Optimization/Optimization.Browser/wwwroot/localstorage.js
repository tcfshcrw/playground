// wwwroot/js/localstorage.js
globalThis.localStorageSet = (key, value) => {
    localStorage.setItem(key, value);
};
globalThis.localStorageGet = (key) => {
    return localStorage.getItem(key);
};
globalThis.customLog = (msg) => {
    console.log("【CustomLog】" + msg);
};