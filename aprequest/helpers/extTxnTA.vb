Option Strict Off
Option Explicit On
Imports System

Namespace dsRequestDetailsTableAdapters
    Partial Public Class txnsTA
        Inherits Global.System.ComponentModel.Component

        Public Property SelectCommand() As SqlClient.SqlCommand()
            Get
                If (Me._commandCollection Is Nothing) Then
                    Me.InitCommandCollection()
                End If
                Return Me._commandCollection
            End Get

            Set(ByVal value As SqlClient.SqlCommand())
                Me._commandCollection = value
            End Set
        End Property

        'Public Function FillByWhere(ByVal dataTable As dsRequestDetails.txnsDataTable, _
        '                            ByVal WhereExp As String) _
        '                            As Integer
        '    Dim stSelect As String

        '    stSelect = Me._commandCollection(0).CommandText
        '    Try
        '        Me._commandCollection(0).CommandText += " WHERE " + WhereExp
        '        Return Me.Fill(dataTable)
        '    Catch ex As Exception

        '    Finally
        '        Me._commandCollection(0).CommandText = stSelect
        '    End Try
        'End Function

        Public Function GetAllActives_MOD(ByVal strExtSQL As String) As dsRequestDetails.txnsDataTable
            Dim cusCmd As New Global.System.Data.SqlClient.SqlCommand
            cusCmd.Connection = Me.Connection
            cusCmd.CommandText = Me.CommandCollection(6).CommandText & strExtSQL
            cusCmd.CommandType = Global.System.Data.CommandType.Text

            Me.Adapter.SelectCommand = cusCmd
            Dim dataTable As dsRequestDetails.txnsDataTable = New dsRequestDetails.txnsDataTable
            Me.Adapter.Fill(dataTable)
            Return dataTable
        End Function

        Public Function GetDone_MOD(ByVal strExtSQL As String) As dsRequestDetails.txnsDataTable
            Dim cusCmd As New Global.System.Data.SqlClient.SqlCommand
            cusCmd.Connection = Me.Connection
            cusCmd.CommandText = strExtSQL
            cusCmd.CommandType = Global.System.Data.CommandType.Text

            Me.Adapter.SelectCommand = cusCmd
            Dim dataTable As dsRequestDetails.txnsDataTable = New dsRequestDetails.txnsDataTable
            Me.Adapter.Fill(dataTable)
            Return dataTable
        End Function
    End Class
End Namespace
