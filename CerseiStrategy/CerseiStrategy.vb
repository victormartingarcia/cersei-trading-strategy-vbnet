Imports System
Imports System.Collections.Generic
Imports TradingMotion.SDKv2.Algorithms
Imports TradingMotion.SDKv2.Algorithms.InputParameters
Imports TradingMotion.SDKv2.Markets.Charts
Imports TradingMotion.SDKv2.Markets.Orders
Imports TradingMotion.SDKv2.Markets.Indicators.Momentum

Namespace CerseiStrategy

    ''' <summary>
    ''' Cersei rules:
    '''     * Entry: Buy when price breaks Rate-of-Change level
    '''     * Exit: Sets a Trailing Stop based on the entry price and moves according to price raise
    '''     * Filters: None
    ''' </summary>
    Public Class CerseiStrategy
        Inherits Strategy

        Dim rocr100Indicator As ROCR100Indicator
        Dim trailingStopOrder As Order

        Dim acceleration As Double
        Dim highestClose As Double
        
        Public Sub New(ByVal mainChart As Chart, ByVal secondaryCharts As List(Of Chart))
            MyBase.New(mainChart, secondaryCharts)
        End Sub

        ''' <summary>
        ''' Strategy Name
        ''' </summary>
        ''' <returns>The complete Name of the strategy</returns>
        Public Overrides ReadOnly Property Name As String
            Get
                Return "Cersei Strategy"
            End Get
        End Property

        ''' <summary>
        ''' Security filter that ensures the Position will be closed at the end of the trading session.
        ''' </summary>
        Public Overrides ReadOnly Property ForceCloseIntradayPosition As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Security filter that sets a maximum open position size of 1 contract (either side)
        ''' </summary>
        Public Overrides ReadOnly Property MaxOpenPosition As UInteger
            Get
                Return 1
            End Get
        End Property

        ''' <summary>
        ''' This strategy uses the Advanced Order Management mode
        ''' </summary>
        Public Overrides ReadOnly Property UsesAdvancedOrderManagement As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Strategy Parameter definition
        ''' </summary>
        ''' <returns>The exposed Parameters collection</returns>
        Public Overrides Function SetInputParameters() As InputParameterList

            Dim parameters As New InputParameterList()

            ' The previous N bars period ROCR 100 indicator will use
            parameters.Add(New InputParameter("ROCR 100 Period", 48))

            ' The distance between the entry and the initial stop loss order
            parameters.Add(New InputParameter("Trailing Stop Loss ticks distance", 85.0))

            ' Break level of ROCR 100 indicator we consider a buy signal
            parameters.Add(New InputParameter("ROCR 100 Buy signal trigger level", 103))

            Return parameters

        End Function

        ''' <summary>
        ''' Initialization method
        ''' </summary>
        Public Overrides Sub OnInitialize()

            log.Debug("CerseiStrategy onInitialize()")

            ' Adding a ROCR 100 indicator to strategy
            ' (see http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:price_oscillators)
            rocr100Indicator = New ROCR100Indicator(Bars.Close, Me.GetInputParameter("ROCR 100 Period"))
            Me.AddIndicator("ROCR 100 indicator", rocr100Indicator)

            ' Setting the initial acceleration for the trailing stop
            acceleration = 0.02

            ' Setting the initial highest close
            highestClose = 0.0

        End Sub

        ''' <summary>
        ''' Strategy enter/exit/filtering rules
        ''' </summary>
        Public Overrides Sub OnNewBar()

            Dim stopMargin As Double = Me.GetInputParameter("Trailing Stop Loss ticks distance") * Me.GetMainChart().Symbol.TickSize

            Dim buySignal As Integer = Me.GetInputParameter("ROCR 100 Buy signal trigger level")

            If rocr100Indicator.GetROCR100()(1) <= buySignal And rocr100Indicator.GetROCR100()(0) > buySignal And Me.GetOpenPosition() = 0 Then

                ' BUY SIGNAL: Entering long and placing a trailing stop loss
                Dim buyOrder As MarketOrder = New MarketOrder(OrderSide.Buy, 1, "Enter long position")
                trailingStopOrder = New StopOrder(OrderSide.Sell, 1, Me.Bars.Close(0) - stopMargin, "Trailing stop long exit")

                Me.InsertOrder(buyOrder)
                Me.InsertOrder(trailingStopOrder)

                ' Resetting acceleration and highest close
                acceleration = 0.02
                highestClose = Me.Bars.Close(0)

            ElseIf Me.GetOpenPosition() = 1 Then

                ' Checking if the price has moved in our favour
                If Me.Bars.Close(0) > highestClose Then

                    highestClose = Me.Bars.Close(0)

                    ' Increasing acceleration
                    acceleration = acceleration * (highestClose - trailingStopOrder.Price)

                    ' Checking if trailing the stop order would exceed the current market price
                    If trailingStopOrder.Price + acceleration < Me.Bars.Close(0) Then

                        ' Setting the new price for the trailing stop
                        trailingStopOrder.Price = trailingStopOrder.Price + acceleration
                        Me.ModifyOrder(trailingStopOrder)

                    Else

                        ' Cancelling the order and closing the position
                        Dim exitLongOrder As MarketOrder = New MarketOrder(OrderSide.Sell, 1, "Exit long position")

                        Me.InsertOrder(exitLongOrder)
                        Me.CancelOrder(trailingStopOrder)

                    End If

                End If

            End If

        End Sub

    End Class
End Namespace
