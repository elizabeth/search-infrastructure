'use strict';

$(document).ready(function () {
    $("#searchForm").submit(function (e) {
        e.preventDefault(e);
        $('#submit').blur();
        $('#search').blur();

        searchPlayers($("#search").val());
        ////add loading icon
        //$('#players').html('<i id="spinner" class="fa fa-5x fa-fw fa-refresh fa-spin"></i>')
        //    .promise()
        //    .done(function () {
        //        //search and return players
        //        searchPlayers($("[name='name']").val());
        //    });
    });

    $("#search").keyup(function (e) {
        //prevent ajax call from happening twice if on submit
        if (e.keyCode != 13) {
            //only make ajax call if user stops typing for short time
            delay(function () {
                if ($("#search").val()) {
                    searchPlayers($("#search").val());
                } else {
                    $("#player").remove();
                }
            }, 500);
        }
    });

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

                        var table = $('<table>');
                        var trh1 = $('<tr>');
                        var tr1 = $('<tr>');
                        trh1.append($('<th>').text('Team')).append($('<th>').text('GP')).append($('<th>').text('MIN')).append($('<th>').text('FGM')).append($('<th>').text('FGA'))
                            .append($('<th>').text('FG%')).append($('<th>').text('3PM')).append($('<th>').text('3PA')).append($('<th>').text('3P%')).append($('<th>').text('FTM'))
                            .append($('<th>').text('FTA')).append($('<th>').text('FT%'));
                        tr1.append($('<td>').text(stats['team'])).append($('<td>').text(stats['gp'])).append($('<td>').text(stats['min'])).append($('<tr>').text(stats['fg_m']))
                            .append($('<td>').text(stats['fg_a'])).append($('<td>').text(stats['fg_pct'])).append($('<td>').text(stats['three_pt_m'])).append($('<td>').text(stats['three_pt_a']))
                            .append($('<td>').text(stats['three_pt_pct'])).append($('<td>').text(stats['ft_m'])).append($('<td>').text(stats['ft_a'])).append($('<td>').text(stats['ft_pct']));

                        var trh2 = $('<tr>');
                        var tr2 = $('<tr>');
                        trh2.append($('<th>').text('OREB')).append($('<th>').text('DREB')).append($('<th>').text('REB')).append($('<th>').text('AST')).append($('<th>').text('TO'))
                            .append($('<th>').text('STL')).append($('<th>').text('BLK')).append($('<th>').text('PF')).append($('<th>').text('PPG'));
                        tr2.append($('<tr>').text(stats['reb_off'])).append($('<td>').text(stats['reb_def'])).append($('<td>').text(stats['reb_off'])).append($('<td>').text(stats['misc_ast']))
                            .append($('<td>').text(stats['misc_to'])).append($('<td>').text(stats['misc_stl'])).append($('<td>').text(stats['misc_blk'])).append($('<td>').text(stats['misc_pf']))
                            .append($('<tr>').text(stats['misc_ppg']));

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
                var player = playerDiv();
                player.remove();
                console.log('Error retrieving nba stats. ' + err);
            });
    }

    //delay the keyup event for quickly typed characters
    var delay = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    //check if player div exists, create if not
    //return empty player div
    var playerDiv = function () {
        //create div to show results in if does not exist
        if ($('#player').length == 0) {
            $('#searchResults').append($('<div>', { id: 'player', class: 'mdc-layout-grid' }));
        }
        $('#player').html('');
        return $('#player');
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