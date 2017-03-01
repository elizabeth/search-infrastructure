<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="search.aspx.cs" Inherits="WebRole.search" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" class="mdc-typography mdc-theme--background">
<head runat="server">
    <title>Search</title>
    <link rel="stylesheet" href="https://unpkg.com/material-components-web@latest/dist/material-components-web.css"/>
    <link rel="stylesheet" href="search.css" />

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>
    <script type="text/javascript" src="https://unpkg.com/material-components-web@latest/dist/material-components-web.js"></script>
    <script type="text/javascript" src="search.js"></script>
</head>
<body class="mdc-typography--body2">
    <div class="mdc-layout-grid">
        <form id="searchForm" class="mdc-layout-grid__cell mdc-layout-grid__cell--span-12" runat="server">
            <div class="mdc-layout-grid" style="padding-bottom:0">
                <div class="mdc-textfield mdc-textfield--upgraded mdc-layout-grid__cell mdc-layout-grid__cell--span-10" style="margin-bottom:0">
                    <input type="text" id="search" name="search" class="mdc-textfield__input" placeholder="search" autocomplete="off" required="required"/>
                </div>

                <div id="submitDiv" class="mdc-layout-grid__cell mdc-layout-grid__cell--span-2">
                    <button id="submit" class="mdc-button mdc-button--raised mdc-button--accent mdc-ripple-upgraded" type="submit">Search</button>
                </div>
            </div>

            <div class="mdc-layout-grid" style="padding-top:0">
                <div id="outer" class="mdc-layout-grid__cell mdc-layout-grid__cell--span-10" style="margin-top:0"></div>
            </div>
        </form>
    </div>

    <div id="searchResults"></div>

    <script async="async" src="//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
    <!-- eliz -->
    <ins class="adsbygoogle"
         style="display:block"
         data-ad-client="ca-pub-5666480866743556"
         data-ad-slot="5982975420"
         data-ad-format="auto"></ins>
    <script>
    (adsbygoogle = window.adsbygoogle || []).push({});
    </script>
</body>
</html>
