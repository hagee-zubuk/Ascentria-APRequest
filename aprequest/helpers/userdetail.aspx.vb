Public Partial Class userdetail
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim strGUID As String = Request("guid") & ""
        Dim strActn As String = Request("play") & ""
        If strGUID = "" And strActn = "" Then
            Response.Write("{ ""error"":1," & _
                """message"":""user id unspecified"" }")
            Exit Sub
        End If

        strActn = strActn.Trim(",")
        Dim oUtil As New ZUtils
        Dim oHash As New zHashes

        If strActn = "" Then
            Dim oUsrs As New dsUsersTableAdapters.userconfigTA
            Dim oDataTab As dsUsers.userconfigDataTable = oUsrs.GetByGUID(strGUID)
            If oDataTab.Count <= 0 Then
                Response.Write("{ ""error"":2," & _
                                """message"":""user record not found"" }")
                Exit Sub
            End If

            Dim oUserRow As dsUsers.userconfigRow = oDataTab(0)
            Dim strOut As String = """username"":""" & oUserRow.username & ""","
            strOut &= """fullname"":""" & oUserRow.fullname & ""","
            strOut &= """email"":"""
            If Not oUserRow.IsemailNull Then strOut &= oUserRow.email
            strOut &= """,""manager"":"""
            If Not oUserRow.Ismgr_idNull Then strOut &= oUserRow.mgr_id
            strOut &= """,""ent"":"""
            If Not oUserRow.Isentity_idNull Then strOut &= oUserRow.entity_id
            strOut &= """,""reg"":"""
            If Not oUserRow.Isregion_idNull Then strOut &= oUserRow.region_id
            strOut &= """,""sit"":"""
            If Not oUserRow.Issite_idNull Then strOut &= oUserRow.site_id
            strOut &= """,""limit"":"""
            If Not oUserRow.IslimitNull Then strOut &= FormatNumber(oUserRow.limit, 2, TriState.True, TriState.False, TriState.True)
            strOut &= """,""fyr"":""" & oUserRow.fiscalyr & ""","
            strOut &= """adm"":""" & oUserRow.isadmin & ""","
            strOut &= """fin"":""" & oUserRow.isfinance & ""","
            strOut &= """mux"":""" & oUserRow.ismultisite & ""","
            'strOut &= """app"":""" & oUserRow.isapprover & ""","
            strOut &= """man"":""" & oUserRow.ismanager & ""","
            strOut &= """req"":""" & oUserRow.isrequester & ""","
            strOut &= """guid"":""" & oUserRow.guid & ""","
            strOut &= """message"": ""found: " & oUserRow.fullname & ""","
            strOut &= """error"": 0,"
            Dim strTmp As String = ""
            Dim lngX As Integer = 0
            Dim lsMgrs As List(Of String) = oUtil.FetchMgrs(oUserRow.username)
            For Each strZ As String In lsMgrs
                strTmp &= "{""name"":""" & strZ & """}"
                lngX += 1
                If lngX < lsMgrs.Count Then strTmp &= ","
            Next
            strOut &= """managers"":[" & strTmp & "]"
            '""" & String.Join(""",""name"":""", lsMgrs.ToArray)
            'strOut &= """managers"":[{""name"":""phalstead""},{""name"":""mmmmbop""}]"

            Response.Write("{ " & strOut & " }")
        ElseIf strActn = "SAVE" Then
            strGUID = Request("uuid")
            strGUID = strGUID.Trim(",")
            Try
                Dim strUsername$, strFullname$, strEmail$, strManager$, strEntityID$, _
                        strRegionID$, strSiteID$, strFiscalYr$, strLimit$, strReq$, _
                        strPro$, strAdm$, strFin$, strMuS$, strMgrList$

                strUsername = Request("username") & ""
                strUsername = strUsername.ToLower()
                strFullname = Request("fullname") & ""
                strEmail = Request("email") & ""
                strManager = Request("manager") & ""
                strEntityID = Request("entity_id") & ""
                strRegionID = Request("region_id") & ""
                strSiteID = Request("site_id") & ""
                strFiscalYr = Request("fiscalyr") & ""
                strLimit = Request("limit") & ""
                strReq = Request("req") & ""
                strMgrList = Request("lbAppr") & ""
                If InStr(strMgrList, strManager) <= 0 Then strMgrList &= "|" & strManager
                Dim aryMgrs = Split(strMgrList, "|")
                'strApp = Request("app") & ""
                strFin = Request("fin") & ""
                strPro = Request("pro") & ""
                strAdm = Request("adm") & ""
                strMuS = Request("mus") & ""
                Dim blnSuccess As Boolean = False
                Dim strMesg As String = "user record updated"
                Using oMgrs As New dsUsersTableAdapters.managersTA
                    oMgrs.FlushManagers(strUsername)
                    For Each strMgr As String In aryMgrs
                        If strMgr <> "" And strMgr <> strUsername Then
                            oMgrs.MgrAssoc(strUsername, strMgr)
                        End If
                    Next
                End Using
                Using oUsrs As New dsUsersTableAdapters.userconfigTA
                    strUsername = FixStr(strUsername.Trim(","))
                    strEmail = FixStr(strEmail.Trim(","))
                    If strGUID = "" Then
                        strGUID = oUsrs.CheckExists(strUsername, strEmail) & ""
                        ' new record
                        If strGUID = "" Then
                            strGUID = oHash.MkGUID
                            oUsrs.Insert(strGUID, strUsername, _
                                        FixStr(strFullname.Trim(",")), strEmail, _
                                        FixStr(strManager.Trim(",")), FixStr(strEntityID.Trim(",")), _
                                        FixStr(strRegionID.Trim(",")), FixStr(strSiteID.Trim(",")), _
                                        FixStr(strFiscalYr.Trim(",")), FixDec(strLimit.Trim(",")), _
                                        FixBln(strReq.Trim(",")), False, FixBln(strPro.Trim(",")), _
                                        FixBln(strAdm.Trim(",")), FixBln(strFin.Trim(",")), _
                                        FixBln(strMuS.trim(",")))
                            blnSuccess = True
                        Else
                            strMesg = "That username or e-mail address already exists. Update failed."
                        End If
                    Else
                        ' update the existing record
                        oUsrs.UpdateByGUID(FixStr(strEmail.Trim(",")), FixStr(strManager.Trim(",")), FixStr(strEntityID.Trim(",")), _
                                           FixStr(strRegionID.Trim(",")), FixStr(strSiteID.Trim(",")), FixStr(strFiscalYr.Trim(",")), _
                                           FixDec(strLimit.Trim(",")), FixBln(strReq.Trim(",")), False, _
                                           FixBln(strPro.Trim(",")), FixBln(strAdm.Trim(",")), FixBln(strFin.Trim(",")), FixBln(strMuS.Trim(",")), strGUID)
                        blnSuccess = True
                    End If
                End Using
                If blnSuccess Then
                    Response.Write("{ ""error"":0," & _
                            """message"":""" & strMesg & """ }")
                Else
                    Response.Write("{ ""error"":4," & _
                            """message"":""" & strMesg & """ }")
                End If
            Catch ex As Exception
                Response.Write("{ ""error"":3," & _
                       """message"":" & ex.Message)
            End Try
        End If
    End Sub

    Function FixStr(ByVal zz) As String
        Dim strTmp$ = ""
        If Not (zz Is Nothing) Then
            strTmp = Trim(zz)
        End If
        Return strTmp
    End Function

    Function FixDec(ByVal zz) As Decimal
        Dim decTmp As Decimal = 0
        If Not (zz Is Nothing) Then
            If IsNumeric(zz) Then decTmp = Convert.ToDecimal(zz)
        End If
        Return decTmp
    End Function

    Function FixBln(ByVal zz) As Boolean
        Dim blnTmp As Boolean = False
        If Not (zz Is Nothing) Then
            Try
                blnTmp = CBool(zz)
            Catch ex As Exception
                ' do nothing
            End Try
        End If
        Return blnTmp
    End Function
End Class