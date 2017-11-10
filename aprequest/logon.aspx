<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="logon.aspx.vb" Inherits="aprequest.logon" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    
    <title>Authenticate</title>
    <style type="text/css">       
#logon, #logon td {
	font-family				: Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetica;
	font-size				: 11pt;
}

input[type=text],input[type=password] {
    padding                 : 0px 3px;
	font-family				: Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetica;
	font-size				: 10pt;
	font-weight             : bold;
}
    </style>
</head>
<body onload="document.getElementById('<%= txtUsername.ClientID %>').focus();">
    <form id="Login" method="post" runat="server">
        <table id="logon" style="width: 400px; margin: 50px auto;">
            <tr><td align="center" colspan="2"><asp:Label ID="errorLabel" Runat="server" ForeColor="#ff3300"></asp:Label></td></tr>
            <tr><td align="right">Username:</td>
                <td><asp:TextBox ID="txtUsername" Runat="server" ></asp:TextBox></td>
                </tr>
            <tr><td align="right">Password:</td>
                <td><asp:TextBox ID="txtPassword" Runat="server" TextMode="Password"></asp:TextBox></td>
                </tr>
            <tr><td align="right">Domain:</td>
                <td><asp:TextBox ID="txtDomain" Runat="server" ></asp:TextBox></td>
                </tr>
            <tr><td></td>
                <td><asp:Button ID="btnLogin" Runat="server" Text="Login" CssClass="button-primary" ></asp:Button></td>
                </tr>
        </table>
    </form>
</body>
</html>