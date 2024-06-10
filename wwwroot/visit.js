window.addEventListener("load", onload);

var criteria = {
    pageId: null,
    start: Math.round(new Date() / 1000) - 3600 * 24,
    end: Math.round(new Date() / 1000),
    interval: "5 days"
};

function getPageInfo() {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/tool/visits/page");
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.setRequestHeader("Accept", "application/json");
    xhr.setRequestHeader("Tid", crypto.randomUUID());
    xhr.onreadystatechange = pageInfoCallback;
    xhr.send(JSON.stringify(criteria));
}

function pageInfoCallback() {
    if (this.readyState < 4) {
        return;
    }
    if (this.status != 200) {
        return;
    }
    var data = JSON.parse(this.responseText);
    document.getElementById("result").textContent = JSON.stringify(data, null, "\t");
}

function onload() {
    var searchParams = new URLSearchParams(window.location.search);
    var id = parseInt(searchParams.get('id'), 10);
    if (isNaN(id)) {
        return;
    }
    criteria.pageId = id;
    getPageInfo();
}