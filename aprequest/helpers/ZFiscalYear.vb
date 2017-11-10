Imports System.Data
Imports System.Data.SqlClient

Public Class ZFiscalYear
    Public FY_Title As String
    Public FY_ID As String

    Public Function FetchFiscalYears() As List(Of ZFiscalYear)
        Dim tmpFY As New List(Of ZFiscalYear)
        Dim cn As New SqlClient.SqlConnection
        Dim ds As New DataSet
        Dim dt As New DataTable

        Dim strCN As String = ConfigurationManager.ConnectionStrings("SAGE_DBConnectionString").ConnectionString
        cn.ConnectionString = strCN
        Dim cmd As New SqlClient.SqlCommand
        cmd.Connection = cn
        cmd.CommandType = CommandType.Text
        Dim strSQL As String = "SELECT [sCodeID], [sTitle] FROM [tblAcctCode_7] WHERE [sStatus]='A' ORDER BY [sTitle] ASC"
        cmd.CommandText = strSQL
        Try
            cn.Open()
            cmd.ExecuteNonQuery()
            Dim da As New SqlDataAdapter(cmd)
            da.Fill(ds)
        Catch ex As Exception
        Finally
            cn.Close()
        End Try
        dt = ds.Tables(0)
        For Each row As DataRow In dt.Rows
            Dim fyi As New ZFiscalYear
            fyi.FY_ID = row("scodeid").ToString().Trim()
            fyi.FY_Title = row("stitle").ToString().Trim()
            tmpFY.Add(fyi)
        Next
        Return tmpFY
    End Function
End Class
