﻿'use strict';

$(document).ready(function () {
    $("#search-form").submit(function (e) {
        e.preventDefault(e);
        $('#submit').blur();
        $('#search').blur();

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
            url: 'http://54.213.188.54/search-infrastructure/search-player.php',
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
                console.log(err);
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
});