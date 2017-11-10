<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/helpers/aprequest.Master" CodeBehind="editreq.aspx.vb" Inherits="aprequest.editreq" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
    <title>AP Request :: Home</title>
    <script language="javascript" type="text/javascript" src="<%= ResolveClientUrl("~/js/calendarview.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveClientUrl("~/js/window.js")%>"> </script>
    <script type="text/javascript" src="<%= ResolveClientUrl("~/js/window_effects.js")%>"> </script>
    <link href="<%= ResolveClientUrl("~/js/default.css")%>" rel="stylesheet" type="text/css" />
    <link href="<%= ResolveClientUrl("~/js/alert.css")%>" rel="stylesheet" type="text/css" />
<script language="javascript" type="text/javascript">
var ajx = Math.floor((Math.random() * 100) + 1);
var winUL = null;
var m_ntr;
var d_idx;
var Approve_Clicked=0;
function fetchEntity(text, li) { var str = new String(li.id); $('code_entity').value = str; $('txtRegiCode').focus(); }
function fetchRegion(text, li) { var str = new String(li.id); $('code_region').value = str; if ($('txtSiteCode') != undefined) {$('txtSiteCode').focus();} else { $('txtVendor').focus();} }
function fetchSite(text, li) { var str = new String(li.id); if( $('code_site')!=undefined ) $('code_site').value = str; $('txtVendor').focus(); }
function fetchVendor(text, li) {
	// li.id contains the value;
	var str = new String(li.id);
	$('vendorid').value = str;
	$('<%=ddFiscalYr.ClientID %>').focus();
}

function combocheck() {
    var zzz = false;
    //try  {    
        var uurl = '<%=ResolveclientUrl("~/helpers/checkcomboentry.aspx")%>';
        ajx++;
        var xxx = new String('ajx=' + ajx + '&ent=' + $('code_entity').value);
        xxx = xxx + '&mul=' + $('<%= blnMultiSite.ClientID%>').value;
        xxx = xxx + '&reg=' + $('code_region').value;
        if ($('txtASite') != undefined) {
            xxx = xxx + '&sit=' + $('txtASite').value;
        } else {
            if ($('code_site') != undefined) xxx = xxx + '&sit=' + $('code_site').value;
        }
        xxx = xxx + '&payr=' + $('<%=txtAPayr.ClientID %>').value;
        xxx = xxx + '&serv=' + $('<%= txtAServ.ClientID %>').value;
        xxx = xxx + '&dept=' + $('<%= txtADept.ClientID %>').value;
        //
        console.log("--attempt combocheck--");
        msgbox = false;
        var sjx = new Ajax.Request(uurl, {
                postBody: xxx,
                asynchronous: false,
                onFailure: function(rct) {
                        Dialog.alert(rct.responseText, { width: 300, height: 100, okLabel: "close" });
                    },
                onSuccess: function(rct) {
                        var retData = rct.responseText.evalJSON();
                        console.log("combocheck error: " + retData.error);
                        if (retData.error > 0) {
                            msgbox = false;
                            Dialog.alert(retData.message, { width: 300, height: 100, okLabel: "close" });
                        } else {
                            msgbox = true;
                        }
                        console.log("combocheck returning: " + msgbox);
                        return(msgbox)
                    }
            });
        var rct = new String(sjx.transport.responseText);
        var retData = rct.evalJSON();        
        if (retData.error > 0) {
            Dialog.alert(retData.message, { width: 300, height: 100, okLabel: "close" });
        } else {
            zzz = true;
        }
        console.log("combocheck returning: " + zzz);
    //} catch (ex) {
        //console.log(ex.message + ' line:' + ex.lineNumber);
        //zzz = false;
    //}
    return(zzz)
}


function entriescomplete_addblock() {
    var zzz = true;
    zzz = zzz && ($('<%= txtADept.ClientID %>').value != "");
    zzz = zzz && ($('<%= txtAServ.ClientID %>').value != "");
    zzz = zzz && ($('<%= txtAGlac.ClientID %>').value != "");
    zzz = zzz && ($('<%= txtAPayr.ClientID %>').value != "");
    zzz = zzz && ($('<%= txtAPern.ClientID %>').value != "");
    zzz = zzz && ($('<%= txtAAmnt.ClientID %>').value != "");
    // console.log("entriescomplete_addblock returns " + zzz);
/*    try {
        var amt = new Number($('<%= txtAAmnt.ClientID %>').value);
        zzz = zzz && (amt > 0);
        if (zzz) $('<%= txtAAmnt.ClientID %>').value = amt.toFixed(2);
    } catch (ex) {
        zzz = false;
    }
*/    
    return(zzz);
}

