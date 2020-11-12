Public Class Withdraw
    Private Bal, num_1 As Double
    Public Property Balance As Double
        Get
            Return Bal
        End Get
        Set(ByVal value As Double)
            Bal = value
        End Set
    End Property
    Public Property amount As Double
        Get
            Return num_1
        End Get
        Set(ByVal value As Double)
            num_1 = value
        End Set
    End Property
    Function Withdraw() As Double
        If Bal > num_1 Then
            Bal = Bal - num_1
            Return Bal
        Else
            Return MsgBox("Error! Insufficent Funds in account")
        End If
    End Function
End Class
