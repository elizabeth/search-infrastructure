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
            <h3 class="mdc-typography--subheading3">Worker Status:</h3>
            <div id="worker" class="mdc-typography--subheading1"></div>
        </div>
    </div>

</body>
</html>