function hndDelRow(oDel) {
    var sCtl = new String(oDel.id);
    // get idx
    var idx = sCtl.replace("del_", "");
    var sNam = 'drg_' + idx;
    var guid = "";
    try {
        guid = $(sNam).value;
    } catch(ex) {
    }
    if (guid == "") return false;
    // delete the row
    d_idx = idx;
    var uurl = '<%= ResolveclientUrl("~/helpers/detailrow_del.aspx")%>';
    var ajx = new Ajax.Request(uurl, {
            postBody: 'guid=' + guid,
            onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
            onSuccess: function(rct) {
                var retData = rct.responseText.evalJSON();
                if ((retData.error > 0) && (retData.error < 16)) {
                    alert(retData.message);
                } else {
                    // delete the row on the display
                    var dNam = 'trow_' + d_idx;
                    Element.remove(dNam);
                }
            }
        });    
}

function hndSaveCB(rct) {
    var retData = rct.responseText.evalJSON();
    if ((retData.error > 0) && (retData.error < 16)) {
        $('txnDetails').appear();
        $('bottom').show();
        alert(retData.message);
    } else {
        $('weredone').show();
        $('realtxn').innerHTML = retData.txn_no;
        /*
        $('txtEntiCode').value = retData.entity;
        $('txtRegiCode').value = retData.region;
        $('txtSiteCode').value = retData.site;
        $('txttVendor').value = retData.vendor;
        $('footerbuttons').hide();
        //$('bottom').hide();
        */
        $('txnblock').hide();
        $('txnDetails').hide();
        $('bottom').hide();
    }
}

function hndSaveAll(event) {
    var sData = $('aspnetForm').serialize();
    Event.stop(event);
    console.log("At: hndSaveAll");
    // TODO: check inputs!
    var errMesg = new String("");
    var blnE = 0;
    if ($('txtTotalAmount').value == '') {
        errMesg += '<li>Request amount is zero</li>';
        blnE++;
    }
    if ($('txtTotalAmount').value != '') {
        var strTmp = new String($('txtTotalAmount').value);
        strTmp = strTmp.replace(',', '');
        var fltAmt = parseFloat(strTmp);
        if (fltAmt <= 0) {
            errMesg += '<li>Invalid request amount</li>';
            blnE++;
        }
    }
    var dateRegEx = /^(0[1-9]|1[012]|[1-9])[- /.](0[1-9]|[12][0-9]|3[01]|[1-9])[- /.](19|20)\d\d$/;
    if ($('txtInvDate').value == '') {
        errMesg += "<li>Invoice date must be filled in</li>";
        blnE++;
    } else {
        if ($('txtInvDate').value.match(dateRegEx) === null) {
            errMesg += '<li>Invalid Invoice date</li>';
            blnE++;
        }
    }
    if ($('txtDueDate').value == '') {
        errMesg += "<li>Due date must be filled in</li>";
        blnE++;
    } else {
        if ($('txtDueDate').value.match(dateRegEx) === null) {
            errMesg += '<li>Invalid Due date</li>';
            blnE++;
        }
    }
    if ($('code_entity').value == '') {
        errMesg += "<li>Entity value is invalid</li>";
        blnE++;
    }
    if ($('code_region').value == '') {
        errMesg += "<li>Region value is invalid</li>";
        blnE++;
    }
    if($('code_site') != undefined ){
        if ($('code_site').value == '') {
            errMesg += "<li>Site name is invalid</li>";
            blnE++;
        }
    }
    if ($('vendorid').value == '') {
        errMesg += "<li>Vendor Name value is invalid</li>";
        blnE++;
    }
    if ($('txtNote').value == '') {
        errMesg += "<li>Please specify a note</li>";
        blnE++;
    }
    
    // throw an alert if needed
    if (blnE > 0) {
        errMesg = '<ul>' + errMesg.replace('\n', '<br />');
        errMesg += '</ul><br /><br />Please correct and try again.';
        Dialog.alert(errMesg, { width: 300, height: 170, okLabel: "close" });
        return(false);
    }
    if ($('<%= txtAAmnt.ClientID %>').value != '') {
        console.log('firing an add-row for good measure');
        hndAddNewRow(null);
    }
    // alert(sData);
    sData = sData + '&guid=' + $('<%= txnGUID.ClientID %>').value;
    sData = sData + '&ddfiscalyr=' + $('<%= ddFiscalYr.ClientID %>').value;
    var uurl = '<%= ResolveclientUrl("~/helpers/txns_add.aspx")%>';
    var ajx = new Ajax.Request(uurl, {
            postBody: sData,
            onFailure: function(rct) {
                    Dialog.alert(retData.message, { width: 300, okLabel: "close" });
                },
            onSuccess: hndSaveCB
        });
    $('txnDetails').fade();
    $('bottom').hide();

}

