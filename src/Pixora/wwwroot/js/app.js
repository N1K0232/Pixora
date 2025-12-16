function uuid() {
    const now = BigInt(Date.now());
    const timestamp = now & BigInt("0xffffffffffff");

    const randomBytes = crypto.getRandomValues(new Uint8Array(10));
    const bytes = new Uint8Array(16);

    bytes[0] = Number((timestamp >> 40n) & 0xffn);
    bytes[1] = Number((timestamp >> 32n) & 0xffn);
    bytes[2] = Number((timestamp >> 24n) & 0xffn);
    bytes[3] = Number((timestamp >> 16n) & 0xffn);
    bytes[4] = Number((timestamp >> 8n) & 0xffn);
    bytes[5] = Number(timestamp & 0xffn);
    bytes[6] = (randomBytes[0] & 0x0f) | 0x70;
    bytes[7] = (randomBytes[1] & 0x3f) | 0x80;
    bytes.set(randomBytes.slice(2), 8);

    const hex = [...bytes].map(b => b.toString(16).padStart(2, "0")).join("");
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}

function dataURIToBlob(dataURI) {
    const splitDataURI = dataURI.split(',')
    const byteString = splitDataURI[0].indexOf('base64') >= 0 ? atob(splitDataURI[1]) : decodeURI(splitDataURI[1])
    const mimeString = splitDataURI[0].split(':')[1].split(';')[0]

    const ia = new Uint8Array(byteString.length)
    for (let i = 0; i < byteString.length; i++)
        ia[i] = byteString.charCodeAt(i)

    return new Blob([ia], { type: mimeString })
}

function GetErrorMessage(statusCode, content) {
    if (statusCode >= 200 && statusCode <= 299) {
        return null;
    }

    if (content.errors) {
        return `${content.title ?? content} (${content.errors[0].message})`;
    }

    return content.detail ?? content.title ?? content;
}

function sleep(time) {
    return new Promise((resolve) => {
        setTimeout(resolve, time);
    });
}

async function copyToClipboard(element, text) {

    let tooltip = bootstrap.Tooltip.getInstance(element);
    tooltip.hide();

    navigator.clipboard.writeText(text);

    element.setAttribute('data-bs-title', 'Copied!');

    tooltip = new bootstrap.Tooltip(element);
    tooltip.show();

    await sleep(3000);
    tooltip.hide();

    // Resets the tooltip title
    element.setAttribute('data-bs-title', 'Copy to clipboard');
    new bootstrap.Tooltip(element);
}

function setAuthCookie(name, accessToken, refreshToken, isPersistent) {
    window.localStorage.setItem('access_token', accessToken);
    window.localStorage.setItem('refresh_token', refreshToken);

    let cookie = `${name}=${accessToken}; path=/; Secure; SameSite=Strict`;

    if (isPersistent) {
        const expirationDays = 7;
        const date = new Date();
        date.setTime(date.getTime() + expirationDays * 24 * 60 * 60 * 1000);
        cookie += `; Expires=${date.toUTCString()}`;
    }

    document.cookie = cookie;
}