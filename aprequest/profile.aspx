<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/helpers/aprequest.Master" CodeBehind="profile.aspx.vb" Inherits="aprequest.profile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
    <title>AP Request :: Profile</title>
    <script language="javascript" type="text/javascript">
        var ajx = Math.floor((Math.random()*100)+1);
        var m_ntr;
        var d_idx;
        document.observe('dom:loaded', function() {
            $('bottom').hide();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Top" runat="server">
    <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
    Set Signing Password/Phrase: <asp:TextBox id="sigPhrase1" TextMode="Password" runat="server"></asp:TextBox>
    <br />
    <br />
    Upload signature file (jpg/png/gif only!): <asp:FileUpload ID="flub1" runat="server" />
    <br />
    <asp:Button ID="btnGo" runat="server" Text="Update Profile" CssClass="button-primary" />
    <br />
    Current Signature:<br />
    <asp:ImageButton ImageUrl="" ID="btnESig" ClientIDMode="Static" runat="server" />
    <br />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Bottom" runat="server">
&nbsp;
</asp:Content>