function hndShowAdd(event) {
    if (event != null) Event.stop(event);
    var zzz = entriescomplete_addblock();
    if (zzz) { $('<%= btnAddRow.ClientID %>').show() } else { $('<%= btnAddRow.ClientID %>').hide() };
    // TODO: move to next element
}
//http://madrobby.github.io/scriptaculous/ajax-inplaceeditor/
function prepNewRow() {
    ajx++;
    var xxx = new String('ajx=' + ajx + '&txn=' + $('<%= txnGUID.ClientID %>').value);
    xxx = xxx + '&dept=' + $('<%= txtADept.ClientID %>').value;
    xxx = xxx + '&serv=' + $('<%= txtAServ.ClientID %>').value;
    xxx = xxx + '&glac=' + $('<%= txtAGlac.ClientID %>').value;
    xxx = xxx + '&payr=' + $('<%= txtAPayr.ClientID %>').value;
    xxx = xxx + '&pern=' + $('<%= txtAPern.ClientID %>').value;
    if ($('txtASite') != undefined) {
        xxx = xxx + '&site=' + $('txtASite').value;
    }
    var famt = "";
    try {
        var amt = new Number($('<%= txtAAmnt.ClientID %>').value);
        if (amt > 0) $('<%= txtAAmnt.ClientID %>').value = amt.toFixed(2);
    } catch (ex) {
        $('<%= txtAAmnt.ClientID %>').value = 0;
    }
    xxx = xxx + '&amnt=' + $('<%= txtAAmnt.ClientID %>').value;

    try {
    } catch (ex) {
    }
    return (xxx);
}

function hndSetupUpload(event) {
    Event.stop(event);
    var uurl = '<%=ResolveclientUrl("~/attachfile.aspx")%>?txnid=' + $('<%= txnGUID.ClientID %>').value;
    winUL = window.open(uurl, 'upld', 
        'channelmode=0,directories=0,fullscreen=0,height=200,' + 
        'left=10,location=0,menubar=0,resizable=0,scrollbars=1,' +
        'titlebar=0,toolbar=0,top=10,width=400');
}

