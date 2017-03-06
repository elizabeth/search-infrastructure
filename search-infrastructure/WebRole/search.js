'use strict';

$(document).ready(function () {
    $('#searchForm').submit(function (e) {
        e.preventDefault(e);
        $('#submit').blur();
        $('#search').blur();
        $('#suggestions').remove();

        var search = $('#search').val();
        searchPlayers(search);
        searchCrawler(search);
        saveSearch(search);
        ////add loading icon
        //$('#players').html('<i id="spinner" class="fa fa-5x fa-fw fa-refresh fa-spin"></i>')
        //    .promise()
        //    .done(function () {
        //        //search and return players
        //        searchPlayers($("[name='name']").val());
        //    });
    });

    $('#search').keyup(function (e) {
        //prevent ajax call from happening twice if on submit
        if (e.keyCode != 13) {
            //only make ajax call if user stops typing for short time
            delay(function () {
                if ($('#search').val()) {
                    $('#player').remove();
                    $('#results').remove();
                    var search = $('#search').val();
                    querySearch(search);
                    searchPlayers(search);
                    searchCrawler(search);
                } else {
                    $('#suggestions').remove();
                    $('#player').remove();
                    $('#results').remove();
                }
            }, 500);
        }
    });

    //AJAX request to search trie
    function querySearch(searchTerm) {
        //send the AJAX call to the server
        $.ajax({
            url: 'querySuggest.asmx/searchTrie',
            type: 'POST',
            data: JSON.stringify({ term: searchTerm }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json'
        })
            //the response from the server
            .done(function (data) {
                var suggestions = suggestDiv();
                try {
                    var results = JSON.parse(data.d);

                    //display the results
                    $.each(results, function (key, bool) {
                        //set on click
                        var divResult = $('<div>').text(key).on('click', function () {
                            clickSuggestion(key);
                        });

                        if (bool) {
                            $(divResult).addClass('prev-search');
                        }
                        suggestions.append(divResult);
                    })
                } catch (err) {
                    suggestions.remove();
                    console.log('Error retrieving suggestions. ' + err);
                }
            }).fail(function (err) {
                $('#suggestions').remove();
                console.log('Error retrieving suggestions. ' + err);
            });
    }

    //AJAX request to search and display players
    function searchPlayers(searchTerm) {
        //send the AJAX call to the server
        $.ajax({
            crossDomain: true,
            contentType: 'application/json; charset-utf-8',
            url: 'http://54.213.188.54/search-infrastructure/search-player.php',
            type: 'GET',
            data: { 'search': searchTerm },
            dataType: 'jsonp'
        })
            //The response from the server
            .done(function (data) {
                var player = playerDiv();

                try {
                    if (data[0]) {
                        var stats = data[0];

                        var row1 = $('<div>', { 'class': 'mdc-layout-grid__cell mdc-layout-grid__cell--span-12' });
                        var row2 = $('<div>', { 'class': 'mdc-layout-grid__cell mdc-layout-grid__cell--span-12' });

                        var name = $('<h3>', { 'class': 'mdc-typography--subheading3' });
                        name.text(stats['name']);
                        row1.append(name);

                        var info = $('<div>', { 'class': 'info' }).html('<strong>Team:</strong> ' + stats['team'] + '<br><strong>GP:</strong> ' + stats['gp'] + '<br><strong>MIN:</strong> ' + stats['min']);
                        var nameArr = stats['name'].split(' ');
                        var pic = 'https://nba-players.herokuapp.com/players/' + nameArr[nameArr.length - 1] + '/' + nameArr[0];
                        row2.append($('<img>', { 'alt': 'Player Picture', 'class': 'nba-picture', 'src': pic}).on('error', function() {
                            this.src='nba-pic.jpg';
                        })).append(info);


                        var table = $('<table>');
                        var trh1 = $('<tr>');
                        var tr1 = $('<tr>');
                        trh1.append($('<th>').text('FGM')).append($('<th>').text('FGA')).append($('<th>').text('FG%')).append($('<th>').text('3PM')).append($('<th>').text('3PA'))
                            .append($('<th>').text('3P%')).append($('<th>').text('FTM')).append($('<th>').text('FTA')).append($('<th>').text('FT%'));
                        tr1.append($('<td>').text(stats['fg_m'])).append($('<td>').text(stats['fg_a'])).append($('<td>').text(stats['fg_pct']))
                            .append($('<td>').text(stats['three_pt_m'])).append($('<td>').text(stats['three_pt_a'])).append($('<td>').text(stats['three_pt_pct']))
                            .append($('<td>').text(stats['ft_m'])).append($('<td>').text(stats['ft_a'])).append($('<td>').text(stats['ft_pct']));

                        var trh2 = $('<tr>');
                        var tr2 = $('<tr>');
                        trh2.append($('<th>').text('OREB')).append($('<th>').text('DREB')).append($('<th>').text('REB')).append($('<th>').text('AST')).append($('<th>').text('TO'))
                            .append($('<th>').text('STL')).append($('<th>').text('BLK')).append($('<th>').text('PF')).append($('<th>').text('PPG'));
                        tr2.append($('<td>').text(stats['reb_off'])).append($('<td>').text(stats['reb_def'])).append($('<td>').text(stats['reb_off'])).append($('<td>').text(stats['misc_ast']))
                            .append($('<td>').text(stats['misc_to'])).append($('<td>').text(stats['misc_stl'])).append($('<td>').text(stats['misc_blk'])).append($('<td>').text(stats['misc_pf']))
                            .append($('<td>').text(stats['misc_ppg']));

                        row2.append(table.append(trh1).append(tr1).append(trh2).append(tr2));

                        player.html(row1.append(row2));
                    } else {
                        player.remove();
                    }
                } catch (err) {
                    player.remove();
                    console.log('Error retrieving nba stats. ' + err);
                }                
            }).fail(function (err) {
                $('#player').remove();
                console.log('Error retrieving nba stats. ' + err);
            });
    }

    //AJAX request to search urls
    function searchCrawler(searchTerm) {
        //send the AJAX call to the server to retrieve search results
        $.ajax({
            url: 'admin.asmx/searchPages',
            type: 'POST',
            data: JSON.stringify({ term: searchTerm }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json'
        })
            //the response from the server
            .done(function (data) {
                var searchResults = resultsDiv();
                try {
                    var results = JSON.parse(data.d);

                    if (results.length == 0) {
                        if (!$('#player')) {
                            searchResults.html('No search results found');
                        }
                    } else {
                        //display the results
                        $.each(results, function (index, obj) {
                            var page = obj.page;
                            var result = $('<div>', { 'class': 'mdc-layout-grid__cell mdc-layout-grid__cell--span-12 result' });
                            var heading = $('<h3>', { 'class': 'mdc-typography--subheading3' }).html(("<a href='" + page.uri + "'>" + page.title + "</a>"));
                            var url = $('<div>', { 'class': 'url' }).text(page.uri);
                            var split = page.desc.split(" ");
                            var date = new Date(page.pubDate);
                            var descString = "<span class='date'>" + date.toDateString() + "</span> - ";
                            //var found = false;
                            for (var i = 0; i < split.length; i++) {
                                var word = split[i].toLowerCase();
                                //$.each(obj.queryWords, function (wordIndex, queryWord) {
                                //    if (word.includes(queryWord)) {
                                //        console.log(queryWord);
                                //        bodyString += " <strong>" + word + "</strong>";
                                //        found = true;
                                //        return false;
                                //    }
                                //});

                                //if (!found) {
                                //    bodyString += " " + word;
                                //}

                                //found = false;

                                if (jQuery.inArray(word, obj.queryWords) != -1) {
                                    descString += " <strong>" + word + "</strong>";
                                } else {
                                    descString += " " + word;
                                }
                            }
                            var div = $('<div>').html(descString);

                            searchResults.append(result.append(heading).append(url).append(div));
                        });
                    }
                } catch (err) {
                    searchResults.text('Error retrieving search results.');
                    console.log('Error retrieving search results. ' + err);
                }
            }).fail(function (err) {
                searchResults.text('Error retrieving search results.');
                console.log('Error retrieving search results. ' + err);
            });
    }

    //AJAX request to save search term to Trie
    function saveSearch(searchTerm) {
        $.ajax({
            url: '/querySuggest.asmx/saveSearch',
            type: 'POST',
            data: JSON.stringify({ term: searchTerm }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json'
        })
            .fail(function (err) {
                console.log('Error saving user search. ' + err);
            });
    }

    var clickSuggestion = (function(val) {
        $('#search').val(val);
        $('#suggestions').remove();
        searchPlayers(val);
        searchCrawler(val);
        saveSearch(val);
    });

    //delay the keyup event for quickly typed characters
    var delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    //check if suggestions div exists, create if not
    //return empty suggestions div
    var suggestDiv = function () {
        //create div to show results in if does not exist
        if ($('#suggestions').length == 0) {
            $('#outer').append($('<div>', { id: 'suggestions', class: 'dropdown-content' }));
        }
        return $('#suggestions').text('');
    };

    //check if player div exists, create if not
    //return empty player div
    var playerDiv = function () {
        //create div to show results in if does not exist
        if ($('#player').length == 0) {
            $('#searchResults').append($('<div>', { id: 'player', class: 'mdc-layout-grid max-width' }));
        }
        return $('#player').text('');
    };

    //check if results div exists, create if not
    //return empty results div
    var resultsDiv = function () {
        //create div to show results in if does not exist
        if ($('#results').length == 0) {
            $('#searchResults').append($('<div>', { id: 'results', class: 'mdc-layout-grid max-width' }));
        }
        return $('#results').text('');
    };

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