Public Class frmMain
#Region "Global variables & objects"
    'Variables needed throughout the application
    Dim LoggedIn As Boolean = False
    Dim EnteredPin As String
    Dim WithdrawalAmount, LodgementAmount As Double
    Dim TransactionID As Integer

    'Variables to hold names & pins
    Dim strNames(99) As String
    Dim intPin(99) As Integer

    'variable to count current row and current user info
    Dim rowIndex As Integer
    Dim currentUser As Integer
    Dim currentBal As Double

    'objects to lodge and withdraw money
    Dim Lodge As New Lodge
    Dim Withdraw As New Withdraw
    Dim Transfer As New Transfer
#End Region
#Region "Database Objects"
    'Loading database into application
    Dim objConnection As New OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= BIS_bank.accdb")
    Dim objAccountDA As New OleDb.OleDbDataAdapter("Select * from Bank", objConnection)
    Dim objCustomerDA As New OleDb.OleDbDataAdapter("Select * from Customer", objConnection)
    Dim objTransactionDA As New OleDb.OleDbDataAdapter("Select * from Transactions", objConnection)
    Dim objAccountCB As New OleDb.OleDbCommandBuilder(objAccountDA)
    Dim objCustomerCB As New OleDb.OleDbCommandBuilder(objCustomerDA)
    Dim objTansactionsCB As New OleDb.OleDbCommandBuilder(objTransactionDA)
    Dim objDataSet As New DataSet()
#End Region
#Region "Form Load"
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        'sub called to fill the application the database data
        FillData()

        'Call sub to hide the tabs controls
        HideTabs(TabControl1)
        HideTabs(TabControl2)
        HideTabs(TabControl3)

        'Makes buttons that do not need to be seen immediatly invisible
        btnStatement.Visible = False
        pnlStatement.Visible = False
        btnTransactions.Visible = False
        pnlTransactions.Visible = False
        btnBalance.Visible = False
        pnlBalance.Visible = False
        btnDetails.Visible = False
        pnlDetails.Visible = False
        btnLogOut.Visible = False
        txtPin.Text = ""
        btnBackspace.Text = ChrW(9003)
        txtUsername.Focus()
    End Sub
