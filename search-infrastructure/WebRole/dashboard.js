'use strict';

$(document).ready(function () {
    //retrieve state of each worker role web crawler, if at least one worker is on, set switch to on and set color
    //retrieve machine counters (CPU, % RAM available)
    //retrieve last 10 urls crawled
    //retrieve number of urls crawled
    //retrieve size of queue
    //retrieve index size
    $.ajax({
        url: '/admin.asmx/getStats',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json'
    })
        .done(function (data) {
            try {
                var stats = JSON.parse(data.d);
                $('#worker').text(stats.status);
                $('#cpu').text(stats.cpu + '%');
                $('#ram').text(stats.memory + 'mb');
                $('#urls-crawled').text(stats.urlsCrawled);
                $('#queue-size').text(stats.queueSize);
                $('#index-size').text(stats.indexSize);
                var lastTen = (stats.lastTenString.slice(0, -2)).split("; ");
                $.each(lastTen, function (index, url) {
                    $('#last-ten').append($('<div>').text(url));
                });
            }
            catch (err) {
                console.log("Error retrieving stats");
            }
        })
        .fail(function (err) {
            console.log("Error retrieving stats");
        });

    //retrieve trie stats
    $.ajax({
        url: '/admin.asmx/getTrieStats',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json'
    })
    .done(function (data) {
        try {
            var stats = JSON.parse(data.d);
            $('#word-count').text(stats.indexSize);
            $('#last-word').text(stats.lastTenString);
        }
        catch (err) {
            console.log("Error retrieving trie stats");
        }
    })
    .fail(function (err) {
        $('#errors').text("Error retrieving trie stats");
    });

    //retrieve any errors and urls
    $.ajax({
        url: '/admin.asmx/getErrors',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json'
    })
    .done(function (data) {
        try {
            var errors = JSON.parse(data.d);
            var errorDiv = $('#errors');
            errorDiv.html('');

            if (errors.length == 0) {
                errorDiv.html('No errors have occurred');
            } else {
                $.each(errors, function (index, error) {
                    errorDiv.append($('<div>').html('<b>'+error.url+'</b>' + ': ' + error.err));
                });
            }
        }
        catch (err) {
            $('#errors').text('Error retrieving errors');
        }
    })
    .fail(function (err) {
        $('#errors').text("Error retrieving errors");
    });

    //user functionality
    //start crawling given specific root url
    $('#start').on('click', function () {
        var input = $("input[name=rootUrl]").val();
        if (input) {
            $.ajax({
                url: '/admin.asmx/startCrawling',
                type: 'POST',
                data: JSON.stringify({ url: input }),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json'
            })
                .done(function (data) {
                    $('#start-msg').text(data.d);
                })
                .fail(function (err) {
                    $('#start-msg').text('Error starting crawl');
                });
        }
    });

    //get page title given specific url
    $('#titleButton').on('click', function () {
        var input = $("input[name=titleUrl]").val();
        if (input) {
            $.ajax({
                url: '/admin.asmx/getPageTitle',
                type: 'POST',
                data: JSON.stringify({ url: input }),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json'
            })
                .done(function (data) {
                    //display title
                    $('#page-title').text(data.d);
                })
                .fail(function (err) {
                    //error title
                    console.log(err);
                    $('#page-title').text('Could not retrieve title');
                });
        }
    });

    //clear index and stop crawling, set switch to off and change color
    $('#clear').on('click', function () {
        var input = $("input[name=password]").val();
        if (input) {
            $.ajax({
                url: '/admin.asmx/clearIndex',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ password: input }),
                dataType: 'json'
            })
            .done(function (data) {
                $('#clear-msg').text(data.d);
            })
            .fail(function (err) {
                //error title
                console.log(err);
                $('#clear-msg').text('Error clearing index');
            });
        }

    });

    //for button ripple click styling
    (function () {
        var pollId = 0;
        pollId = setInterval(function () {
            var pos = getComputedStyle(document.querySelector('.mdc-button')).position;
            if (pos === 'relative') {
                init();
                clearInterval(pollId);
            }
        }, 250);
        function init() {
            var btns = document.querySelectorAll('.mdc-button');
            for (var i = 0, btn; btn = btns[i]; i++) {
                mdc.ripple.MDCRipple.attachTo(btn);
            }
        }
    })();
});