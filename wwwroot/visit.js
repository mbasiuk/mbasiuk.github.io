window.addEventListener("load", onload);

var criteria = {
    pageId: null,
    start: Math.round(new Date() / 1000) - 3600 * 24,
    end: Math.round(new Date() / 1000),
    interval: ""
};

var startEl, endEl, vizEl, vizSvgEl, rgEl;

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
    drawSparklines(vizEl, data, criteria);
    renderGrid(rgEl, data);
}

function renderGrid(egEl, data) {
    // 100% wide - row
    // narrow - more vertical
    var frag = document.createDocumentFragment();
    var schema = ["id", "page", "ip", "timestamp", "time", "sessionTimestamp", "sessionTime", "userAgent", "acceptedLang", "referer", "origin", "platform", "ua", "mobile"];
    var labels = {
        "id": "id",
        "page": "p",
        "ip": "ip",
        "timestamp": "ts",
        "sessionTime": "st",
        "time": "t",
        "sessionTimestamp": "ft",
        "userAgent": "ua",
        "acceptedLang": "l",
        "referer": "ref",
        "origin": "or",
        "platform": "p",
        "ua": "ua2",
        "mobile": "mob"
    };
    var classList = {
        "id": "id",
        "page": "p",
        "ip": "ip",
        "timestamp": "ts",
        "time": "t",
        "sessionTimestamp": "ft",
        "sessionTime": "st",
        "userAgent": "ua",
        "acceptedLang": "l",
        "referer": "ref",
        "origin": "or",
        "platform": "p",
        "ua": "ua2",
        "mobile": "mob"
    };
    var extentions = {
        "userAgent": function (record, spanVal, spanLab, frag) {
            if (/iphone/gi.test(record.userAgent)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("iphone");
                spanVal.textContent = "";
            }
            if (/LinkedInApp/gi.test(record.userAgent)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("linkedin");
            }
            if (/windows nt/gi.test(record.userAgent)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("windows");
                spanVal.textContent = "";
            }
            if (/Macintosh/gi.test(record.userAgent)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("mac");
                spanVal.classList.add("hidden");
            }
        },
        "ua": function (record, spanVal, spanLab, frag) {
            if (/google chrome/gi.test(record.ua)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("googlechrome");
                spanVal.textContent = "";
            }
        },
        "acceptedLang": function (record, spanVal, spanLab, frag) {
            if (/en\-IN|\,hi;/gi.test(record.acceptedLang)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-in");
            }    
            if (/zh\-CN\,/gi.test(record.acceptedLang)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-cn");
            }            

            if (/en\-us/gi.test(record.acceptedLang)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-us");
            }
            if (/\,uk;|en\-GB/gi.test(record.acceptedLang)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-uk");
            }
            if (/de\-DE/gi.test(record.acceptedLang)) {
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-de");
            }
            if(/fr\-FR/gi.test(record.acceptedLang)){
                var ico = document.createElement("span");
                frag.appendChild(ico);
                ico.classList.add("flag-fr");
            }
            
        }
    };

    egEl.innerText = ""
    for (var i = 0; i < data.length; i++) {
        var record = data[i];
        if (i === 0) {
            record.userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 11_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";
        }
        record.time = new Date(record.timestamp * 1000).toLocaleString();
        record.sessionTime = new Date(record.sessionTimestamp * 1000).toLocaleString();
        for (var j = 0; j < schema.length; j++) {
            var col = schema[j];
            var val = record[col];
            var classes = classList[col];
            if (!val || val == "-") {
                continue;
            }
            var spanLab = document.createElement("span");
            frag.appendChild(spanLab);
            spanLab.classList.add(classes);
            spanLab.classList.add("label");
            spanLab.textContent = labels[col];
            var spanVal = document.createElement("span");
            frag.appendChild(spanVal);
            spanVal.textContent = record[col];
            spanVal.classList.add(classes);
            spanVal.classList.add("value");
            var exFun = extentions[col];
            if (typeof (exFun) == "function") {
                exFun(record, spanVal, spanLab, frag);
            }
        }
        var container = document.createDocumentFragment();
        var div = document.createElement("div");
        div.className = "c";
        div.appendChild(frag);
        container.appendChild(div);
        egEl.appendChild(container);
    }
}

function drawSparklines(vizEl, data, criteria) {
    if (!data.length) {
        vizEl.setAttribute("d", "M 0 0 Z");
        return;
    }
    var width = (criteria.end - criteria.start) || 1;
    var buckets_number = 12;

    var buckets = Array(buckets_number).fill(0);
    for (var i = 0; i < data.length; i++) {
        var x = data[i].timestamp;
        if ((x < criteria.start) || (x > criteria.end)) {
            continue;
        }
        var bucket = Math.trunc(((x - criteria.start) / width) * buckets_number);
        buckets[bucket]++;
    }

    var d = "";
    var w = 10;
    var x = 5;
    var y = 0;
    for (var i = 0; i < buckets_number; i++) {
        y = buckets[i];
        if (y) {
            d += " M " + x + " 0 V " + y;
        }
        x += w;
    }
    d += " Z";
    var max = Math.max(...buckets) || 1;
    vizEl.setAttribute("d", d);
    vizSvgEl.setAttribute("viewBox", "0 0 " + (x - 5) + " " + max);
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
    vizSvgEl = document.getElementById("vizSvgEl");
    setDate(startEl, criteria.start);
    setDate(endEl, criteria.end);
    var rangesContainers = document.getElementById("ranges");
    rgEl = document.getElementById("resultTable");
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