<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="pt_picker.aspx.vb" Inherits="aprequest.pt_picker" %>
<%@ Register src="helpers/Toplinks.ascx" tagname="Toplinks" tagprefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/prototype.js")%>" ></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/scriptaculous.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/modalbox.js")%>"></script>
    <link href="<%= ResolveclientUrl("~/js/modalbox.css")%>" rel="Stylesheet" type="text/css" />
    <title>Processed Transactions</title>
<script language="javascript" type="text/javascript">
</script>
</head>
<body>
<form id="frmZZ" runat="server">
<div id="ascbox" class="ascbox">
<img id="Img1" src="~/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance" title="" style="height: 80px; width:286px; float: left;"/>
<div style="width: 286px; height: 16px; float: right; margin: 20px 20px 0px 0px; font: 10pt bold Calibri, Tahoma, Verdana, Arial, Helvetica; color: #888; text-align: right;">
<asp:Literal ID="datedisp" runat="server"></asp:Literal></div>
<uc1:Toplinks ID="Toplinks1" runat="server" />
</div>
    <asp:HiddenField ID="errorLabel" Runat="server" />
    <div id="top" class="content">
            <div style="width: 30%; float: left;">
                Welcome, <asp:LinkButton ID="ulink" runat="server" Text="Unknown User"></asp:LinkButton>
            </div>
            <div style="width: 80%; float: left; text-align: center; margin: 0px auto; clear: both;">
                <h1>Select Criteria for Processed Transactions</h1>
            </div>
            <div style="float:none; clear: both;">&nbsp;</div>
        <div style="margin-top: 10px; text-align: center;">
        <table>
            <tr><td style="text-align: right;">Select month:&nbsp;</td><td>
                <asp:DropDownList ID="ddMonth" runat="server"/></td></tr>
            <tr><td style="text-align: right;">Entity:&nbsp;</td><td>
                <asp:DropDownList ID="ddEntity" runat="server" Width="100%"/></td></tr>
            <tr><td style="text-align: right;">Region:&nbsp;</td><td>
                <asp:DropDownList ID="ddRegion" runat="server" Width="100%"/></td></tr>
            <tr><td></td><td><asp:Button ID="btnGo" runat="server" Text="Go" Width="80px" /></td></tr>
        </table>
        </div>
    </div>
    </form>
</body>
</html>
