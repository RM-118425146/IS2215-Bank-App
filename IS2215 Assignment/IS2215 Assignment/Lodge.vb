Public Class Lodge
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
    Function Lodge() As Double
        Bal = Bal + num_1
        Return Bal
    End Function
End Class
