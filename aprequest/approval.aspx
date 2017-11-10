<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="approval.aspx.vb" Inherits="aprequest.approval" %>
<%@ Register src="helpers/Toplinks.ascx" tagname="Toplinks" tagprefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <link href="~/aprequest.css" rel="Stylesheet" type="text/css" />
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/prototype.js")%>" ></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/scriptaculous.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/modalbox.js")%>"></script>
    <link href="<%= ResolveclientUrl("~/js/modalbox.css")%>" rel="Stylesheet" type="text/css" />
    <title>Request Queue</title>
<script language="javascript" type="text/javascript">
    var ajx = Math.floor((Math.random() * 100) + 1);
    var last_gridcell = "";
    var tmpText = "";
    var last_idx = -1;
/*
function opendetails(chGUID) {
    //alert("Got this: " + chGUID);
    Modalbox.show($('topopup'), { title: 'Details', width: 480 });
}
*/
function cleardetails() {
    $('txt_dept').value = '';
    $('txt_serv').value = '';
    $('txt_glac').value = '';
    $('txt_payr').value = '';
    $('txt_pern').value = '';
    $('txt_amnt').value = '';
    $('code_dept').value = '';
    $('code_serv').value = '';
    $('code_glac').value = '';
    $('code_payr').value = '';
    $('code_pern').value = '';
}

function flag_unapprove(guid, ix) {
    var uurl = '<%=ResolveClientUrl("~/helpers/unapprove.aspx") %>';
    var ajx = new Ajax.Request(uurl, {
        postBody: 'guid=' + guid + '&ix=' + ix,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if ((retData.error > 0) && (retData.error < 16)) {
                alert(retData.message);
            } else {
                var tgt1 = 'txn' + retData.target;
                var tgt2 = 'img_' + retData.target;
                alert('Approval revoked.');
                $(tgt2).hide();
                $(tgt1).show();
                // location.reload();
                // Modalbox.show($('zzz'), { title: 'Approval Revoked', width: 480 });
            }
        }
    });
}

function mkdele(guid) {
    var uurl = '<%= ResolveclientUrl("~/helpers/txn_dele.aspx")%>';
    var ajx = new Ajax.Request(uurl, {
        postBody: 'guid=' + guid,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if (retData.error > 0) {
                alert(retData.message);
            } else {
                location.reload();
            }
        }
    });
}

function modHoldSetting(guid, hld) {
    var uurl = '<%= ResolveclientUrl("~/helpers/sethold.aspx")%>';
    var usrid = $('<%= usrGUID.ClientID %>').value;
    var rison = encodeURI($('txtHoldReason').value);
    var ajx = new Ajax.Request(uurl, {
        postBody: 'guid=' + guid + '&hold=' + hld + '&uid=' + usrid + '&reason=' + rison,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if (retData.error > 0) {
                alert(retData.message);
            } else {
                location.reload();
            }
        }
    });
}

function unhold(guid) {
    modHoldSetting(guid, 0);
}

function hold(guid) {
    $('holdGUID').value = guid;
    Modalbox.show($('holdreasonbox'), { title: 'Hold Reason', width: 530, aspnet: true });
}

function dismissHoldDialog() {
    var guid = $('holdGUID').value;
    modHoldSetting(guid, 1);
    Modalbox.hide();
}