#End Region
#Region "Button clicks"
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        'closes the application
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to exit the application", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
        If result = DialogResult.OK Then
            Me.Close()
        ElseIf result = DialogResult.Cancel Then
            Exit Sub
        End If
    End Sub
    Private Sub btnLogIn_Click(sender As Object, e As EventArgs) Handles btnLogIn.Click

        'objects used to populate the application
        Dim objCustomer As DataRow, objAccount As DataRow
        Dim rowIndex As Integer = objDataSet.Tables("Customer").Rows.Count

        'If statement to check if login details are correct
        'if they are correct then the user is taken to the next page and buttons are made visible
        For i As Integer = 0 To rowIndex - 1
            If txtUsername.Text.ToUpper = strNames(i).ToUpper Then
                If EnteredPin = intPin(i) Then
                    'set up textboxes, buttons, and labels as required for logged in user
                    TabControl1.SelectTab(1)
                    LoggedIn = True
                    currentUser = i + 1
                    btnStatement.Visible = True
                    btnTransactions.Visible = True
                    btnBalance.Visible = True
                    pnlBalance.Visible = True
                    btnDetails.Visible = True
                    btnLogOut.Visible = True
                    EnteredPin = ""
                    txtPin.Text = ""
                    txtUsername.Text = ""
                    pnlBalance.Focus()

                    'finding the correct data to display for the current user
                    txtWelcome.Text = "Welcome " & objDataSet.Tables("Customer").Rows(i).Item("Cust_Name")
                    objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
                    For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
                        txtBalance.Text = "Balance: €" & objAccount.Item("Account_Balance")
                        currentBal = objAccount.Item("Account_Balance")
                    Next

                    Exit Sub
                Else
                    'error messaage
                    MsgBox("Details Incorrect, Please check your credentials")
                    txtUsername.Clear()
                    txtPin.Text = ""
                    Exit For
                End If

            ElseIf i = rowIndex - 1 And txtUsername.Text.ToUpper <> strNames(i).ToUpper Then
                'error message
                MsgBox("Details Incorrect, Please check your credentials")
                txtUsername.Clear()
                txtPin.Text = ""
                Exit For
            End If
        Next
    End Sub
    Private Sub btnLogOut_Click(sender As Object, e As EventArgs) Handles btnLogOut.Click
        'When the user logs out they are returned to the home screen
        TabControl1.SelectTab(0)
        TabControl2.SelectTab(0)
        TabControl3.SelectTab(0)
        txtAmountLodge.Clear()
        txtAmountWithdraw.Clear()
        LoggedIn = False

        'Makes buttons that do not need to be seen immediatly invisible
        btnStatement.Visible = False
        pnlStatement.Visible = False
        btnTransactions.Visible = False
        pnlTransactions.Visible = False
        btnBalance.Visible = False
        pnlBalance.Visible = False
        btnDetails.Visible = False
        pnlDetails.Visible = False
        btnLogOut.Visible = False
    End Sub
    Private Sub btnTransactions_Click(sender As Object, e As EventArgs) Handles btnTransactions.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(7)
        pnlBalance.Visible = False
        pnlStatement.Visible = False
        pnlDetails.Visible = False
        pnlTransactions.Visible = True
    End Sub
    Private Sub btnStatement_Click(sender As Object, e As EventArgs) Handles btnStatement.Click
        'Controls made visible and invisble as needed
        pnlBalance.Visible = False
        pnlDetails.Visible = False
        pnlTransactions.Visible = False
        pnlStatement.Visible = True
        TabControl1.SelectTab(12)

        'Adding columns to datagridview
        DataGridView2.ColumnCount = 6
        DataGridView2.Columns(0).Name = "Date"
        DataGridView2.Columns(1).Name = "Description"
        DataGridView2.Columns(2).Name = "ID"
        DataGridView2.Columns(3).Name = "Withdrawal"
        DataGridView2.Columns(4).Name = "Deposit"
        DataGridView2.Columns(5).Name = "Balance"

        'populating datagridview with the current users info
        DataGridView2.Rows.Clear()
        Dim objCustomer As DataRow, objTransactions, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        Dim TransID, TransName, TransDate, Amount, TransType As String
        Dim TotalWithdrawal, TotalDeposit, TotalBalance As Double
        Dim newRow(5) As String
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            currentBal = objAccount.Item("Account_Balance")
            lblStatementName.Text = "Customer Name:" & objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Name")
            lblAddress.Text = "Customer Address:" & objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Address")
            lblCustomerID.Text = "Customer ID:" & objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Customer_ID")
            lblInsertAccountNumber.Text = "Account Number:" & objAccount.Item("Account_Number")
        Next

        TotalBalance = currentBal

        For Each objTransactions In objCustomer.GetChildRows("Customer2Transactions")

            TransID = objTransactions.Item("Trans_ID")
            TransName = objTransactions.Item("Trans_Name")
            TransDate = objTransactions.Item("Trans_Date")
            Amount = objTransactions.Item("Trans_Amount")
            TransType = objTransactions.Item("Trans_Type")

            newRow(0) = TransDate
            newRow(1) = TransName
            newRow(2) = TransID
            If TransType = "Out" Then
                newRow(3) = Amount
                TotalWithdrawal = TotalWithdrawal + CDbl(Amount)
                newRow(4) = ""
                TotalBalance = TotalBalance + CDbl(Amount)
            Else
                newRow(3) = ""
                newRow(4) = Amount
                TotalDeposit = TotalDeposit + CDbl(Amount)
                TotalBalance = TotalBalance - CDbl(Amount)
            End If
            newRow(5) = TotalBalance
            DataGridView2.Rows.Add(newRow)

        Next

        'correcting data errors relating to balance
        For i As Integer = DataGridView2.Rows.Count - 1 To 0 Step -1
            Dim Balance, Deposit, Withdrawal As Double
            If i = DataGridView2.Rows.Count - 1 Then
                DataGridView2.Rows.Item(i).Cells.Item(5).Value = currentBal
            Else
                If DataGridView2.Rows.Item(i + 1).Cells.Item(3).Value = "" Then
                    Balance = CDbl(DataGridView2.Rows.Item(i + 1).Cells.Item(5).Value)
                    Deposit = CDbl(DataGridView2.Rows.Item(i + 1).Cells.Item(4).Value)
                    DataGridView2.Rows.Item(i).Cells.Item(5).Value = Balance - Deposit
                Else
                    Balance = CDbl(DataGridView2.Rows.Item(i + 1).Cells.Item(5).Value)
                    Withdrawal = CDbl(DataGridView2.Rows.Item(i + 1).Cells.Item(3).Value)
                    DataGridView2.Rows.Item(i).Cells.Item(5).Value = Balance + Withdrawal
                End If
            End If
        Next

        'adding important info to display at top & bottom of statement
        newRow(0) = ""
        newRow(1) = "**Totals**"
        newRow(2) = ""
        newRow(3) = TotalWithdrawal
        newRow(4) = TotalDeposit
        newRow(5) = ""
        DataGridView2.Rows.Add(newRow)

        newRow(0) = DataGridView2.Rows.Item(0).Cells.Item(0).Value
        newRow(1) = "Previous Balance"
        newRow(2) = ""
        newRow(3) = ""
        newRow(4) = ""
        If DataGridView2.Rows.Item(0).Cells.Item(3).Value = "-" Then
            newRow(5) = CDbl(DataGridView2.Rows.Item(0).Cells.Item(5).Value) - CDbl(DataGridView2.Rows.Item(0).Cells.Item(4).Value)
        Else
            newRow(5) = CDbl(DataGridView2.Rows.Item(0).Cells.Item(5).Value) + CDbl(DataGridView2.Rows.Item(0).Cells.Item(3).Value)
        End If
        DataGridView2.Rows.Insert(0, newRow)

        lblDates.Text = DataGridView2.Rows.Item(1).Cells.Item(0).Value & " to " & Date.Today

        'editing the datagridview visuals
        DataGridView2.AutoResizeColumns()
        DataGridView2.AutoResizeRows()
        DataGridView2.AutoResizeColumnHeadersHeight()
        DataGridView2.RowHeadersVisible = False

    End Sub
    Private Sub btnBalance_Click(sender As Object, e As EventArgs) Handles btnBalance.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(1)
        TabControl2.SelectTab(0)
        TabControl3.SelectTab(0)
        txtAmountLodge.Clear()
        txtAmountWithdraw.Clear()
        pnlBalance.Visible = True
        pnlStatement.Visible = False
        pnlDetails.Visible = False
        pnlTransactions.Visible = False
        pnlBalance.Focus()
    End Sub
    Private Sub btnDetails_Click(sender As Object, e As EventArgs) Handles btnDetails.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(4)
        TabControl2.SelectTab(0)
        TabControl3.SelectTab(0)
        txtAmountLodge.Clear()
        txtAmountWithdraw.Clear()
        pnlBalance.Visible = False
        pnlStatement.Visible = False
        pnlDetails.Visible = True
        pnlTransactions.Visible = False

        'populating textboxes with the current users info
        Dim objCustomer As DataRow, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            txtAccountNumber.Text = objAccount.Item("Account_Number")
            cboAccountType.Text = objAccount.Item("Account_Type")
            txtCustomerName.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Name")
            txtCustomerAddress.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Address")
            txtCustPhone.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Telephone")
        Next

    End Sub
    Private Sub btn9_Click(sender As Object, e As EventArgs) Handles btn9.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "9"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn8_Click(sender As Object, e As EventArgs) Handles btn8.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "8"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn7_Click(sender As Object, e As EventArgs) Handles btn7.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "7"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn6_Click(sender As Object, e As EventArgs) Handles btn6.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "6"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn5_Click(sender As Object, e As EventArgs) Handles btn5.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "5"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn4_Click(sender As Object, e As EventArgs) Handles btn4.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "4"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn3_Click(sender As Object, e As EventArgs) Handles btn3.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "3"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn2_Click(sender As Object, e As EventArgs) Handles btn2.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "2"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn1_Click(sender As Object, e As EventArgs) Handles btn1.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "1"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btn0_Click(sender As Object, e As EventArgs) Handles btn0.Click
        'Entering the pin values into a variable
        EnteredPin = EnteredPin & "0"
        txtPin.Text = txtPin.Text & "X"
    End Sub
    Private Sub btnLodge9_Click(sender As Object, e As EventArgs) Handles btnLodge9.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "9"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge8_Click(sender As Object, e As EventArgs) Handles btnLodge8.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "8"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge7_Click(sender As Object, e As EventArgs) Handles btnLodge7.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "7"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge6_Click(sender As Object, e As EventArgs) Handles btnLodge6.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "6"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge5_Click(sender As Object, e As EventArgs) Handles btnLodge5.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "5"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge4_Click(sender As Object, e As EventArgs) Handles btnLodge4.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "4"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge3_Click(sender As Object, e As EventArgs) Handles btnLodge3.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "3"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge2_Click(sender As Object, e As EventArgs) Handles btnLodge2.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "2"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge1_Click(sender As Object, e As EventArgs) Handles btnLodge1.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "1"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodge0_Click(sender As Object, e As EventArgs) Handles btnLodge0.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountLodge.Text = txtAmountLodge.Text & "0"
        LodgementAmount = CDbl(txtAmountLodge.Text)
    End Sub
    Private Sub btnLodgePoint_Click(sender As Object, e As EventArgs) Handles btnLodgePoint.Click
        'Entering the withdrawal amount into the textbox & variable
        If txtAmountLodge.Text = "" Then
            txtAmountLodge.Text = txtAmountLodge.Text & "0."
            LodgementAmount = CDbl(txtAmountLodge.Text)
        Else
            txtAmountLodge.Text = txtAmountLodge.Text & "."
            LodgementAmount = CDbl(txtAmountLodge.Text)
        End If
    End Sub
    Private Sub btnClearLodgement_Click(sender As Object, e As EventArgs) Handles btnClearLodgement.Click
        'Clearing the textbox & variable
        txtAmountLodge.Text = ""
        LodgementAmount = 0
    End Sub
    Private Sub btnEnterLodgement_Click(sender As Object, e As EventArgs) Handles btnEnterLodgement.Click
        'finding the related data for the current user
        Dim objCustomer As DataRow, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        Lodge.amount = LodgementAmount
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            Lodge.Balance = objAccount.Item("Account_Balance")
        Next
        'lodging the money into the account
        Lodge.Lodge()
        lblLodgeAmount.Text = "€" & LodgementAmount
        lblBalUpdateLodge.Text = "€" & Lodge.Balance
        TabControl3.SelectTab(1)
    End Sub
    Private Sub btnConfirmLodge_Click(sender As Object, e As EventArgs) Handles btnConfirmLodge.Click
        'new datarow to hold the updated data
        'updated the item we want
        'send the update to the database
        Dim objRowCurrent As DataRow
        objRowCurrent = objDataSet.Tables("Bank").Rows.Find(currentUser)
        objRowCurrent("Account_Balance") = Lodge.Balance
        objAccountDA.Update(objDataSet, "Bank")
        objDataSet.AcceptChanges()

        Dim objNewTransaction As DataRow
        TransactionID += 1
        objNewTransaction = objDataSet.Tables("Transactions").NewRow
        objNewTransaction.Item("Trans_ID") = TransactionID
        objNewTransaction.Item("Trans_Name") = "ATM Lodgement"
        objNewTransaction.Item("Trans_Date") = Date.Today
        objNewTransaction.Item("Trans_Amount") = Lodge.amount
        objNewTransaction.Item("Cust_ID") = currentUser
        objNewTransaction.Item("Trans_Type") = "In"
        objDataSet.Tables("Transactions").Rows.Add(objNewTransaction)
        objTransactionDA.Update(objDataSet, "Transactions")
        objDataSet.AcceptChanges()

        'open the next page
        lblConfirmLodgementBal2.Text = "€" & Lodge.Balance
        TabControl3.SelectTab(2)
    End Sub
    Private Sub btnWithdraw9_Click(sender As Object, e As EventArgs) Handles btnWithdraw9.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "9"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw8_Click(sender As Object, e As EventArgs) Handles btnWithdraw8.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "8"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw7_Click(sender As Object, e As EventArgs) Handles btnWithdraw7.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "7"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw6_Click(sender As Object, e As EventArgs) Handles btnWithdraw6.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "6"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw5_Click(sender As Object, e As EventArgs) Handles btnWithdraw5.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "5"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw4_Click(sender As Object, e As EventArgs) Handles btnWithdraw4.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "4"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw3_Click(sender As Object, e As EventArgs) Handles btnWithdraw3.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "3"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw2_Click(sender As Object, e As EventArgs) Handles btnWithdraw2.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "2"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdraw1_Click(sender As Object, e As EventArgs) Handles btnWithdraw1.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "1"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnWithdrawPoint_Click(sender As Object, e As EventArgs) Handles btnWithdrawPoint.Click
        'Entering the withdrawal amount into the textbox & variable
        If txtAmountWithdraw.Text = "" Then
            txtAmountWithdraw.Text = txtAmountWithdraw.Text & "0."
            WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
        Else
            txtAmountWithdraw.Text = txtAmountWithdraw.Text & "."
            WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
        End If
    End Sub
    Private Sub btnWithdraw0_Click(sender As Object, e As EventArgs) Handles btnWithdraw0.Click
        'Entering the withdrawal amount into the textbox & variable
        txtAmountWithdraw.Text = txtAmountWithdraw.Text & "0"
        WithdrawalAmount = CDbl(txtAmountWithdraw.Text)
    End Sub
    Private Sub btnClearWithdrawal_Click(sender As Object, e As EventArgs) Handles btnClearWithdrawal.Click
        'clearing the textbox
        txtAmountWithdraw.Clear()
    End Sub
    Private Sub btnLodge2Home_Click(sender As Object, e As EventArgs) Handles btnLodge2Home.Click
        'return to the balance page
        TabControl1.SelectTab(1)
        TabControl3.SelectTab(0)
        pnlBalance.Visible = True
        pnlStatement.Visible = False
        txtAmountLodge.Clear()
        txtBalance.Text = "Balance: €" & Lodge.Balance
    End Sub
    Private Sub btnEnterWithdrawal_Click(sender As Object, e As EventArgs) Handles btnEnterWithdrawal.Click
        'finding the related data for the current user
        Dim objCustomer As DataRow, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        Withdraw.amount = WithdrawalAmount
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            Withdraw.Balance = objAccount.Item("Account_Balance")
        Next
        'lodging the money into the account
        If Withdraw.Balance > Withdraw.amount Then
            Withdraw.Withdraw()
            lblWithdrawAmount.Text = "€" & WithdrawalAmount
            lblBalUpdateWithdraw.Text = "€" & Withdraw.Balance
            TabControl2.SelectTab(1)
        Else
            MsgBox("Error! Insufficient funds in account!")
            txtAmountWithdraw.Clear()
        End If

    End Sub
    Private Sub btnConfirmWithdraw_Click(sender As Object, e As EventArgs) Handles btnConfirmWithdraw.Click
        'new datarow to hold the updated data
        'updated the item we want
        'send the update to the database
        Dim objRowCurrent As DataRow
        objRowCurrent = objDataSet.Tables("Bank").Rows.Find(currentUser)
        objRowCurrent("Account_Balance") = Withdraw.Balance
        objAccountDA.Update(objDataSet, "Bank")
        objDataSet.AcceptChanges()

        Dim objNewTransaction As DataRow
        TransactionID += 1
        objNewTransaction = objDataSet.Tables("Transactions").NewRow
        objNewTransaction.Item("Trans_ID") = TransactionID
        objNewTransaction.Item("Trans_Name") = "ATM Withdrawal"
        objNewTransaction.Item("Trans_Date") = Date.Today
        objNewTransaction.Item("Trans_Amount") = Withdraw.amount
        objNewTransaction.Item("Cust_ID") = currentUser
        objNewTransaction.Item("Trans_Type") = "Out"
        objDataSet.Tables("Transactions").Rows.Add(objNewTransaction)
        objTransactionDA.Update(objDataSet, "Transactions")
        objDataSet.AcceptChanges()

        'open the next page
        lblConfirmWithdrawalBal2.Text = "€" & Withdraw.Balance
        TabControl2.SelectTab(2)
    End Sub
    Private Sub btnWithdraw2Home_Click(sender As Object, e As EventArgs) Handles btnWithdraw2Home.Click
        'return to the balance page
        TabControl1.SelectTab(1)
        TabControl2.SelectTab(0)
        pnlBalance.Visible = True
        pnlTransactions.Visible = False
        txtAmountLodge.Clear()
        txtBalance.Text = "Balance €" & Withdraw.Balance
    End Sub
    Private Sub btnBackspace_Click(sender As Object, e As EventArgs) Handles btnBackspace.Click
        'code for backspace button to remove the last entered character
        Dim charLength As Integer = EnteredPin.Length
        If charLength = 4 Then
            txtPin.Text = txtPin.Text.Substring(0, 3)
            EnteredPin = EnteredPin.Substring(0, 3)
        ElseIf charLength = 3 Then
            txtPin.Text = txtPin.Text.Substring(0, 2)
            EnteredPin = EnteredPin.Substring(0, 2)
        ElseIf charLength = 2 Then
            txtPin.Text = txtPin.Text.Substring(0, 1)
            EnteredPin = EnteredPin.Substring(0, 1)
        Else
            txtPin.Text = ""
            EnteredPin = ""
        End If
    End Sub
    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        'enabling the user to edit their details
        'this disables navigation throughout the application
        btnBalance.Enabled = False
        btnStatement.Enabled = False
        btnTransactions.Enabled = False
        btnLogOut.Enabled = False
        txtCustomerName.ReadOnly = False
        txtCustomerAddress.ReadOnly = False
        txtCustPhone.ReadOnly = False
        btnEdit.Visible = False
        btnSave.Visible = True
        btnCancel.Visible = True
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        're-enabling naviagtion controls
        btnBalance.Enabled = True
        btnStatement.Enabled = True
        btnTransactions.Enabled = True
        btnLogOut.Enabled = True
        txtCustomerName.ReadOnly = True
        txtCustomerAddress.ReadOnly = True
        txtCustPhone.ReadOnly = True
        btnEdit.Visible = True
        txtWelcome.Text = "Welcome " & txtCustomerName.Text
        btnCancel.Visible = False
        btnSave.Visible = False

        'sending the changes to the database
        Dim objRowCurrent As DataRow
        objRowCurrent = objDataSet.Tables("Customer").Rows.Find(currentUser)
        objRowCurrent("Cust_Name") = txtCustomerName.Text
        objRowCurrent("Cust_Address") = txtCustomerAddress.Text
        objRowCurrent("Cust_Telephone") = txtCustPhone.Text
        objCustomerDA.Update(objDataSet, "Customer")
        objDataSet.AcceptChanges()

        'repopulating the changed arrays
        Dim strCurrentName As String
        Dim intCurrentpin As Integer
        For i As Integer = 0 To rowIndex
            strCurrentName = objDataSet.Tables("Customer").Rows(i).Item("Cust_Name")
            strNames(i) = strCurrentName
            intCurrentpin = objDataSet.Tables("Customer").Rows(i).Item("Pin")
            intPin(i) = intCurrentpin
        Next

    End Sub
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        're-enabling naviagtion controls
        btnBalance.Enabled = True
        btnStatement.Enabled = True
        btnTransactions.Enabled = True
        btnLogOut.Enabled = True
        txtCustomerName.ReadOnly = True
        txtCustomerAddress.ReadOnly = True
        txtCustPhone.ReadOnly = True
        btnEdit.Visible = True
        btnSave.Visible = False
        btnCancel.Visible = False

        'populating textboxes with the current users info
        Dim objCustomer As DataRow, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            txtAccountNumber.Text = objAccount.Item("Account_Number")
            cboAccountType.Text = objAccount.Item("Account_Type")
            txtCustomerName.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Name")
            txtCustomerAddress.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Address")
            txtCustPhone.Text = objDataSet.Tables("Customer").Rows.Find(currentUser).Item("Cust_Telephone")
        Next
    End Sub
    Private Sub btnPin_Click(sender As Object, e As EventArgs) Handles btnPin.Click
        'gathering the new pin and sending it to the database
        Dim objRowCurrent As DataRow
        objRowCurrent = objDataSet.Tables("Customer").Rows.Find(currentUser)
        objRowCurrent("Pin") = InputBox("Please enter yout New 4 digit pin")
        objCustomerDA.Update(objDataSet, "Customer")
        objDataSet.AcceptChanges()

        'repopulating the changed arrays
        Dim strCurrentName As String
        Dim intCurrentpin As Integer
        For i As Integer = 0 To rowIndex
            strCurrentName = objDataSet.Tables("Customer").Rows(i).Item("Cust_Name")
            strNames(i) = strCurrentName
            intCurrentpin = objDataSet.Tables("Customer").Rows(i).Item("Pin")
            intPin(i) = intCurrentpin
        Next
    End Sub
    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        'Displays team members
        TabControl1.SelectTab(6)
        pnlBalance.Focus()
    End Sub
    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        'Displays team members
        TabControl1.SelectTab(6)
        pnlBalance.Focus()
    End Sub
    Private Sub btnWithdrawal_Click(sender As Object, e As EventArgs) Handles btnWithdrawal.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(2)
        TabControl2.SelectTab(0)
        TabControl3.SelectTab(0)
        txtAmountLodge.Clear()
        txtAmountWithdraw.Clear()
    End Sub
    Private Sub btnLodgement_Click(sender As Object, e As EventArgs) Handles btnLodgement.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(3)
        TabControl2.SelectTab(0)
        TabControl3.SelectTab(0)
        txtAmountLodge.Clear()
        txtAmountWithdraw.Clear()
    End Sub
    Private Sub btnViewTransactions_Click(sender As Object, e As EventArgs) Handles btnViewTransactions.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(8)

        'Adding columns to datagridview
        DataGridView1.ColumnCount = 5
        DataGridView1.Columns(0).Name = "Transaction ID"
        DataGridView1.Columns(1).Name = "Transaction Name"
        DataGridView1.Columns(2).Name = "Transaction Date"
        DataGridView1.Columns(3).Name = "Amount"
        DataGridView1.Columns(4).Name = "IN/OUT"

        'populating datagridview with the current users info
        DataGridView1.Rows.Clear()
        Dim objCustomer As DataRow, objTransactions As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        Dim TransID, TransName, TransDate, Amount, TransType As String
        Dim newRow(5) As String
        For Each objTransactions In objCustomer.GetChildRows("Customer2Transactions")
            TransID = objTransactions.Item("Trans_ID")
            TransName = objTransactions.Item("Trans_Name")
            TransDate = objTransactions.Item("Trans_Date")
            Amount = objTransactions.Item("Trans_Amount")
            TransType = objTransactions.Item("Trans_Type")
            newRow(0) = TransID
            newRow(1) = TransName
            newRow(2) = TransDate
            newRow(3) = Amount
            newRow(4) = TransType
            DataGridView1.Rows.Add(newRow)
        Next
        DataGridView1.AutoResizeColumns()
        DataGridView1.RowHeadersVisible = False

    End Sub
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(7)
        pnlBalance.Visible = False
        pnlStatement.Visible = False
        pnlDetails.Visible = False
        pnlTransactions.Visible = True
    End Sub
    Private Sub btnTransfer_Click(sender As Object, e As EventArgs) Handles btnTransfer.Click
        'Controls made visible and invisble as needed
        TabControl1.SelectTab(9)
        txtTransferAmount.Text = ""
    End Sub
    Private Sub btnTransfer9_Click(sender As Object, e As EventArgs) Handles btnTransfer9.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "9"
    End Sub
    Private Sub btnTransfer8_Click(sender As Object, e As EventArgs) Handles btnTransfer8.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "8"
    End Sub
    Private Sub btnTransfer7_Click(sender As Object, e As EventArgs) Handles btnTransfer7.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "7"
    End Sub
    Private Sub btnTransfer6_Click(sender As Object, e As EventArgs) Handles btnTransfer6.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "6"
    End Sub
    Private Sub btnTransfer5_Click(sender As Object, e As EventArgs) Handles btnTransfer5.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "5"
    End Sub
    Private Sub btnTransfer4_Click(sender As Object, e As EventArgs) Handles btnTransfer4.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "4"
    End Sub
    Private Sub btnTransfer3_Click(sender As Object, e As EventArgs) Handles btnTransfer3.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "3"
    End Sub
    Private Sub btnTransfer2_Click(sender As Object, e As EventArgs) Handles btnTransfer2.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "2"
    End Sub
    Private Sub btnTransfer1_Click(sender As Object, e As EventArgs) Handles btnTransfer1.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "1"
    End Sub
    Private Sub btnTransfer0_Click(sender As Object, e As EventArgs) Handles btnTransfer0.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "0"
    End Sub
    Private Sub btnTransferPoint_Click(sender As Object, e As EventArgs) Handles btnTransferPoint.Click
        'entering the transfer amount
        txtTransferAmount.Text = txtTransferAmount.Text & "."
    End Sub
    Private Sub btnTransferBackspace_Click(sender As Object, e As EventArgs) Handles btnTransferBackspace.Click
        'code for backspace button to remove the last entered character
        Dim charLength As Integer = txtTransferAmount.Text.Length
        txtTransferAmount.Text = txtTransferAmount.Text.Substring(0, charLength - 1)
    End Sub
    Private Sub btnTransferConfirm_Click(sender As Object, e As EventArgs) Handles btnTransferConfirm.Click
        'finding the related data for the current user
        Dim objCustomer As DataRow, objAccount As DataRow
        objCustomer = objDataSet.Tables("Customer").Rows.Find(currentUser)
        Transfer.amount = CDbl(txtTransferAmount.Text)
        For Each objAccount In objCustomer.GetChildRows("Customer2Bank")
            Transfer.Balance = objAccount.Item("Account_Balance")
        Next

        'setting up transaction
        If Transfer.Balance > Transfer.amount Then
            Transfer.Transfer()
            txtTransactionAmount.Text = "€" & Transfer.amount
            txtTransactionID.Text = TransactionID + 1
            txtTransactionName.Text = "Bank Transfer - " & txtMessage.Text
            txtTransactionDate.Text = Date.Today
            TabControl1.SelectTab(10)
        Else
            MsgBox("Error! Insufficient funds in account!")
            txtTransferAmount.Clear()
        End If

    End Sub
    Private Sub btnConfirmTransaction_Click(sender As Object, e As EventArgs) Handles btnConfirmTransaction.Click
        'new datarow to hold the updated data
        'updated the item we want
        'send the update to the database
        Dim TransName As String = txtTransactionName.Text
        Dim objRowCurrent As DataRow
        objRowCurrent = objDataSet.Tables("Bank").Rows.Find(currentUser)
        objRowCurrent("Account_Balance") = Transfer.Balance
        objAccountDA.Update(objDataSet, "Bank")
        objDataSet.AcceptChanges()

        'executing the transaction
        Dim objNewTransaction As DataRow
        TransactionID += 1
        objNewTransaction = objDataSet.Tables("Transactions").NewRow
        objNewTransaction.Item("Trans_ID") = TransactionID
        objNewTransaction.Item("Trans_Name") = TransName
        objNewTransaction.Item("Trans_Date") = Date.Today
        objNewTransaction.Item("Trans_Amount") = Transfer.amount
        objNewTransaction.Item("Cust_ID") = currentUser
        objNewTransaction.Item("Trans_Type") = "Out"
        objDataSet.Tables("Transactions").Rows.Add(objNewTransaction)
        objTransactionDA.Update(objDataSet, "Transactions")
        objDataSet.AcceptChanges()

        'updating the recipients balance
        Dim objRowAlternate As DataRow
        objRowAlternate = objDataSet.Tables("Bank").Rows.Find(CInt(txtTransferTo.Text))
        objRowAlternate("Account_Balance") = objRowAlternate("Account_Balance") + Transfer.amount
        objAccountDA.Update(objDataSet, "Bank")
        objDataSet.AcceptChanges()

        'executing the recipients transaction
        Dim objNewTransactionAlternate As DataRow
        TransactionID += 1
        objNewTransactionAlternate = objDataSet.Tables("Transactions").NewRow
        objNewTransactionAlternate.Item("Trans_ID") = TransactionID
        objNewTransactionAlternate.Item("Trans_Name") = TransName
        objNewTransactionAlternate.Item("Trans_Date") = Date.Today
        objNewTransactionAlternate.Item("Trans_Amount") = Transfer.amount
        objNewTransactionAlternate.Item("Cust_ID") = CInt(txtTransferTo.Text)
        objNewTransactionAlternate.Item("Trans_Type") = "In"
        objDataSet.Tables("Transactions").Rows.Add(objNewTransactionAlternate)
        objTransactionDA.Update(objDataSet, "Transactions")
        objDataSet.AcceptChanges()

        lblNewTransBalance.Text = "€" & Transfer.Balance
        TabControl1.SelectTab(11)
    End Sub
    Private Sub btnReturnBack_Click(sender As Object, e As EventArgs) Handles btnReturnBack.Click
        'changing screen
        TabControl1.SelectTab(7)
    End Sub
