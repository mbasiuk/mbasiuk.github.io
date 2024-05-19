function renderRecentSearch(data) {
    var json;
    if (!(xhr.readyState == 4 && xhr.status == 200 && (json = JSON.parse(xhr.responseText)) && json.length)) return;
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
    document.getElementById("searchbiz").appendChild(frag);
}

var xhr = new XMLHttpRequest();
xhr.open("POST", "/tool/search/recent", true);
xhr.onreadystatechange = renderRecentSearch;
xhr.send(null);

function renderVisits(data) {
    var json;
    if (!(xhr1.readyState == 4 && xhr1.status == 200 && (json = JSON.parse(xhr1.responseText)) && json.length)) return;
    var frag = document.createDocumentFragment();
    for (var i = 0; i < json.length; i++) {
        var visit = json[i];
        var page = document.createElement("a");
        page.textContent = visit.page;
        page.setAttribute("href", "/tool/visits/page/" + escape(visit.page));

        var total = document.createElement("span");
        total.textContent = visit.total;

        var unique = document.createElement("span");
        unique.textContent = visit.unique;

        var newVisitors = document.createElement("span");
        newVisitors.textContent = visit.n;

        frag.appendChild(page);
        frag.appendChild(total);
        frag.appendChild(unique);
        frag.appendChild(newVisitors);
    }
    document.getElementById("visitbypages").appendChild(frag);
}

var xhr1 = new XMLHttpRequest();
xhr1.open("POST", "/tool/visits", true);
xhr1.onreadystatechange = renderVisits;
xhr1.send(null);
