<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="useradmin.aspx.vb" Inherits="aprequest.useradmin" %>
<%@ Register src="helpers/Toplinks.ascx" tagname="Toplinks" tagprefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>User Administration</title>
    <link rel="Stylesheet" type="text/css" href="aprequest.css" />
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/prototype.js")%>" ></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/scriptaculous.js")%>"></script>
    <script language="javascript" type="text/javascript" src="<%= ResolveclientUrl("~/js/modalbox.js")%>"></script>
    <link rel="Stylesheet" type="text/css" href="<%= ResolveclientUrl("~/js/modalbox.css")%>" />
    <script language="javascript" type="text/javascript" >
var ixEnt = 0;
var ixReg = 0;
var ixSit = 0;
var ixFyr = 0;
var mgr;
function setSelector(sbox, sval) {
    var strSel = new String('select#' + sbox + ' option');
    var options = $$(strSel);
    var len = options.length;
    for (var i = 0; i < len; i++) {
        if (options[i].value == sval) {
            options[i].selected = true;
            break;
        }
    }
    if (i >= len) i = -1;
    return i;
}

function setCheck(lbox, sval) {
    var strSel = new String('select#' + lbox + ' option');
    var options = $$(strSel);
    var len = options.length;
    console.log('looking for: ' + sval);
    for (var i = 0; i < len; i++) {
        if (options[i].value == sval) {
            options[i].selected = 'selected';
            options[i].checked = true;
            console.log('found: ' + i + ' (' + options[i].text + '): ' + sval);
            break;
        }
    }
    if (i >= len) i = -1;
    return i;
}

function clearSel(lbox) {
    var strSel = new String('select#' + lbox + ' option');
    var options = $$(strSel);
    var len = options.length;
    for (var i = 0; i < len; i++) {
        options[i].selected = false;
    }
    return (i);
}

function my_ser(lbox) {
    var strSel = new String('select#' + lbox + ' option');
    var options = $$(strSel);
    var res = '';
    var cnt = 0;
    var len = options.length;
    for (var i = 0; i < len; i++) {
        if (options[i].selected) {
            if (cnt > 0) res += '|';
            res += options[i].value;
            cnt++;
        }
    }
    return (res);
}


function delUsr(guid) {
    var frmDat = 'guid=' + escape(guid);
    var uurl = '<%=Resolveclienturl("~/helpers/userdelete.aspx") %>?' + frmDat;
    var ajx = new Ajax.Request(uurl, {
        postBody: frmDat,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if (retData.error > 0) {
                alert(retData.message);
            } else {
                alert(retData.message);
                location.reload();
            }
        }
    });
}

function attempt_save() {
    //event.stop();
    var frmDat = 'uuid=' + $('uuid').value + '&limit=' + $('limit').value +
            '&username=' + $('username').value + '&fullname=' + $('fullname').value +
            '&email=' + $('email').value + '&manager=' + $('manager').value;
    frmDat = frmDat + '&entity_id=' + $('<%=ddEnt.ClientID %>').value;
    frmDat = frmDat + '&region_id=' + $('<%=ddReg.ClientID %>').value;
    frmDat = frmDat + '&site_id=' + $('<%=ddSite.ClientID %>').value;
    frmDat = frmDat + '&fiscalyr=' + $('<%=ddFY.ClientID %>').value;
    frmDat = frmDat + '&lbAppr=' + my_ser('<%=lbAppr.ClientID %>');
    if ($('chkReq').checked) frmDat += '&req=1';
    //if ($('chkApp').checked) frmDat += '&app=1';
    if ($('chkPro').checked) { frmDat += '&pro=1' } else { frmDat += '&pro=0'};
    if ($('chkFin').checked) { frmDat += '&fin=1' } else { frmDat += '&fin=0'};
    if ($('chkAdm').checked) { frmDat += '&adm=1' } else { frmDat += '&adm=0'};
    if ($('chkMus').checked) { frmDat += '&mus=1' } else { frmDat += '&mus=0'};
    frmDat += '&play=SAVE';
    var uurl = '<%= ResolveclientUrl("~/helpers/userdetail.aspx")%>?' + escape(frmDat);
    //alert("URL: " + sURL);
    var ajx = new Ajax.Request(uurl, {
        postBody: frmDat,
        onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
        onSuccess: function(rct) {
            var retData = rct.responseText.evalJSON();
            if ((retData.error > 0) && (retData.error < 16)) {
                alert(retData.message);
            } else {
                alert(retData.message);
                Modalbox.hide();
                
                location.reload();
            }
        }
    });
}

