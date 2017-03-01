<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="search.aspx.cs" Inherits="WebRole.search" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js"></script>
    <script type="text/javascript" src="search.js"></script>
</head>
<body>
    <form id="Form1" runat="server">
    <div>
        <input id="search" name="search"/>
    </div>
        <button id="submit" type="submit">Search</button>
    </form>
</body>
</html>
