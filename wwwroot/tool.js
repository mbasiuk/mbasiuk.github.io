function renderRecentSearch(data) {
    var json;
    if (!(this.readyState == 4 && this.status == 200 && (json = JSON.parse(this.responseText)) && json.length)) return;
    var frag = document.createDocumentFragment();
    for (var i = 0; i < json.length; i++) {
        var search = json[i];
        var el = document.createElement("a");
        var sp = document.createElement("span");
        var cr = document.createElement("span");
        cr.className = "date";
        if (search.created) {
            cr.textContent = new Date(search.created).toLocaleString();
        }
        sp.textContent = search.description;
        el.href = "/tool/search/" + search.id;
        el.textContent = search.id;
        frag.appendChild(el);
        frag.appendChild(sp);
        frag.appendChild(cr);
    }
    var sb = document.getElementById("searchbiz");
    sb.innerText = "";
    sb.appendChild(frag);
}

function getSearchRecent() {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/tool/search/recent", true);
    xhr.onreadystatechange = renderRecentSearch;
    xhr.send(null);
}

function renderVisits() {
    var json;
    if (!(this.readyState == 4 && this.status == 200 && (json = JSON.parse(this.responseText)) && json.length)) return;
    var frag1 = document.createDocumentFragment();
    for (var i = 0; i < json.length; i++) {
        var visit = json[i];

        var frag2 = document.createDocumentFragment();

        var c = document.createElement("div");
        c.className = "visit";

        var ch = document.createElement("input");
        ch.setAttribute("type", "checkbox");
        ch.setAttribute("name", "visitid");
        ch.setAttribute("value", visit.id);
        ch.className = "visitid";

        var sp = document.createElement("span");
        sp.classList = "sp";

        var page = document.createElement("a");
        page.textContent = visit.page;
        page.setAttribute("href", "/tool/visits/page?id=" + parseInt(visit.id, 10));

        var total = document.createElement("span");
        total.textContent = visit.total;

        var unique = document.createElement("span");
        unique.textContent = visit.unique;

        var newVisitors = document.createElement("span");
        newVisitors.textContent = visit.n;

        frag2.appendChild(ch);
        frag2.appendChild(page);
        frag2.appendChild(sp);
        frag2.appendChild(total);
        frag2.appendChild(unique);
        frag2.appendChild(newVisitors);
        c.appendChild(frag2);
        frag1.appendChild(c);
    }
    var visitbypages = document.getElementById("visitbypages");
    visitbypages.innerText = "";
    visitbypages.appendChild(frag1);
}

function getVisits() {
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/tool/visits", true);
    xhr.onreadystatechange = renderVisits;
    xhr.send(null);
}

function ignoreProbing(event) {
    var that = event.target;
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/tool/visits/ignore", true);
    var visitsContainer = document.getElementById("visitbypages")
    var visitIds = visitsContainer.querySelectorAll("input:checked");
    var x = {
        values: Array.from(visitIds).map(x => parseInt(x.value))
    };
    xhr.onreadystatechange = function () {
        that.setAttribute("rs", this.readyState);
        that.setAttribute("s", this.status);
        if (this.readyState == 4 && this.status < 400) {
            getVisits();
        }
    };
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.send(JSON.stringify(x));
    return false;
}

function getServerDetails() {
    var serverdetails = document.getElementById("serverdetails");
    var xhr = new XMLHttpRequest();
    xhr.open("POST", "/tool/serverdetails", true);
    xhr.onreadystatechange = function () {
        if (this.readyState < 4) {
            return;
        }
        if (this.status > 302) {
            return;
        }
        serverdetails.textContent = this.responseText;
    };
    xhr.setRequestHeader("Content-Type", "application/json");
    xhr.setRequestHeader("tid", crypto.randomUUID());
    xhr.send();
    return false;
}

getVisits();
getSearchRecent();