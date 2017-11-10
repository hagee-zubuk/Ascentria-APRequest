<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ForceEMail.aspx.vb" Inherits="aprequest.ForceEMail" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    <asp:TextBox ID="txtGUID" ReadOnly=true runat="server" BorderStyle="Dotted" 
        Width="304px" Wrap="False"></asp:TextBox><br />
    <asp:DropDownList ID="ddTxns" runat="server" Width="248px">
    </asp:DropDownList>
    <asp:Button ID="btnLookup" runat="server" Text="Lookup" />
    <p>
    <asp:Literal ID="litResult1" runat="server"></asp:Literal>
    </p>
    <p>
        <asp:Button ID="btnSend" runat="server" Text="Send" />
    </p>
    </form>
</body>
</html>