function setMBObservers() {
    clearSel('<%=lbAppr.ClientID %>');
    if (mgr != null) {
        for (var i = 0; i < mgr.length; i++) {
            var obj = mgr[i];
            console.log('Trying checkbox');
            setCheck('<%=lbAppr.ClientID %>', obj.name);
        }
    }
    $('btnSave').observe('click', function() { attempt_save(); });
    $('<%=ddEnt.ClientID %>').selectedIndex = ixEnt;
    $('<%=ddReg.ClientID %>').selectedIndex = ixReg;
    $('<%=ddSite.ClientID %>').selectedIndex = ixSit;
    $('<%=ddFY.ClientID %>').selectedIndex = ixFyr;
}

function rmMBObservers() {
    $('btnSave').stopObserving('click', function() { attempt_save(); });
}

function adduser_clicked(event) {
    event.stop();
    mgr = null;
    $('uuid').value = "";
    $('username').value = "";
    $('fullname').value = "";
    $('email').value = "";
    $('manager').value = "";
    $('limit').value = "";
    $('chkReq').checked = false;
    //$('chkApp').checked = false;
    $('chkPro').checked = false;
    $('chkFin').checked = false;
    $('chkAdm').checked = false;
    $('btnSave').value = "Save";
    ixEnt = setSelector('<%=ddEnt.ClientID %>', '');
    ixReg = setSelector('<%=ddReg.ClientID %>', '');
    ixSit = setSelector('<%=ddSite.ClientID %>', '9999');
    ixFyr = setSelector('<%=ddFY.ClientID %>', 'NA');
    Modalbox.show($('modal'), { title: 'Add User', width: 600, aspnet: true, afterLoad: setMBObservers, onHide: rmMBObservers });
}

Event.observe(window, 'load', function() {
    //hide();
    $('lnkAddUser').observe('click', adduser_clicked);
    $('<%=ddEnt.ClientID %>').insert(new Element('option', { value: '' }).update(''));
    $('<%=ddReg.ClientID %>').insert(new Element('option', { value: '' }).update(''));
    $('<%=ddSite.ClientID %>').insert(new Element('option', { value: '' }).update(''));
    $('<%=ddFY.ClientID %>').insert(new Element('option', { value: '' }).update(''));
    $$('.modal_link').each(function(element) {
        element.observe('click', function(event) {
            var aTag = Event.element(event);
            var sURL = new String(aTag.readAttribute('href'));
            event.stop();
            var uurl = '<%= ResolveclientUrl("~/helpers/userdetail.aspx")%>';
            // alert("URL: " + sURL);
            var ajx = new Ajax.Request(uurl, {
                postBody: 'guid=' + sURL,
                onFailure: function(rct) { alert("ERROR!\n\n" + rct.responseText); },
                onSuccess: function(rct) {
                    var retData = rct.responseText.evalJSON();
                    if ((retData.error > 0) && (retData.error < 16)) {
                        alert(retData.message);
                    } else {
                        $('uuid').value = retData.guid;
                        $('username').value = retData.username;
                        $('fullname').value = retData.fullname;
                        $('email').value = retData.email;
                        $('manager').value = retData.manager;
                        $('limit').value = retData.limit;
                        $('btnSave').value = "Update";
                        mgr = retData.managers;
                        Modalbox.show($('modal'), { title: retData.fullname, width: 600, afterLoad: setMBObservers, onHide: rmMBObservers });
                        ixEnt = setSelector('<%=ddEnt.ClientID %>', retData.ent);
                        ixReg = setSelector('<%=ddReg.ClientID %>', retData.reg);
                        ixSit = setSelector('<%=ddSite.ClientID %>', retData.sit);
                        ixFyr = setSelector('<%=ddFY.ClientID %>', retData.fyr);

                        $('chkReq').checked = (retData.req == 'True');
                        //$('chkApp').checked = (retData.app == 'True');
                        $('chkFin').checked = (retData.fin == 'True');
                        $('chkPro').checked = (retData.man == 'True');
                        $('chkAdm').checked = (retData.adm == 'True');
                        $('chkMus').checked = (retData.mux == 'True');
                    }
                }
            });

        });
    });
});
   	</script>
