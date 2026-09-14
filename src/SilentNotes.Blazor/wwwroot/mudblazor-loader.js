function checkMinChromeWebViewVersion(minVersion) {
    // Chromium/WebView based browsers contain a "Chrome/xx.x.xxxx.xx" part
    const match = navigator.userAgent.match(/Chrome\/(\d+)/);
    if (match) {
        const majorVersion = parseInt(match[1], 10);
        return majorVersion >= minVersion;
    }
    return true; // Fallback for non-Chromium browsers
}

// Workaround: Check whether we need to load an ES2019 compatible version.
if (checkMinChromeWebViewVersion(80))
    document.write('<script src="_content/MudBlazor/MudBlazor.min.js"><\/script>');
else
    document.write('<script src="MudBlazor.es2019.js"><\/script>');