#End Region
#Region "Subs"
    Public Sub HideTabs(Tab As Object)
        'The following code is taken from "codeproject.com.Questions/614157/How-To-Hide-TabControl-Headers"
        'It removes the tab headers from the tabcontrol
        Tab.Appearance = TabAppearance.FlatButtons
        Tab.ItemSize = New Size(0, 1)
        Tab.SizeMode = TabSizeMode.Fixed
        For Each TabPage In Tab.TabPages
            TabPage.text = ""
        Next
    End Sub
    Public Sub FillData()
        'clearing the dataset
        objDataSet.Clear()

        'setting the schema and filling
        objAccountDA.FillSchema(objDataSet, SchemaType.Source, "Bank")
        objAccountDA.Fill(objDataSet, "Bank")

        objCustomerDA.FillSchema(objDataSet, SchemaType.Source, "Customer")
        objCustomerDA.Fill(objDataSet, "Customer")

        objTransactionDA.FillSchema(objDataSet, SchemaType.Mapped, "Transactions")
        objTransactionDA.Fill(objDataSet, "Transactions")

        'setting the relationships
        objDataSet.Relations.Clear()
        objDataSet.Relations.Add("Customer2Bank", objDataSet.Tables("Customer").Columns("Customer_ID"), objDataSet.Tables("Bank").Columns("Customer_ID"))
        objDataSet.Relations.Add("Customer2Transactions", objDataSet.Tables("Customer").Columns("Customer_ID"), objDataSet.Tables("Transactions").Columns("Cust_ID"))

        'setting rowIndex to count the number of rows
        rowIndex = objDataSet.Tables("Customer").Rows.Count - 1

        'temp variables to hold current names
        Dim strCurrentName As String
        Dim intCurrentPin As Integer
        Dim i As Integer
        'loading the required data
        For i = 0 To rowIndex
            strCurrentName = objDataSet.Tables("Customer").Rows(i).Item("Cust_Name")
            strNames(i) = strCurrentName
            intCurrentPin = objDataSet.Tables("Customer").Rows(i).Item("Pin")
            intPin(i) = intCurrentPin
        Next
        Dim TransRows As Integer = objDataSet.Tables("Transactions").Rows.Count
        TransactionID = TransRows
    End Sub
#End Region
#Region "Easter Egg"
    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        'Opening EasterEgg
        TabControl1.SelectTab(5)
        EasterEgg.Document.Window.ScrollTo(0, 600)
    End Sub
    Private Sub btnReturn_Click(sender As Object, e As EventArgs) Handles btnReturn.Click
        'Leaving EasterEgg
        If LoggedIn = True Then
            TabControl1.SelectTab(1)
            pnlTransactions.Visible = False
            pnlStatement.Visible = False
            pnlDetails.Visible = False
            pnlBalance.Visible = True
            pnlBalance.Focus()
        Else
            TabControl1.SelectTab(0)
        End If
    End Sub
#End Region
End Class