function opendetails(guid) {
    $('txt_dept').value = '';
    $('code_dept').value = '';
    $('txt_serv').value = '';
    $('code_serv').value = '';
    $('txt_glac').value = '';
    $('code_glac').value = '';
    $('txt_payr').value = '';
    $('code_payr').value = '';
    $('txt_pern').value = '';
    $('code_pern').value = '';
    $('txt_amnt').value = '';
    var uurl = '<%= ResolveclientUrl("~/helpers/detailrow_display.aspx")%>';
    var ajx = new Ajax.Request(uurl, {
            postBody: 'guid=' + guid,
            onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
            onSuccess: function(rct) {
                    var retData = rct.responseText.evalJSON();
                    if ((retData.error > 0) && (retData.error < 16)) {
                        alert(retData.message);
                    } else {
                        // success. populate the table
                        $('txt_dept').value = retData.depttitle;
                        $('code_dept').value = retData.deptcode;
                        $('txt_serv').value = retData.servtitle;
                        $('code_serv').value = retData.servcode;
                        $('txt_glac').value = retData.glaccttitle;
                        $('code_glac').value = retData.glacctcode;
                        $('txt_payr').value = retData.payertitle;
                        $('code_payr').value = retData.payercode;
                        $('txt_pern').value = retData.persontitle;
                        $('code_pern').value = retData.person;
                        $('txt_amnt').value = retData.amount;
                        Modalbox.show($('topopup'), { title: 'Details', width: 480 });
                    }
                }
            });
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

function Update_clicked() {
    resetDDsearchboxes();
    Modalbox.hide();
}
function Cancel_clicked() {
    resetDDsearchboxes();
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
        if (qtyp!='amnt') $(tgt).innerHTML = "<input type=\"text\" name=\"" + tbox + "\" id=\"" + tbox + "\" class=\"addbox align-center\" />";
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

/*
function savedetails(ctl_idx, rec_id) {
    var idx = parseInt(ctl_idx);
    if (idx > 0) {
        console.log ("Amount: " + $(amnt + idx).value + "; " +
                "Person: " +
    }
}
*/
// TODO: these must save, really...
function saveZZZ(text, li) {
    var str = new String(li.id);
    //console.log("Control:" + last_gridcell);
    $(last_gridcell).innerHTML = li.textContent;
    console.log("Text: " + text.value + "; Value: " + li.id + "; Content: " + li.textContent);
    var tgt = String(last_gridcell);
    var sv_typ = tgt.substr(0, 4);
    var guid = String('gidZ' + last_idx);
    guid = $(guid).value;
    console.log("GUID: " + guid);
    tmpText = li.textContent;
    var uurl = '<%= ResolveclientUrl("~/helpers/detailrow_savesingle.aspx")%>';
    var postbody = 'guid=' + guid + '&type=' + sv_typ + '&id=' + li.id + '&title=' + li.textContent;
    console.log("url: " + uurl);
    console.log("postbody: " + postbody);

    var ajx = new Ajax.Request(uurl, {
        postBody: postbody,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if ((retData.error > 0) && (retData.error < 16)) {
                alert(retData.message);
            } else {
                //alert(retData.message);
                $(last_gridcell).innerHTML = retData.title;
                last_gridcell = "";
                last_idx = -1;
                tmpText = "";
            }
        }
    });
}

function fetchdept(text, li) {
    //console.log(event.target.id);
    var str = new String(li.id);
    console.log("Control:" + last_gridcell);
    $(last_gridcell).innerHTML = li.textContent;
    last_gridcell = "";
}
function fetchserv(text, li) { var str = new String(li.id); $(last_gridcell).innerHTML = li.textContent; last_gridcell = ""; }
function fetchglac(text, li) { var str = new String(li.id); $(last_gridcell).innerHTML = li.textContent; last_gridcell = ""; }
function fetchpayr(text, li) { var str = new String(li.id); $(last_gridcell).innerHTML = li.textContent; last_gridcell = ""; }
function fetchpern(text, li) { var str = new String(li.id); $(last_gridcell).innerHTML = li.textContent; last_gridcell = ""; }

document.observe('dom:loaded', function() {
    $('autocomplete_choices').hide();
    $$('p.ipecac').invoke('observe', 'click', hndGridClick);
    if ($('<%=errorLabel.ClientID%>').value != '') alert($('<%=errorLabel.ClientID%>').value);
});
</script>
</head>
<body>
<form id="form1" runat="server">
<div id="ascbox" class="ascbox">
<img id="Img2" src="~/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance" title="" style="height: 80px; width:286px; float: left;"/>
<div style="width: 286px; height: 16px; float: right; margin: 20px 20px 0px 0px; font: 10pt bold Calibri, Tahoma, Verdana, Arial, Helvetica; color: #888; text-align: right;">
<asp:Literal ID="datedisp" runat="server"></asp:Literal></div>
<uc1:Toplinks ID="Toplinks1" runat="server" />
</div>
        <asp:HiddenField ID="errorLabel" Runat="server" />
        <asp:HiddenField ID="usrGUID" Runat="server" />
        <div id="top" class="content">
            <div style="width: 50%; float: left;">
                Welcome, <asp:LinkButton ID="ulink" runat="server" Text="Unknown User"></asp:LinkButton>
            </div>
            <div style="width: 100%; text-align: center; clear: both; margin: 0px auto;">
                <h1>Request Queue</h1>
            </div>
            <div style="float:none; clear: both;">&nbsp;</div>
            <div style="text-align: center;">
                <asp:Table ID="tblTrans" runat="server" CssClass="apprmini" >
                </asp:Table>
                <div id="autocomplete_choices" class="autocomplete"></div>
                <asp:Button ID="btnApprove" runat="server" Text="Approve Checked Items" CssClass="button-primary" />
            </div>
        </div>
        <div id="topopup" style="display: none; width: 100%;">
            <table>
                <tr><td align="right">Department:</td>
                    <td>
                    <input type="text" name="txt_dept" id="txt_dept" readonly class="addbox wider" />
                    <input type="text" name="code_dept" id="code_dept" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Service:</td>
                    <td>
                    <input type="text" name="txt_serv" id="txt_serv" readonly class="addbox wider" />
                    <input type="text" name="code_serv" id="code_serv" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">GL Account:</td>
                    <td>
                    <input type="text" name="txt_glac" id="txt_glac" readonly class="addbox wider" />
                    <input type="text" name="code_glac" id="code_glac" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Payer:</td>
                    <td>
                    <input type="text" name="txt_payr" id="txt_payr" readonly class="addbox wider" />
                    <input type="text" name="code_payr" id="code_payr" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Person:</td>
                    <td>
                    <input type="text" name="txt_pern" id="txt_pern" readonly class="addbox wider" />
                    <input type="text" name="code_pern" id="code_pern" readonly class="align-center" style="width: 80px;" tabindex="-1" />
                    </td></tr>
                <tr><td align="right">Amount:</td>
                    <td>
                    <input type="text" name="txt_amnt" id="txt_amnt" readonly class="addbox wider" />
                    </td></tr>
                <tr><td align="right">&nbsp;</td>
                    <td align="center">
                        <input type="button" id="btnCancel" name="btnCancel" value="Close" onclick="Cancel_clicked();" />
                    </td></tr>
            </table>
        </div>
<asp:HiddenField ID="dbg" runat="server" />
    </form>
    <div id="holdreasonbox" style="display: none; width: 500px;">
    <p style="margin: 0px; font: normal 8pt calibri,sans-serif;">Specify a hold reason:</p>
    <input type="hidden" id="holdGUID" name="holdGUID" />
    <textarea id="txtHoldReason" name="txtHoldReason" style="font: normal 9pt sans-serif; width: 95%; height: 100px;"></textarea>
    <input type="button" id="btnHoldReason" value="Hold" onclick="dismissHoldDialog()" />
    </div>
</body>
</html>
