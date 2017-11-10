<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="singletransaction.aspx.vb" Inherits="aprequest.singletransaction" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    <title></title>
</head>
<body style="width: 900px; margin: 3px auto;">
<div id="ascbox" class="ascbox">
<img id="Img2" src="~/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance" title="" style="height: 80px; width:286px; float: left;"/>
        <asp:Image ID="imgBC" BorderColor="#aabbbb" BorderWidth="1px" BorderStyle="Dotted" Width="400px" runat="server" CssClass="barcodesec" />
</div>
    <form id="form1" runat="server">
    <div id="wereok" style="width: 100%;">
<table style="width: 100%;">
    <tr><td colspan="3" align="center">
        <asp:Label ID="lblMesg" Font-Bold=true runat="server" Text="-"></asp:Label>
        </td></tr>

    <tr><td align="right">Entity:</td>
        <td><asp:TextBox ID="txtEntiTitle" readonly="true" Width="500px" runat="server" /></td>
        <td><asp:TextBox ID="txtEntiCode" readonly="true" Width="70px" runat="server" /></td>
        </tr>
    <tr><td align="right">Region:</td>
        <td><asp:TextBox ID="txtRegnTitle" readonly="true" Width="500px" runat="server" /></td>
        <td><asp:TextBox ID="txtRegnCode" readonly="true" runat="server" /></td>
        </tr>
<asp:Literal ID="BigSiteName" runat="server" Text=""></asp:Literal>        
    <tr><td align="right" valign="top">Vendor:</td>
        <td><asp:TextBox ID="txtVendTitle" rows="2" readonly="true" Width="500px" runat="server" /></td>
        <td valign="top"><asp:TextBox ID="txtVendCode" readonly="true" runat="server" /></td>
        </tr>
    <tr><td align="right">Fiscal Year:</td>
        <td><asp:TextBox ID="txtFiscTitle" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtFiscCode" readonly="true" runat="server" /></td>
        </tr>
    <tr><td align="right">Invoice No:</td>
        <td><asp:TextBox ID="txtInvNo" readonly="true" runat="server" /></td>
        </tr>
    <tr><td></td><td>Invoice Date:</td>
        <td>Due Date:</td></tr>
    <tr><td></td>
        <td><asp:TextBox ID="txtInvDt" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtDueDt" readonly="true" runat="server" /></td>
        </tr>
    <tr><td align="right">Amount:</td>
        <td><asp:TextBox ID="txtAmount" readonly="true" runat="server" /></td>
        <td></td></tr>
    <tr><td align="right">Requester:</td>
        <td colspan="2"><asp:TextBox ID="txtReqst" readonly="true" Width="400px" runat="server" /></td>
        </tr>
    <tr><td align="right">Approval:</td>
        <td colspan="2"><asp:TextBox ID="txtApprv" readonly="true" Width="400px" runat="server" />
            <asp:Literal ID="litApprvDate" runat="server"></asp:Literal>
        </td>
        </tr>
</table>
<br />
<!-- http://localhost:52283/singletransaction.aspx?txn=735 -->
<asp:Table ID="tblTrans" runat="server" BorderColor="#bbbbbb" BorderStyle="Dotted" BorderWidth="1px" Width="770px" CssClass="middle">
    <asp:TableRow runat="server">
        <asp:TableCell ID="hdSite" CssClass="gridheading" runat="server">Site</asp:TableCell>
        <asp:TableCell ID="hdDept" CssClass="gridheading" runat="server">Dept</asp:TableCell>
        <asp:TableCell ID="hdServ" CssClass="gridheading" runat="server">Serv</asp:TableCell>
        <asp:TableCell ID="hdGLAc" CssClass="gridheading" runat="server">GL Acct</asp:TableCell>
        <asp:TableCell ID="hdPayer" CssClass="gridheading" runat="server">Payer</asp:TableCell>
        <asp:TableCell ID="hdPern" CssClass="gridheading" runat="server">Person</asp:TableCell>
        <asp:TableCell ID="hdAmnt" CssClass="gridheading" runat="server">Amount</asp:TableCell>
    </asp:TableRow>

</asp:Table>
    </div>
    <asp:Literal ID="litSp_Notes" runat="server"></asp:Literal>
    </form>
</body>
</html>
