<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="search.aspx.cs" Inherits="admin.search" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search</title>
    <script type="text/javascript" src="search.js"></script>
</head>
<body>
    <form id="search" runat="server">
    <div>
        <input name="search" id="searchInput"/>
    </div>
        <button id="submit" type="submit" class="btn btn-default">Search</button>
    </form>
</body>
</html>
