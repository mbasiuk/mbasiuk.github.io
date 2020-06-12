addEventListener('load', function () {

    var counters = document.getElementsByClassName('counter');
    for (var i = 0; i < counters.length; i++) {
        var counter = counters[i];
        counter.addEventListener('click', onCounterClick);
    }
});

function onCounterClick() {
    this.classList.toggle("selected")
}
