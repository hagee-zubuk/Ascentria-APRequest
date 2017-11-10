<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="logviewer.aspx.vb" Inherits="aprequest.logviewer" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/prototype.js")%>" ></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/scriptaculous.js")%>"></script>
    <title>Activity Log</title>
<script language="javascript" type="text/javascript">
</script>    
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Table ID="tblLog" runat="server" CssClass="vwlog" >
            <asp:TableHeaderRow>
                <asp:TableHeaderCell>Date/Time</asp:TableHeaderCell>
                <asp:TableHeaderCell>User</asp:TableHeaderCell>
                <asp:TableHeaderCell>Event</asp:TableHeaderCell>
                </asp:TableHeaderRow>
        </asp:Table>
    </div>
    </form>
</body>
</html>
