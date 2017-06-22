VERSION 5.00
Begin VB.Form Form1 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "OptionValue32 Test"
   ClientHeight    =   4575
   ClientLeft      =   45
   ClientTop       =   435
   ClientWidth     =   3525
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   4575
   ScaleWidth      =   3525
   StartUpPosition =   3  'Windows Default
   Begin VB.TextBox txtResult 
      Height          =   285
      Left            =   1800
      TabIndex        =   18
      Top             =   4080
      Width           =   1455
   End
   Begin VB.CommandButton Command1 
      Caption         =   "Get Price"
      Height          =   495
      Left            =   1800
      TabIndex        =   16
      Top             =   3360
      Width           =   1455
   End
   Begin VB.TextBox txtUndPrice 
      Height          =   285
      Left            =   1800
      TabIndex        =   3
      Top             =   600
      Width           =   1455
   End
   Begin VB.TextBox txtUndVol 
      Height          =   285
      Left            =   1800
      TabIndex        =   5
      Top             =   960
      Width           =   1455
   End
   Begin VB.TextBox txtYearToMat 
      Height          =   285
      Left            =   1800
      TabIndex        =   7
      Top             =   1320
      Width           =   1455
   End
   Begin VB.TextBox txtRiskFreeRate 
      Height          =   285
      Left            =   1800
      TabIndex        =   9
      Top             =   1680
      Width           =   1455
   End
   Begin VB.TextBox txtOptionType 
      Height          =   285
      Left            =   1800
      TabIndex        =   11
      Top             =   2040
      Width           =   1455
   End
   Begin VB.TextBox txtOptionStyle 
      Height          =   285
      Left            =   1800
      TabIndex        =   13
      Top             =   2400
      Width           =   1455
   End
   Begin VB.TextBox txtDivString 
      Height          =   285
      Left            =   1800
      TabIndex        =   15
      Top             =   2760
      Width           =   1455
   End
   Begin VB.TextBox txtStrike 
      Height          =   285
      Left            =   1800
      TabIndex        =   1
      Top             =   240
      Width           =   1455
   End
   Begin VB.Label Label2 
      Caption         =   "Result"
      Height          =   255
      Left            =   240
      TabIndex        =   17
      Top             =   4080
      Width           =   1215
   End
   Begin VB.Label Label9 
      Caption         =   "Underlying Price"
      Height          =   255
      Left            =   240
      TabIndex        =   2
      Top             =   600
      Width           =   1215
   End
   Begin VB.Label Label8 
      Caption         =   "Underlying Vol"
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   960
      Width           =   1215
   End
   Begin VB.Label Label7 
      Caption         =   "Years to maturity"
      Height          =   255
      Left            =   240
      TabIndex        =   6
      Top             =   1320
      Width           =   1215
   End
   Begin VB.Label Label6 
      Caption         =   "Risk free rate"
      Height          =   255
      Left            =   240
      TabIndex        =   8
      Top             =   1680
      Width           =   1215
   End
   Begin VB.Label Label5 
      Caption         =   "Option Type"
      Height          =   255
      Left            =   240
      TabIndex        =   10
      Top             =   2040
      Width           =   1215
   End
   Begin VB.Label Label4 
      Caption         =   "Option Style"
      Height          =   255
      Left            =   240
      TabIndex        =   12
      Top             =   2400
      Width           =   1215
   End
   Begin VB.Label Label3 
      Caption         =   "Dividends"
      Height          =   255
      Left            =   240
      TabIndex        =   14
      Top             =   2760
      Width           =   1215
   End
   Begin VB.Label Label1 
      Caption         =   "Strike Price"
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   1215
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub Command1_Click()

    Dim priceClass As New OptionValue32_VB.Pricer
    Dim strike As Double
    Dim undPrice As Double
    Dim undVol As Double
    Dim riskFreeRate As Double
    Dim yearsToMaturity As Double
    Dim OpType As Integer
    Dim OpStyle As Integer
    Dim divs As String
    Dim ret As Double
    
    strike = txtStrike.Text
    undPrice = txtUndPrice.Text
    undVol = txtUndVol.Text
    riskFreeRate = txtRiskFreeRate.Text
    yearsToMaturity = txtYearToMat.Text
    OpType = txtOptionType.Text
    OpStyle = txtOptionStyle.Text
    divs = txtDivString.Text
    
    ret = priceClass.OptionValue(strike, undPrice, undVol, yearsToMaturity, riskFreeRate, OpType, OpStyle, divs)
    
    txtResult.Text = ret
    

End Sub

Private Sub Form_Load()

    txtStrike.Text = "0.99"
    txtUndPrice.Text = "0.90"
    txtUndVol.Text = "0.1"
    txtYearToMat.Text = "0.25"
    txtRiskFreeRate.Text = "0.04"
    txtOptionType.Text = "0"
    txtOptionStyle.Text = "0"
    txtDivString.Text = "0.15/0.03"

End Sub
