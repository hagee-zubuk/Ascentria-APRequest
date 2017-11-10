<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Toplinks.ascx.vb" Inherits="aprequest.Toplinks" %>
<div style="width: 500px; text-align: right; margin-top: 30px; float: right;">
    <asp:LinkButton cssclass="topmenu" ID="lnkAdmin" runat="server">Admin</asp:LinkButton>
    <asp:LinkButton cssclass="topmenu" ID="lnkExport" runat="server">Export</asp:LinkButton>
    <asp:LinkButton cssclass="topmenu" ID="lnkDones" runat="server">Processed Txns</asp:LinkButton>
    <asp:LinkButton cssclass="topmenu" ID="lnkAppr" runat="server">Queue</asp:LinkButton>
    <asp:LinkButton cssclass="topmenu" ID="lnkMine" runat="server">My Requests</asp:LinkButton>
    <asp:LinkButton cssclass="topmenu" ID="lnkRequest" Visible="false" runat="server">Request</asp:LinkButton>
    &nbsp;&nbsp;::
    <asp:LinkButton cssclass="topb" ID="lnkLogout"  runat="server">Logout</asp:LinkButton>
    ::
</div>