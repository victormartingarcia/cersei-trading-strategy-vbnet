Cersei Trading Strategy
============================================

Table of Contents
----

* [Overview](#overview)
* [Cersei Trading Rules](#Cersei-trading-rules)
* [Download](#download)
* [Quick Start](#quick-start)
* [User Manual](#user-manual)
* [About iSystems](#about-isystems)
* [Disclaimer](#disclaimer)

Overview
----

Cersei is a trading algorithm written in VB.Net using the [TradingMotion SDK] development tools (there is a [C# port] too).

![OHLC example chart](markdown_files/OHLC.png)
<sub>__Image footnote:__ Example of Cersei OHLC financial chart showing some automatic trades</sub>

The strategy code is all contained in [CerseiStrategy.cs], including a default parameter combination.

This default parameter combination has been optimized to run over 60' bars of _IBEX Future Index_.

Trading a maximum of 1 contract of IBEX Future, this is how performed (hypothetically) from 2001 to 20014:

![Net P&L chart](markdown_files/PL.png)
<sub>__Image footnote:__ Hypothetic Net P&L chart for Cersei strategy</sub>

Anyway, go open Visual Studio, clone the project and start with the trading algo development! Sure you can do better and improve all these figures :)

Cersei Trading Rules
----

Cersei's trading plan is quite simple. It __buys 1 contract__ when the price breaks above a specified rate-of-change (Momentum) level.

While the strategy has a long position in the market, it __places one exit order__. A _Trailing Stop_ (keep locking profits while the price is moving in our favour).

Besides, this is a pure __intraday strategy__. That means it won't leave any open position at the end of the session, so in case we still got a position it will be closed automatically.

### To sum up ###
```
CerseiStrategy rules:

  * Entry: Price breaks ROCR100 (Momentum) level (long-only)
  * Exit: Sets a Trailing Stop
  * Filters (sets the entry only under certain conditions): None
```

### Show me the code ###

Here is a simplified C# source code of Cersei's _OnNewBar()_ function. The complete code is all contained in [CerseiStrategy.cs] along with comments and definition of parameters.

```VB.net
Dim stopMargin As Decimal = Me.GetInputParameter("Trailing Stop Loss ticks distance") * Me.GetMainChart().Symbol.TickSize

Dim buySignal As Integer = Me.GetInputParameter("ROCR 100 Buy signal trigger level")

If rocr100Indicator.GetROCR100()(1) <= buySignal And rocr100Indicator.GetROCR100()(0) > buySignal And Me.GetOpenPosition() = 0 Then

    ' BUY SIGNAL: Entering long and placing a trailing stop loss
    Dim buyOrder As MarketOrder = New MarketOrder(OrderSide.Buy, 1, "Enter long position")
    trailingStopOrder = New StopOrder(OrderSide.Sell, 1, Me.Bars.Close(0) - stopMargin, "Trailing stop long exit")

    Me.InsertOrder(buyOrder)
    Me.InsertOrder(trailingStopOrder)

    ' Resetting acceleration and highest close
    acceleration = 0.02D
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
```

Download
----

First of all, make sure you have Visual Studio 2010 version (or higher). [TradingMotion SDK] is fully compatible with [Visual Studio Express] free versions.

Download TradingMotion [Visual Studio extension], and the windows desktop application [TradingMotionSDK Toolkit installer].


Quick Start
----

* Create a free account to access TradingMotionAPI (required). It can be created from TradingMotionSDK Toolkit (the desktop application)
* Clone the repository:
```sh
git clone https://github.com/victormartingarcia/Cersei-trading-strategy-csharp
```
* Open Visual Studio and load solution _CerseiStrategy/CerseiStrategy.sln_
* Edit _app.config_ file adding your TradingMotionAPI credentials on _appSettings_ section

And you're all set!

Running the project (F5) will perform a _development backtest simulation_ over last 6 months DAX 60' bars data.

Once it has finished, it will ask if you want to see the P&L report in TradingMotionSDK Toolkit. Pressing 'y' will load the same backtest with the desktop app, where it will show performance statistics, charts, and so on.

User Manual
----

__[More documentation in the Getting Started Guide]__

About iSystems
----

[iSystems] by [TradingMotion] is a marketplace for automated trading systems.

_iSystems_ has partnered with [11 international brokers](http://www.tradingmotion.com/Brokers) (and counting) that offer these trading systems to their clients (both corporate and retail) who pay for a license fee that the developer charges.

The trading systems run with live market data under a controlled environment in iSystems' datacenters.

This way the developers just need to worry about how to make their trading systems better and iSystems platform does the rest.

Visit [Developers] section on TradingMotion's website for more info on how to develop and offer your systems.

Disclaimer
----

I am R&D engineer at [TradingMotion LLC], and head of [TradingMotion SDK] platform. Beware, the info here can be a little biased ;)

  [C# port]: https://github.com/victormartingarcia/Cersei-trading-strategy-csharp
  [TradingMotion SDK]: http://sdk.tradingmotion.com
  [CerseiStrategy.cs]: CerseiStrategy/CerseiStrategy.cs
  [iSystems platform]: https://www.isystems.com
  [iSystems.com]: https://www.isystems.com
  [iSystems]: https://www.isystems.com
  [TradingMotion LLC]: http://www.tradingmotion.com
  [TradingMotion]: http://www.tradingmotion.com
  [Developers]: http://www.tradingmotion.com/Strategies/Developers
  [Visual Studio Express]: http://www.visualstudio.com/en-us/downloads#d-2010-express
  [TradingMotion SDK website]: http://sdk.tradingmotion.com
  [TradingMotionSDK Toolkit installer]: http://sdk.tradingmotion.com/files/TradingMotionSDKInstaller.msi
  [Visual Studio extension]: http://sdk.tradingmotion.com/files/TradingMotionSDK_VisualStudio.vsix
  [More documentation in the Getting Started Guide]: http://sdk.tradingmotion.com/GettingStarted
