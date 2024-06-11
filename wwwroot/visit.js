window.addEventListener("load", onload);

var criteria = {
    pageId: null,
    start: Math.round(new Date() / 1000) - 3600 * 24,
    end: Math.round(new Date() / 1000),
    interval: ""
};

var startEl, endEl, vizEl;

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
    drawSparklines(vizEl, data, criteria);
}

function drawSparklines(vizEl, data, criteria) {
    var width = (criteria.end - criteria.start);
    var parts = 10;
    var part = width / parts;
    var start = 0;
    var end = part;
    for (var i = 0; i < parts; i++) {
        start = start + 1;
        end = end + 1;
    }
}

function setDate(el, timestampSeconds) {
    var offset = new Date().getTimezoneOffset();
    var adjustedDate = new Date(timestampSeconds * 1000 - offset * 60000);
    el.value = adjustedDate.toISOString().slice(0, 16);
}

function onload() {
    startEl = document.getElementById("start");
    endEl = document.getElementById("end");
    vizEl = document.getElementById("visits");
    setDate(startEl, criteria.start);
    setDate(endEl, criteria.end);
    var rangesContainers = document.getElementById("ranges");
    rangesContainers.addEventListener("click", rangesOnClick);
    startEl.addEventListener("blur", updateFilter);
    endEl.addEventListener("blur", updateFilter);
    document.getElementById("load").addEventListener("click", getPageInfo);

    var searchParams = new URLSearchParams(window.location.search);
    var id = parseInt(searchParams.get('id'), 10);
    if (isNaN(id)) {
        return;
    }
    criteria.pageId = id;
    getPageInfo();
}

function rangesOnClick(event) {
    var classList = event.target.classList;

    if (classList.contains("btn-range")) {
        criteria.interval = event.target.value;
    }

    var lookup = {
        "onehours": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "onedays": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "onedays": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "oneweeks": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24 * 7;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "onemonth": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24 * 30;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "oneyear": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24 * 356;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        },
        "all": function () {
            criteria.start = Math.round(new Date() / 1000) - 3600 * 24 * 356 * 5;
            criteria.end = Math.round(new Date() / 1000);
            setDate(startEl, criteria.start);
            setDate(endEl, criteria.end);
        }
    };

    for (var i = 0; i < classList.length; i++) {
        var fun = lookup[classList[i]];
        if (typeof (fun) == "function") {
            fun();
        }
    }
}

function updateFilter() {
    criteria.interval = "";
    criteria.start = new Date(startEl.value) / 1000;
    criteria.end = new Date(endEl.value) / 1000;
}