<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="dashboard.aspx.cs" Inherits="WebRole.dashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" class="mdc-typography mdc-theme--background">
<head runat="server">
    <title>Dashboard</title>
    <link rel="stylesheet" href="https://unpkg.com/material-components-web@latest/dist/material-components-web.css"/>
    <%--<link rel="stylesheet" href="dashboard.css" />--%>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>
    <script type="text/javascript" src="https://unpkg.com/material-components-web@latest/dist/material-components-web.js"></script>
    <script type="text/javascript" src="dashboard.js"></script>
</head>
<body class="mdc-typography--body2">
<%--    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>--%>
    <div class="mdc-layout-grid">
        <div class="mdc-layout-grid__cell mdc-layout-grid__cell--span-12">
            <h3 class="mdc-typography--subheading3">Worker Status: <span id="worker" class="mdc-typography--subheading1"></span></h3>
            <h3 class="mdc-typography--subheading3">CPU Utilization: <span id="cpu" class="mdc-typography--subheading1"></span></h3>
            <h3 class="mdc-typography--subheading3">RAM Available: <span id="ram" class="mdc-typography--subheading1"></span></h3>

            <h3 class="mdc-typography--subheading3"># Words Added: <span id="word-count" class="mdc-typography--subheading1"></span></h3>
            <h3 class="mdc-typography--subheading3">Last Word Added: <span id="last-word" class="mdc-typography--subheading1"></span></h3>

            <h3 class="mdc-typography--subheading3"># URLs Crawled: <span id="urls-crawled" class="mdc-typography--subheading1"></span></h3>
            <h3 class="mdc-typography--subheading3">Queue Size: <span id="queue-size" class="mdc-typography--subheading1"></span></h3>
            <h3 class="mdc-typography--subheading3">Index Size: <span id="index-size" class="mdc-typography--subheading1"></span></h3>
    

            <h3 class="mdc-typography--subheading3">Start Crawling</h3>
            <div>
                <div class="mdc-textfield mdc-textfield--upgraded">
                    <input type="text" class="mdc-textfield__input" name="rootUrl" id="rootUrl" placeholder="root url"/>
                </div>
                <button type="button" id="start" class="mdc-button mdc-button--raised mdc-button--accent mdc-ripple-upgraded">Start</button>
            </div>
            <div id="start-msg" class="mdc-typography--subheading1"></div>

            <h3 class="mdc-typography--subheading3">Get Page Title</h3>
                <div>
                    <div class="mdc-textfield mdc-textfield--upgraded">
                        <input type="text" class="mdc-textfield__input" name="titleUrl" placeholder="url" />
                    </div>
                    <button type="button" id="titleButton" class="mdc-button mdc-button--raised mdc-button--accent mdc-ripple-upgraded">Retrieve</button>
                </div>
            <div id="page-title"  class="mdc-typography--subheading1"></div>


            <h3 class="mdc-typography--subheading3">Clear Index</h3>
            <div class="mdc-textfield mdc-textfield--upgraded">
                <input type="password" class="mdc-textfield__input" name="password" id="password" placeholder="enter password" />
            </div>
            <button class="mdc-button mdc-button--raised mdc-button--accent mdc-ripple-upgraded">Clear</button>
            <div id="clear-msg" class="mdc-typography--subheading1"></div>

            <h3 class="mdc-typography--subheading3">Last 10 URLs</h3>
            <div id="last-ten" class="scroll mdc-typography--subheading1"></div>

            <h3 class="mdc-typography--subheading3">Errors</h3>
            <div id="errors" class="scroll mdc-typography--subheading1"></div>
        </div>
    </div>
</body>
</html>