</head>
<body>
<form id="frmUA" name="frmUA" runat="server">
<div id="Div1" class="ascbox">
<img id="Img2" src="~/images/ascentria-logo.gif" runat="server" alt="Ascentria Care Alliance" title="" style="height: 80px; width:286px; float: left;"/>
<div style="width: 286px; height: 16px; float: right; margin: 20px 20px 0px 0px; font: 10pt bold Calibri, Tahoma, Verdana, Arial, Helvetica; color: #888; text-align: right;">
<asp:Literal ID="datedisp" runat="server"></asp:Literal></div>
<uc1:Toplinks ID="Toplinks1" runat="server" />
</div>
    
    <div id="top" class="content">
        <div style="width: 25%; float: left;">
            Welcome, <asp:LinkButton ID="ulink" runat="server" Text="Unknown User"></asp:LinkButton>
        </div>
        <div style="width: 60%; float: left; text-align: center; clear: both; margin: 0px auto;">
            <h1 style="font-size: 1.5em !important;">User Admin Page</h1>
        </div>        
        <div style="float:none; clear: both;">&nbsp;</div>
			<div >
				<a href="#" id="trgModalWin" name="trgModalWin" style="display: none;">Blah</a>
                <asp:Table ID="tblUsrs" runat="server" Width="100%">
                    <asp:TableHeaderRow ID="tblUsrsHead" BorderColor="Gray" BorderWidth="1" runat="server" CssClass="grid">
                        <asp:TableHeaderCell ID="tuhName" runat="server" Text="Name" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhUsername" runat="server" Text="Username" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhManager" runat="server" Text="Manager" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhEntity" runat="server" Text="Entity" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhRegion" runat="server" Text="Region" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhSite" runat="server" Text="Site" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhLimit" runat="server" Text="Limit" HorizontalAlign="Center" />
                        <asp:TableHeaderCell ID="tuhAccount" runat="server" Text="Account" HorizontalAlign="Center" />
                    </asp:TableHeaderRow>
                </asp:Table>
                <br />
                <a class="button-primary" id="lnkAddUser" href="">Add User</a>
                <br />
                <div style="width: 100%; float: none; clear: both;">
                <div style="width: 50%; float: right; text-align: right;">
                    <a href="#" onclick="window.open('<%=resolveurl("~/logviewer.aspx") %>','actvy','channelmode=0,directories=0,fullscreen=0,height=600,left=20,location=0,menubar=0,resizable=1,scrollbars=1,status=0,titlebar=1,toolbar=0,top=40,width=800');">
                    Browse Activity Log
                    </a>
                    </div>
                </div>
				<div id="modal" style="display: none;">
				    <table style="width: 550px; font-size: 10pt;">
					<tr><td align="right">Username:</td><td><input id="username" name="username" /></td></tr>
					<tr><td align="right">Full name:</td><td><input style="width: 150px;" id="fullname" name="fullname" /></td></tr>
					<tr><td align="right">e-Mail:</td><td><input id="email" name="email" style="width: 250px;" /></td></tr>
					<tr><td align="right">Manager:</td><td><input id="manager" name="manager" style="width: 250px;" /></td></tr>
                    <tr><td align="right">Add'l Approvers:</td><td><asp:ListBox ID="lbAppr" Font-Size="Smaller" SelectionMode="Multiple" runat="server"></asp:ListBox></td></tr>
					<tr><td align="right">Entity:</td><td><asp:DropDownList ID="ddEnt" runat="server"></asp:DropDownList></td></tr>
					<tr><td align="right">Region:</td><td><asp:DropDownList ID="ddReg" runat="server" Width="250px" ></asp:DropDownList></td></tr>
					<tr><td align="right">Site:</td><td><asp:DropDownList ID="ddSite" runat="server"></asp:DropDownList></td></tr>
					<tr><td align="right">Limit:</td><td><input id="limit" name="limit" maxlength="12" style="text-align: center; width: 120px;"/></td></tr>
					<tr><td align="right">Default Fiscal Year:</td><td><asp:DropDownList ID="ddFY" runat="server"></asp:DropDownList></td></tr>
                    </table>
					<table style="width: 70%; font-size: 9pt;">
						<tr><th colspan="4" align="center">Permissions</th></tr>
						<tr><th align="center">Request</th>
							<th align="center">Export</th>
                            <th align="center">Finance</th>
							<th align="center">Administer</th>
                            <th align="center">Multi-site</th>
							</tr>
						<tr><td align="center"><input type="checkbox" id="chkReq" name="chkReq" value="1" /></td>
							<td align="center"><input type="checkbox" id="chkPro" name="chkPro" value="1" /></td>
                            <td align="center"><input type="checkbox" id="chkFin" name="chkFin" value="1" /></td>
							<td align="center"><input type="checkbox" id="chkAdm" name="chkAdm" value="1" /></td>
                            <td align="center"><input type="checkbox" id="chkMus" name="chkMus" value="1" /></td>
							</tr>
					</table>
					<div style="width: 100%; text-align: center; margin-top: 10px;">
					<input type="hidden" id="uuid" name="uuid" />
					<input id="btnSave" name="btnSave" type="button" class="button-primary" style="width: 80px;" value="Save" />
					</div>
				</div>
			</div>
    </div>
    </form>
</body>
</html>
