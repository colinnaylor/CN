Attribute VB_Name = "basOptionValue32Wrapper"
'actual calculation is performed by the existing C DLL that was being used by the valuation spreadsheets
Declare Function vbOptVal Lib "S:\dlls\OptionValue32.dll" _
    (ByVal StrikePrice As Double, ByVal SharePrice As Double, _
                        ByVal Vol As Double, ByVal t As Double, _
                        ByVal RateFund As Double, ByVal RateDisc As Double, _
                        ByVal RateStock As Double, ByVal OptionType As Long, _
                        ByVal OptionStyle As Long, ByVal ModelSteps As Long, _
                        ByVal ModelType As Long, ByVal DivMethod As Long, _
                        VBDivis() As VBTermStruct_t) As Double


'structure to store dividends occuring before maturity of option - required for call to C Function
Public Type VBTermStruct_t
    time As Double
    Amount As Double
End Type


Public Function OptionValueWrapper(ByVal StrikePrice As Double, ByVal UnderlyingPrice As Double, ByVal UnderlyingVolatility As Double, ByVal YearsToMaturity As Double, ByVal RiskFreeRate As Double, ByVal OptionType As Integer, ByVal OptionStyle As Integer, ByVal Dividends As String)
    
    Dim tDivArray() As VBTermStruct_t
    
    'convert div string to array of termstructs
    tDivArray() = ParseDivString(Dividends)
    
    OptionValueWrapper = vbOptVal(StrikePrice, UnderlyingPrice, UnderlyingVolatility, YearsToMaturity, RiskFreeRate, RiskFreeRate, 0#, OptionType, OptionStyle, 50, 3, 1, tDivArray)
    
End Function

'the divs will be passed in a string of the format 't1/div1,t2/div2,...'
'i.e. 0.55/0.0045,1.55/0.0046
Function ParseDivString(DivString As String) As VBTermStruct_t()
    Dim tDivArray() As VBTermStruct_t
    Dim divs() As String
    Dim i As Integer
    
    Dim t As String
    Dim a As String
    
    'if nothing passed in return single empty element - required by C Function
    If DivString = "" Then
        ReDim tDivArray(0 To 0)
        tDivArray(0).time = 0#
        tDivArray(0).Amount = 0#
    Else
        divs = Split(DivString, ",")
        ReDim tDivArray(0 To UBound(divs))
        
        For i = 0 To UBound(divs)
        
            t = Split(divs(i), "/")(0)
            a = Split(divs(i), "/")(1)
            
            tDivArray(i).time = t
            tDivArray(i).Amount = a
        
            i = i + 1
        Next i
    End If
    

    ParseDivString = tDivArray
    
End Function


