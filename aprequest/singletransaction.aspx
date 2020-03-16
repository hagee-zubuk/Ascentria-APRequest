<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="singletransaction.aspx.vb" Inherits="aprequest.singletransaction" %>
<!doctype html>
<html lang="en">
<head id="Head1" runat="server">
    <meta charset="utf-8">
    <meta name="description" content="Singe Transaction Sheet">
    <meta name="author" content="Zubuk/Hagee">
    <link href="http://webserv8/aprequest/aprequest.css" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Open+Sans+Condensed:300" rel="stylesheet">
    <title></title>
    <style>
body { 
    /* width: 800px; */
    width: 7.75in !important;
    font-size: 10pt;
    margin: 10px auto 30px;
    font-family: Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetice, Sans-serif;
}
table { width: 750px; clear: both; border-collapse: collapse; }
td { font-family: Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetice, Sans-serif; font-size: 9pt; }
td:first-child { text-align: right; }
input[type="text"] { width: 100%; }
.align-center { text-align: center !important; }

.ascbox { width : 750px;
height: 80px;
}

/* GRIDVIEW */
.grid {
    font-family             : 'Open Sans Condensed', Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetica;
    font-size               : 10pt;
    border                  : 1px solid #888;
    border-collapse         : collapse;
    color                   : #333;
    width                   : 100%;
}

.gridheading, .grid th {
    border                  : 1px solid #fdd;
    color                   : Menu;
    padding                 : 4px 2px;
    vertical-align          : bottom;
    text-align              : center;
    background-color        : #a33;
    font-family             : 'Open Sans Condensed', Candara, 'Trebuchet MS', Verdana;
}

.gridheading {
        background-color        : #abb;
}

.grid td {
    color                   : #333;
    border-bottom           : solid 1px #edb;
    padding                 : 4px 2px;
    texct-align             : center;
}

.gridRow {
    background-color        : #dbb;
}

.gridAltRow {
    background-color        : #fcc;
}

.gridEditRow {
    background-color        : #0dd;
}

.gridFooterRow {
    background-color        : #eee;
}

.grid tr.gridRow:hover, .grid tr.gridAltRow:hover {
    font-family             : Calibri, 'Trebuchet MS', Tahoma, Verdana, Arial, Helvetica;
    font-size               : 10pt;
    border                  : solid 1px #888;
    border-collapse         : collapse;
    background-color        : #e9b;
}
.middle {
    border              : 1px dotted #bbb;
    width               : 750px;
    margin              : 10px auto 20px;
}
.barcodesec  {
    float               : right;
    height              : 50px;
    margin-top          : 15px;
    width               : 400px;
    /* border              : 1px dotted #abb !important; */
}
#txtRegnTitle { width: 400px !important; }
textarea#txtVendTitle { min-width: 300px; font-size: 9pt; min-height: 90px; }
    </style>    
</head>
<body>
<div id="ascbox" class="ascbox">
    <img id="Img2" src="http://webserv8/aprequest/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance"
        title="" style="height: 80px; width:286px; float: left;"/>
    <asp:Image ID="imgBC"  runat="server" CssClass="barcodesec" />
</div>
<form id="form1" runat="server">
    <div id="wereok" style="width: 100%; clear: both;">
<table>
    <tr><td>&nbsp;</td>
        <td style="width: 55% !important;">&nbsp;</td>
        <td style="width: 42% !important;">&nbsp;</td></tr>
    <tr><td colspan="3" class="align-center">
        <asp:Label ID="lblMesg" Font-Bold=true runat="server" Text="-" ></asp:Label>
        </td></tr>
    <tr><td>Entity:</td>
        <td><asp:TextBox ID="txtEntiTitle" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtEntiCode" readonly="true" runat="server" /></td>
        </tr>
    <tr><td>Region:</td>
        <td><asp:TextBox ID="txtRegnTitle" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtRegnCode" readonly="true" runat="server" /></td>
        </tr>
<asp:Literal ID="BigSiteName" runat="server" Text=""></asp:Literal>        
    <tr><td valign="top">Vendor:</td>
        <td><asp:TextBox ID="txtVendTitle" rows="2" readonly="true" runat="server" /></td>
        <td valign="top"><asp:TextBox ID="txtVendCode" readonly="true" runat="server" /></td>
        </tr>
    <tr><td>Fiscal Year:</td>
        <td><asp:TextBox ID="txtFiscTitle" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtFiscCode" readonly="true" runat="server" /></td>
        </tr>
    <tr><td>Invoice No:</td>
        <td><asp:TextBox ID="txtInvNo" readonly="true" runat="server" /></td>
        </tr>
    <tr><td></td><td>Invoice Date:</td>
        <td>Due Date:</td></tr>
    <tr><td></td>
        <td><asp:TextBox ID="txtInvDt" readonly="true" runat="server" /></td>
        <td><asp:TextBox ID="txtDueDt" readonly="true" runat="server" /></td>
        </tr>
    <tr><td>Amount:</td>
        <td><asp:TextBox ID="txtAmount" readonly="true" runat="server" /></td>
        <td></td></tr>
    <tr><td>Requester:</td>
        <td ><asp:TextBox ID="txtReqst" readonly="true" runat="server" /></td>
        </tr>
    <tr><td>Approval:</td>
        <td ><asp:TextBox ID="txtApprv" readonly="true" runat="server" />
            <asp:Literal ID="litApprvDate" runat="server"></asp:Literal>
        </td>
        </tr>
</table>
<br />
<asp:Table ID="tblTrans" runat="server" CssClass="middle">
    <asp:TableRow ID="TableRow1" runat="server">
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