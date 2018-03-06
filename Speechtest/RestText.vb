

Public Class RestText
    Public Property RecognitionStatus As String
    Public Property Offset As Integer
    Public Property Duration As Integer
    Public Property Nbest As List(Of Nbest1)
End Class

Public Class Nbest1
    Public Property Confidence As Single
    Public Property Lexical As String
    Public Property ITN As String
    Public Property MaskedITN As String
    Public Property Display As String
End Class
