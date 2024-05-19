function signIn() {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/signin", true);
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.onreadystatechange = signInCallback;
    xhr.send(JSON.stringify({
        user: document.getElementById("user").value,
        password: document.getElementById("password").value
    }));
    return false;
}

function signInCallback() {
    if (this.readyState < 4) {
        return;
    }

    if (this.status >= 400) {
        console.error(this.status, this.responseText);
        return;
    }

    var currentUrl = window.location.href;
    var url = new URL(currentUrl);
    var params = new URLSearchParams(url.search);
    var returnUrl = params.get('returnUrl');
    document.location.href = returnUrl || "/";
}