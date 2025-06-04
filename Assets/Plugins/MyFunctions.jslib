// 使用 Unity 和 WebView 通信
mergeInto(LibraryManager.library, {
    SendMessageToParent: function (message) 
    {
        const data = {
            type: 'send:message',
            data: {
                message: UTF8ToString(message),
            },
        }; 
        window.parent.postMessage(data, "*");
    },

    OpenUrl: function (url) 
    {
        const data = {
            type: 'openurl',
            data: {
                message: UTF8ToString(url),
            },
        }; 
        window.parent.postMessage(data, "*");
    },

    Back: function () 
    {
        const data = {
            type: 'back',
        }; 
        window.parent.postMessage(data, "*");
    },

    ReloadPage: function () {
        const data = {
            type: 'reload',
        }; 
        window.parent.postMessage(data, "*");
    },
});
