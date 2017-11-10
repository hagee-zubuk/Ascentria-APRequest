Imports System.IO
Public Class ZXLSWriter
    Private oStream As Stream
    Private oWriter As BinaryWriter

    Private clBegin() As UShort = {&H809, 8, 0, &H10, 0, 0}
    Private clEnd() As UShort = {&HA, 0}


    Public Sub New(ByRef oSt As Stream)
        Me.oStream = oSt
        oWriter = New BinaryWriter(oSt)
    End Sub

    Private Sub WriteUShortArray(ByVal value() As UShort)
        Dim i As Integer
        For i = 0 To value.Length - 1
            oWriter.Write(value(i))
        Next
    End Sub

    Public Sub WriteCell(ByVal row As Integer, ByVal col As Integer, ByVal value As String)
        Dim clData() As UShort = {&H204, 0, 0, 0, 0, 0}
        Dim iLen As Integer = value.Length
        Dim plainText() As Byte = Encoding.ASCII.GetBytes(value)
        clData(1) = 8 + iLen
        clData(2) = row
        clData(3) = col
        clData(5) = iLen
        WriteUShortArray(clData)
        oWriter.Write(plainText)
    End Sub

    Public Sub WriteCell(ByVal row As Integer, ByVal col As Integer, ByVal value As Integer)
        Dim clData() As UShort = {&H27E, 10, 0, 0, 0}
        cldata(2) = row
        cldata(3) = col
        WriteUShortArray(cldata)
        Dim iValue As Integer = (value << 2) Or 2
        oWriter.Write(iValue)

    End Sub

    Public Sub WriteCell(ByVal row As Integer, ByVal col As Integer, ByVal value As Double)
        Dim clData() As UShort = {&H203, 14, 0, 0, 0}
        clData(2) = row
        clData(3) = col
        WriteUShortArray(clData)
        oWriter.Write(value)
    End Sub

    Public Sub WriteCell(ByVal row As Integer, ByVal col As Integer)
        Dim clData() As UShort = {&H201, 6, 0, 0, &H17}
        clData(2) = row
        clData(3) = col
        WriteUShortArray(clData)
    End Sub

    Public Sub BeginWrite()
        WriteUShortArray(clBegin)
    End Sub

    Public Sub EndWrite()
        WriteUShortArray(clEnd)
        oWriter.Flush()
    End Sub
End Class
