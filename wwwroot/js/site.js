// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.onreadystatechange = function () {
    var state = document.readyState;
    var startTime = new Date().getTime(); // Get the start time
    if (state == 'interactive') {
        document.getElementById('loading').style.visibility = 'visible';
        setTimeout(function () {
            var elapsedTime = new Date().getTime() - startTime;
            var remainingTime = 4000 - elapsedTime;
            if (remainingTime > 0) {
                setTimeout(function () {
                    document.getElementById('loading').style.visibility = 'hidden';
                }, remainingTime);
            } else {
                document.getElementById('loading').style.visibility = 'hidden';
            }
        }, 0); // 0 milliseconds delay
    } else if (state == 'complete') {
        document.getElementById('loading').style.visibility = 'hidden';
    }
};