function hndAddNewRow(event) {
    if (event != null) Event.stop(event);
    var zzz = entriescomplete_addblock();
    if (zzz) {
        zzz = combocheck();
    } else {
        Dialog.alert("Input incomplete for new detail row.\n\nPlease correct and try again", { width: 300, height: 100, okLabel: "close" });
        //alert("Input incomplete for new detail row.\n\nPlease correct and try again");
    }
    if (zzz) {
        var xxx = prepNewRow();
        var uurl = '<%= ResolveclientUrl("~/helpers/detailrow_add.aspx")%>';
        var ajx = new Ajax.Request(uurl, {
            postBody: xxx,
            onFailure: function(rct) {
                    Dialog.alert(rct.responseText, { width: 300, height: 100, okLabel: "close" });
                },
            onSuccess: function(rct) {
                var retData = rct.responseText.evalJSON();
                if ((retData.error > 0) && (retData.error < 16)) {
                    Dialog.alert(retData.message, { width: 300, height: 100, okLabel: "close" });
                } else {
                    m_ntr = new Element('tr', { 'id':'trow_'+retData.cid,'class': 'gridRow' });
                    m_ntr.insert(new Element('td', { 'class': 'align-center' }).update('<input type="hidden" id="drg_' +
                            retData.cid + '" value="' + retData.guid + '">'));
                    if ($('txtASite') != undefined) {
                        m_ntr.insert(new Element('td').update(retData.Site));
                    }
                            // + TODO: add edit and copy functionality
                            //'<img src="images/info.png" alt="edit" id="edt_' + retData.cid + '" />&nbsp;' +
                            //'<img src="images/add.png" alt="copy" id="cpy_' + retData.cid + '" />'));
                    m_ntr.insert(new Element('td').update(retData.Dept));
                    m_ntr.insert(new Element('td').update(retData.Serv));
                    m_ntr.insert(new Element('td').update(retData.GLAc));
                    m_ntr.insert(new Element('td').update(retData.Payr));
                    m_ntr.insert(new Element('td').update(retData.Pern));
                    m_ntr.insert(new Element('td', { 'class': 'align-right' }).update(retData.Amnt));
                    m_ntr.insert(new Element('td', { 'class': 'align-center' }).update('<img src="images/recycle_bin.png" alt="copy" id="del_' + retData.cid + '" onclick="hndDelRow(this)"; />'));
                    $('addRowSecn').insert({ before: m_ntr });
                    $('txtTotalAmount').value = retData.Total
                    m_ntr = null;

                    $('<%= txtADept.ClientID %>').value = "";
                    $('<%= txtAServ.ClientID %>').value = "";
                    $('<%= txtAGlac.ClientID %>').value = "";
                    $('<%= txtAPayr.ClientID %>').value = "";
                    $('<%= txtAPern.ClientID %>').value = "";
                    $('<%= txtAAmnt.ClientID %>').value = "";
                    if ($('txtASite') != undefined) {
                        $('txtASite').value = "";
                    }
                    $('<%= btnAddRow.ClientID %>').hide();
                    // alert(retData.message);
                }
            }
        });
    }
}
/*
function enaUL(event) {
    if (event != null) Event.stop(event);
    $('<%=btnAttach.ClientID%>').hide();
    $('divUL').innerHTML = "<form id=\"frm_ul\" name=\"frm_ul\">" +
                "<input type=\"file\" name=\"datafile\" id=\"datafile\" /><br />" +
                "<input type=\"hidden\" name=\"ud_txn\" id=\"ud_txn\" value=\"" +
                $('<%=txnNumber.ClientID %>').value + "\" />" +
                "<input type=\"button\" value=\"upload\" id=\"ud_upbtn\" name=\"ud_upbtn\" />" +
                "<div id=\"upload\"></div></form>";
    $('divUL').show();
    $('ud_upbtn').observe('click', uploadFz);
}
*/
function checkSigKeyUp(vntZ) {
    var uurl = '<%= ResolveclientUrl("~/helpers/checksignature.aspx")%>';
    var xxx = 'val=' + vntZ; 
    var ajx = new Ajax.Request(uurl, {
        postBody: xxx,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if ((retData.error > 0) && (retData.error < 16)) {
                alert(retData.message);
            } else {
                $('<%= btnSubmit.ClientID%>').show();
            }
        }
    });
}

function mvNext0(text, li) { $('<%=txtADept.ClientID %>').focus(); }
function mvNext1(text, li) { $('<%=txtAServ.ClientID %>').focus(); }
function mvNext2(text, li) { $('<%=txtAGLAc.ClientID %>').focus(); }
function mvNext3(text, li) { $('<%=txtAPayr.ClientID %>').focus(); }
function mvNext4(text, li) { $('<%=txtAPern.ClientID %>').focus(); }
function mvNext5(text, li) { $('<%=txtAAmnt.ClientID %>').focus(); }

function hndDetailKeyPress(e) {
    if (e.keyCode == Event.KEY_RETURN) {
        var element = Event.element(e);
        console.log('Code: [' + e.keyCode + '] from: [' + element + ']');
        Event.stop(e);
        console.log('Got ENTER');
        hndAddNewRow(null);
    }
}

