<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="attachfile.aspx.vb" Inherits="aprequest.attachfile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/prototype.js")%>" ></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/scriptaculous.js")%>"></script>
    <title>Attach File to Transaction</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:FileUpload ID="file1" runat="server" Visible="False" />
        <asp:HiddenField ID="txnid" runat="server" Visible="False" />
        <asp:Button ID="btnGO" runat="server" Text="Upload" CssClass="button-primary" 
            Visible="False" />
        <asp:Button ID="btnOK" runat="server" OnClientClick="window.close(); return false;" 
            Text="Close" Visible="False" CssClass="button-primary" />
    </div>
    </form>
</body>
</html>
