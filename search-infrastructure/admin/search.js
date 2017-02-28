'use strict';

$(document).ready(function () {
    $("#search").submit(function (e) {
        e.preventDefault(e);
        $('#submit').blur();
        $('#searchInput').blur();

        //$('#submit').blur();
        ////add loading icon
        //$('#players').html('<i id="spinner" class="fa fa-5x fa-fw fa-refresh fa-spin"></i>')
        //    .promise()
        //    .done(function () {
        //        //search and return players
        //        searchPlayers($("[name='name']").val());
        //    });
    });

    $("#searchInput").keyup(function (e) {
        //prevent ajax call from happening twice if on submit
        if (e.keyCode != 13) {
            //only make ajax call if user stops typing for short time
            delay(function () {
                if ($("#input").val()) {
                    searchPlayers($("#input").val());
                } else {
                    //$("#results").remove();
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
            url: 'http://54.213.188.54/search-infrastructure/search-player.phh',
            type: 'GET',
            data: { 'search': searchTerm },
            dataType: 'jsonp'
        })
            //The response from the server
            .done(function (data) {
                console.log(data);
                //$("#players").html(data);
            }).fail(function (err) {
                //$("#players").html("Error retrieving data. Please try again later.");
            });
    }
});