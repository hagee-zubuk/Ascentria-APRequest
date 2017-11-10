<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="processedtxns2.aspx.vb" Inherits="aprequest.ProcessedTxns2" %>
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
    var ajx = Math.floor((Math.random() * 100) + 1);
    var last_gridcell = "";
    var tmpText = "";
    var last_idx = -1;

    function cleardetails() {
        $('txt_enti').value = '';
        $('txt_regn').value = '';
        $('txt_site').value = '';
        $('txt_vend').value = '';
        $('txt_fisc').value = '';
        $('txt_amnt').value = '';
        $('txt_invn').value = '';
        $('txt_invd').value = '';
        $('txt_dued').value = '';
        $('code_enti').value = '';
        $('code_regn').value = '';
        $('code_site').value = '';
        $('code_vend').value = '';
        $('detailblock').innerHTML = '';
    }

    function dlpdf(txn) {
        window.open('<%=Resolveclienturl("~/helpers/dlpdf.aspx")%>?txn=' + txn, 'dlpdf',
            'channelmode=0,directories=0,fullscreen=0,height=100,left=10,location=0,menubar=0,resizable=0,scrollbars=0,status=0,titlebar=0,top=10,width=150', false);
    }

    function opendetails(guid) {
        cleardetails();
        var uurl = '<%= ResolveclientUrl("~/helpers/txn_display2.aspx")%>';
        var ajx = new Ajax.Request(uurl, {
            postBody: 'guid=' + guid,
            onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
            onSuccess: function(rct) {
                var retData = rct.responseText.evalJSON();
                if ((retData.error > 0) && (retData.error < 16)) {
                    alert(retData.message);
                } else {
                    // success. populate the table
                    $('txt_enti').value = retData.entititle;
                    $('code_enti').value = retData.enticode;
                    $('txt_regn').value = retData.regntitle;
                    $('code_regn').value = retData.regncode;
                    $('txt_site').value = retData.sitetitle;
                    $('code_site').value = retData.sitecode;
                    $('txt_vend').value = retData.vendtitle;
                    $('code_vend').value = retData.vendcode;
                    $('txt_fisc').value = retData.fyr;
                    $('txt_amnt').value = retData.total;
                    $('txt_invn').value = retData.inv_no;
                    $('txt_invd').value = retData.inv_dt;
                    $('txt_dued').value = retData.due_dt;
                    $('detailblock').innerHTML = retData.detblock;
                    Modalbox.show($('topopup'), { title: 'Details', width: 580 });
                }
            }
        });
    }

    function hndGridClick(event) {
        if (event != null) Event.stop(event);
        var tgt = String(event.target.id);
        if (last_gridcell == "") {
            var qtyp = tgt.substr(0, 4);
            if (qtyp.length != 4) return;
            last_gridcell = tgt;
            console.log(tgt.replace(qtyp, ""));
            last_idx = parseInt(tgt.replace(qtyp, ""));
            var tbox = "txt_DD" + ajx++;
            if (qtyp != 'amnt') $(tgt).innerHTML = "<input type=\"text\" name=\"" + tbox + "\" id=\"" + tbox + "\" class=\"addbox align-center\" />";
            switch (qtyp) {
                case 'dept':
                    new Ajax.Autocompleter(tbox, "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=dept", select: "idElem", afterUpdateElement: saveZZZ });
                    break;
                case 'serv':
                    new Ajax.Autocompleter(tbox, "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=serv", select: "idElem", afterUpdateElement: saveZZZ });
                    break;
                case 'glac':
                    new Ajax.Autocompleter(tbox, "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=glac", select: "idElem", afterUpdateElement: saveZZZ });
                    break;
                case 'payr':
                    new Ajax.Autocompleter(tbox, "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=payr", select: "idElem", afterUpdateElement: saveZZZ });
                    break;
                case 'pern':
                    new Ajax.Autocompleter(tbox, "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=pern", select: "idElem", afterUpdateElement: saveZZZ });
                    break;
                case 'amnt':
                    new Ajax.InPlaceEditor(tgt, '<%= ResolveclientUrl("~/helpers/detailrow_updateamount.aspx")%>', { callback: ipcCallback, onComplete: ipDone });
                    break;
            }
            if (qtyp != 'amnt') $(tbox).focus();
        }
    }

    function observeDDsearchboxes() {
        $('autocomplete_choices').hide();
        // w00t-w00t!
        new Ajax.Autocompleter("txt_edept", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=dept", select: "idElem", afterUpdateElement: fetchdept });
        new Ajax.Autocompleter("txt_eserv", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=serv", select: "idElem", afterUpdateElement: fetchserv });
        new Ajax.Autocompleter("txt_eglac", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=glac", select: "idElem", afterUpdateElement: fetchglac });
        new Ajax.Autocompleter("txt_epayr", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=payr", select: "idElem", afterUpdateElement: fetchpayr });
        new Ajax.Autocompleter("txt_epern", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=pern", select: "idElem", afterUpdateElement: fetchpern });
    }

    function resetDDsearchboxes() {
        $('txt_dept').stopObserving();
        $('txt_serv').stopObserving();
        $('txt_glac').stopObserving();
        $('txt_payr').stopObserving();
        $('txt_pern').stopObserving();
    }

    function Cancel_clicked() {
        // resetDDsearchboxes();
        Modalbox.hide();
    }
    function ipcCallback(form, value) {
        var guid = String('gidZ' + last_idx);
        guid = $(guid).value;
        console.log("GUID: " + guid);
        return 'guid=' + guid + '&amnt=' + escape(value);
    }

    function ipDone(rct) {
        var retData = rct.responseText.evalJSON();
        if ((retData.error > 0) && (retData.error < 16)) {
            alert(retData.message);
        } else {
            //location.reload(true);
        }
    }

document.observe('dom:loaded', function() {
        if ($('<%=errorLabel.ClientID%>').value != '') alert($('<%=errorLabel.ClientID%>').value);
    });
</script>

</head>
<body>
<form id="form1" runat="server">
<div id="ascbox" class="ascbox">
<img id="Img1" src="~/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance" title="" style="height: 80px; width:286px; float: left;"/>
<div style="width: 286px; height: 16px; float: right; margin: 20px 20px 0px 0px; font: 10pt bold Calibri, Tahoma, Verdana, Arial, Helvetica; color: #888; text-align: right;">
<asp:Literal ID="datedisp" runat="server"></asp:Literal></div>
<uc1:Toplinks ID="Toplinks1" runat="server" />
</div>

    <asp:HiddenField ID="errorLabel" Runat="server" />
    <div id="top" class="content">
        <div style="width: 50%; float: left;">
            Welcome, <asp:LinkButton ID="ulink" runat="server" Text="Unknown User"></asp:LinkButton>
        </div>
        <div style="width: 100%; clear: both; text-align: center">
            <h1>Processed Transactions</h1>
        </div>
        <div style="float:none; clear: both;">&nbsp;</div>
        <p>
        &lt;&lt;&nbsp;<a href="<%=Resolveclienturl("~/pt_picker.aspx")%>">Processed Date Picker</a>
        </p>
        <asp:Table ID="tblTrans" runat="server" CssClass="apprmini" ></asp:Table>
    </div>
        <div id="topopup" style="display: none; width: 100%;">
            <table>
                <tr><td align="right">Entity:</td>
                    <td>
                    <input type="text" name="txt_enti" id="txt_enti" readonly class="addbox wider" />
                    <input type="text" name="code_enti" id="code_enti" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Region:</td>
                    <td>
                    <input type="text" name="txt_regn" id="txt_regn" readonly class="addbox wider" />
                    <input type="text" name="code_regn" id="code_regn" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Site:</td>
                    <td>
                    <input type="text" name="txt_site" id="txt_site" readonly class="addbox wider" />
                    <input type="text" name="code_site" id="code_site" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Vendor:</td>
                    <td>
                    <input type="text" name="txt_vend" id="txt_vend" readonly class="addbox wider" />
                    <input type="text" name="code_vend" id="code_vend" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Fiscal Yr:</td>
                    <td><input type="text" name="txt_fisc" id="txt_fisc" readonly class="addbox wider" />
                    </td></tr>
                <tr><td align="right">Total Amt:</td>
                    <td><input type="text" name="txt_amnt" id="txt_amnt" readonly class="addbox wider" />
                    </td></tr>
                <tr><td align="right">Invoice #:</td>
                    <td>
                    <input type="text" name="txt_invn" id="txt_invn" readonly class="addbox wider" />
                    </td></tr>
                <tr><td align="right">Inv Date:</td>
                    <td><input type="text" name="txt_invd" id="txt_invd" readonly class="addbox wider" />
                    </td></tr>
                <tr><td align="right">Due Date:</td>
                    <td><input type="text" name="txt_dued" id="txt_dued" readonly class="addbox wider" />
                    </td></tr>
                <tr><td colspan="2"><div id="detailblock" style="width: 100%;">&nbsp;</div></td></tr>
                <tr><td align="right">&nbsp;</td>
                    <td align="center">
                        <input type="button" id="btnCancel" name="btnCancel" value="Close" onclick="Cancel_clicked();" />
                    </td></tr>
            </table>
        </div>
    </form>
</body>
</html>