function uploadFz(fm) {
    //fileUpload('recvfileupload.aspx', 'upload');
    return(false);
}
//frmUL
document.observe('dom:loaded', function() {
    if ($('<%=defaulthash.ClientID %>').value != '') {
        var dflRaw = $('<%=defaulthash.ClientID %>').value;
        var dflData = dflRaw.evalJSON();
        if (dflData.result > 0) {
            $('txtEntiCode').value = dflData.entitytitle;
            $('code_entity').value = dflData.entitycode;
            $('txtRegiCode').value = dflData.regiontitle;
            $('code_region').value = dflData.regioncode;
            if ( $('txtSiteCode') != undefined ) {
                $('txtSiteCode').value = dflData.sitetitle;
                $('code_site').value = dflData.sitecode;
            }
            $('txtVendor').value = dflData.vendortitle;
            $('vendorid').value = dflData.vendorcode;
            $('txtInvNo').value = dflData.invoice_no;
            $('txtInvDate').value = dflData.invoice_dt;
            $('txtDueDate').value = dflData.due_dt;
            $('txtTotalAmount').value = dflData.totalamnt;
            $('txtNote').value = dflData.note;
            $('txtAPNote').value = dflData.spnote;
            $('txtVendor').focus();
        } else {
            $('txtEntiCode').focus();
        }
    } else {
        $('txtEntiCode').focus();
    }
    $('autocomplete_choices').hide();
    new Ajax.Autocompleter("txtEntiCode", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { parameters: "type=entity", paramName: "value", minChars: 2, afterUpdateElement: fetchEntity });
    new Ajax.Autocompleter("txtRegiCode", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=region", afterUpdateElement: fetchRegion });
    if ( $('txtSiteCode') != undefined ) new Ajax.Autocompleter("txtSiteCode", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 1, parameters: "type=site", afterUpdateElement: fetchSite });
    new Ajax.Autocompleter("txtVendor", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 3, parameters: "type=vendor", afterUpdateElement: fetchVendor });

    new Ajax.Autocompleter("<%= txtADept.ClientID %>", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=dept", select: "idElem", afterUpdateElement: mvNext1 });
    new Ajax.Autocompleter("<%= txtAServ.ClientID %>", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=serv", select: "idElem", afterUpdateElement: mvNext2 });
    new Ajax.Autocompleter("<%= txtAGLAc.ClientID %>", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=glac", select: "idElem", afterUpdateElement: mvNext3 });
    new Ajax.Autocompleter("<%= txtAPayr.ClientID %>", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=payr", select: "idElem", afterUpdateElement: mvNext4 });
    new Ajax.Autocompleter("<%= txtAPern.ClientID %>", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=pern", select: "idElem", afterUpdateElement: mvNext5 });
    if ($('txtASite') != undefined) {
        new Ajax.Autocompleter("txtASite", "autocomplete_choices", '<%= ResolveclientUrl("~/helpers/SageDBProxy.aspx")%>', { paramName: "value", minChars: 2, parameters: "type=site", select: "idElem", afterUpdateElement: mvNext0 });
    }
    Calendar.setup({ dateField: 'txtInvDate', triggerElement: 'btnCalendar' });
    Calendar.setup({ dateField: 'txtDueDate', triggerElement: 'btnCalDue' });

    $('<%= btnAddRow.ClientID %>').observe('click', hndAddNewRow);
    //$('<%= btnAddRow.ClientID %>').hide();
    $('<%=btnAttach.ClientID%>').observe('click', hndSetupUpload);
    hndShowAdd(null);
    $$('input.addbox').invoke('observe', 'blur', hndShowAdd).invoke('observe', 'change', hndShowAdd);
    $$('input.addbox').invoke('observe', 'keypress', hndDetailKeyPress);
    $('<%=txtAAmnt.ClientID %>').observe('keyup', hndShowAdd);
    $('aspnetForm').observe('submit', function(e) {
            if (Approve_Clicked ==0) {
                Event.stop(e);
                console.log('got ENTER');
                hndSaveAll(e);
            }
        });
    //$('txtFiscalYr').value = $('<%=FiscalYr.ClientID %>').value;
    $('txn').value = $('<%=txnNumber.ClientID %>').value;
    if (document.getElementById('<%= btnApprove.ClientID%>') != null) {
        $('<%= btnApprove.ClientID%>').observe('click', function() {
            Approve_Clicked = 1;
        });
    }
    $('<%= btnSubmit.ClientID%>').observe('click', function(e) {
        Approve_Clicked = 0;
        hndSaveAll(e);
    });

    /*      fileUpload(this.form,'recvfileupload.aspx','upload'); 
    *      return false;
    */
    $('<%= btnSubmit.ClientID%>').show();

    //$('<%=btnAttach.ClientID%>').observe('click', enaUL);
    if ($('<%=errorLabel.ClientID%>').value != '') alert($('<%=errorLabel.ClientID%>').value);
});
</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Top" runat="server">
    <div id="txnDetails">
    <asp:HiddenField ID="txnGUID" runat="server" />
    <asp:HiddenField ID="txnNumber" runat="server" />
    <asp:HiddenField ID="defaulthash" runat="server" />
    <asp:HiddenField ID="blnMultiSite" runat="server" />
    <asp:HiddenField ID="FiscalYr" runat="server" />
    <asp:HiddenField ID="errorLabel" Runat="server" />
    <input type="hidden" id="txn" name="txn" value="" />
    <table id="multinavi" style="width: 700px;">
    <tr><td align="right">Entity Name:</td>
        <td colspan="2"><input type="text" name="txtEntiCode" id="txtEntiCode" style="width: 100%;" />
        <div id="autocomplete_choices" class="autocomplete"></div>
        </td>
        <td><input type="text" name="code_entity" id="code_entity" readonly class="align-center" style="width: 80px;" tabindex="-1" /></td>
        </tr>
    <tr><td align="right">Region Name:</td>
        <td colspan="2"><input type="text" name="txtRegiCode" id="txtRegiCode"  style="width: 100%;" /></td>
        <td><input type="text" name="code_region" id="code_region" readonly class="align-center" style="width: 80px;" tabindex="-1" /></td>
        </tr>
<asp:Literal ID="BigSiteName" runat="server" Text=""></asp:Literal>
    <tr><td align="right">Vendor ID:</td>
        <td colspan="2"><input type="text" name="txtVendor" id="txtVendor" style="width: 100%;" /></td>
        <td><input type="text" name="vendorid" id="vendorid" readonly class="align-center" style="width: 80px;" tabindex="-1" /></td>
        </tr>
    <tr><td></td>
        <td></td>
        <td align="right">Fiscal Year</td>
        <td><!-- input type="text" name="txtFiscalYr" id="txtFiscalYr" value="" style="width: 80px;" class="align-center" /* -->
        <asp:DropDownList ID="ddFiscalYr" runat="server">
            </asp:DropDownList>
        </td>
        </tr>
    <tr><td align="right">Invoice No:</td>
        <td><input type="text" name="txtInvNo" id="txtInvNo" class="align-center" /></td>
        <td align="right">Total Amount:</td>
        <td><input type="text" id="txtTotalAmount" name="txtTotalAmount" class="align-center" tabindex="-1" readonly="readonly" /></td>
        </tr>
    <tr><td align="right">Invoice Date:</td>
        <td><input type="text" name="txtInvDate" id="txtInvDate" class="align-center" style="width: 100px;" />
            <div id="calContainer" class="calbutton" >
                <img src="images/calendar.png" id="btnCalendar" alt="" />
            </div>
            </td>
        <td align="right">Due Date:</td>
        <td><input type="text" name="txtDueDate" id="txtDueDate" class="align-center" style="width: 100px;" />
            <div id="calDue" class="calbutton" >
                <img src="images/calendar.png" id="btnCalDue" alt="" />
            </div>
            </td></tr>
    </table>
    </div>
    <div id="weredone" style="display: none;">
        <p></p>
        <div>Transaction number <div id="realtxn" style="display: inline-block; font-weight: bold;">xxx</div> has been updated</div>
        <br />
        <p>Click here to <a href="default.aspx">start a new request</a></p>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Bottom" runat="server">
    <div id="detailbuttons" style="padding: 0px; margin: 0px auto; width: 715px; float: none; clear: both;" > 
        <div id="textbuttons" 
            style="display: none; width: 400px; padding-left: 70px; float: left; margin: 0px; text-align: center;">
            <asp:Button ID="btnAdd" runat="server" Text="Add" CssClass="button-primary" />
            <asp:Button ID="btnCopy" runat="server" Text="Copy" CssClass="button-primary" />
            <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="button-primary" />
            <asp:Button ID="btnDel" runat="server" Text="Delete" CssClass="button-primary" />
        </div>
        <div id="imgbuttons" style="text-align: right; width: 160px; float: right; margin: 0px;" >
            <asp:ImageButton ImageUrl="~/images/paperclip.gif" ID="btnAttach" TabIndex="-1" ClientIDMode="Static" runat="server" />
            <div id="divUL" name="divUL" style="display: none;" >
            </div>
        </div>
    </div>
    <div id="gvw" style="float: none; clear: both; margin: 0px auto; height: 200px; min-height: 200px; width: 730px;">	
        <!-- We do it this way because the way Microsoft wants me to do it REALLY FRACKING SUCKS! You  hear me there at Redmond??? -->
        <table class="grid" cellspacing="0" rules="all" id="gvEG" name="gvEG" style="border-collapse: collapse;">
            <tr class="gridRow"><th style="width: 25px;">&nbsp;</th>
<asp:Literal ID="litHeader" runat="server" Text=""></asp:Literal>
                <th style="width: 80px;" align="center" scope="col">Department</th>
                <th style="width: 80px;" align="center" scope="col">Service</th>
                <th style="width: 80px;" align="center" scope="col">GL Account</th>
                <th style="width: 80px;" align="center" scope="col">Payer</th>
                <th style="width: 80px;" align="center" scope="col">Person</th>
                <th style="width: 80px;" align="center" scope="col">Amount</th>
                <th style="width: 45px">&nbsp;</th></tr>
            <asp:Literal ID="DetailsTable" runat="server"></asp:Literal>                
            <tr class="gridFooterRow" id="addRowSecn">
                <td style="width: 25px;">&nbsp;</td>
<asp:Literal ID="litBody" runat="server" Text=""></asp:Literal>                
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtADept" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" runat="server" ></asp:TextBox></td>
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtAServ" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" runat="server"></asp:TextBox></td>
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtAGLAc" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" runat="server"></asp:TextBox></td>
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtAPayr" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" runat="server"></asp:TextBox></td>
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtAPern" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" runat="server"></asp:TextBox></td>
                <td style="width: 80px;" align="center"><asp:TextBox ID="txtAAmnt" AutoCompleteType="None" CssClass="addbox" ClientIDMode="Static" Width="80px" runat="server"></asp:TextBox></td>
                <td align="center" style="width: 45px;">
                    <asp:ImageButton ID="btnAddRow" AlternateText="Add" ImageUrl="~/images/accept.png" Height="16px" Width="16px" ToolTip="Add" ClientIDMode="Static" runat="server" />
                </td>
                </tr>
        </table>
        <div id="notesection" style="width: 80%; margin: 5px auto; float: none; clear: both;">
	    Invoice Description:<br />
	    <input type="text" maxlength="50" id="txtNote" name="txtNote" style="width: 100%; height: 40px; padding-top: 5px;"/>
	    <br />
        Special Notes for A/P:<br />
	    <input type="text" maxlength="100" id="txtAPNote" name="txtAPNote" style="width: 100%; height: 40px; padding-top: 5px;"/>
	    </div>
	    <br style="clear: both;" />
        </div>
        <div id="footleft"
            style="margin-top: 20px; width: 142px; padding-right: 10px; float: left; text-align: left;">
                    <asp:Literal ID="litApproved" runat="server"></asp:Literal>
                    <asp:Button ID="btnApprove" name="btnApprove" runat="server" Text="Approve" Height="25px" CssClass="button-primary" ClientIDMode="Static" />
        </div>
        <div id="footright" 
            style="margin-top: 20px; width: 112px; padding-right: 10px; float: right; text-align: right;">
            <asp:Button ID="btnSubmit" runat="server" Text="Update Request" Height="25px" CssClass="button-primary" ClientIDMode="Static" />
        </div>
	</div>
</asp:Content>