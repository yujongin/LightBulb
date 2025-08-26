mergeInto(LibraryManager.library, {
NotifyReactApp: function (message) {
    
const messageStr = UTF8ToString(message);

if (typeof window.ReactAppNotify === 'function') {
window.ReactAppNotify(messageStr);
} else {
console.warn('ReactAppNotify function is not defined.');
}
}
});