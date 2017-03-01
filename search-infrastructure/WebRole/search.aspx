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
    <form id="searchForm" runat="server">
    <div>
        <div class="input-group">
            <div class="mdc-textfield mdc-textfield--upgraded">
                <input type="text" id="search" name="search" class="mdc-textfield__input" placeholder="search" autocomplete="off" required="required"/>
            </div>
            <button id="submit" class="mdc-button mdc-button--raised mdc-button--accent mdc-ripple-upgraded" type="submit">Search</button>
        </div>
    </div>
        
    </form>

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

    <div id="searchResults"></div>
    <div id="results" class="mdc-layout-grid"></div>
</body>
</html>
