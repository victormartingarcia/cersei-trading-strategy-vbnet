Imports System
Imports System.Configuration
Imports TradingMotion.SDKv2.Algorithms.Debug
Imports TradingMotion.SDKv2.Markets
Imports TradingMotion.SDKv2.Markets.Charts
Imports TradingMotion.SDKv2.Markets.Symbols
Imports TradingMotion.SDKv2.WebServices

Namespace CerseiStrategy

    Module DebugBacktest

        Sub Main()

            ' IMPORTANT INFORMATION
            ' =====================
            '
            ' The purpose of this console application is to allow you to test/debug the Strategy
            ' while you are developing it. 
            ' 
            ' Running this project will perform n Ibex 6 month backtest on the Strategy, using 60 min bars.
            ' 
            ' Once the backtest is finished, you will be able to launch the TradingMotionSDKToolkit 
            ' application to see the graphical result.
            ' 
            ' If you want to debug your code you can place breakpoints on the Strategy subclass
            ' and Debug the project.
            ' 
            ' 
            ' REQUIRED CREDENTIALS: Edit your app.config and enter your login/password for accessing the TradingMotion API

            Dim startBacktestDate As DateTime = DateTime.Parse(DateTime.Now.AddMonths(-6).AddDays(-1).ToShortDateString() + " 00:00:00")
            Dim endBacktestDate As DateTime = DateTime.Parse(DateTime.Now.AddDays(-1).ToShortDateString() + " 23:59:59")

            TradingMotionAPIClient.Instance.SetUp("https://www.tradingmotion.com/api/webservice.asmx", ConfigurationManager.AppSettings("TradingMotionAPILogin"), ConfigurationManager.AppSettings("TradingMotionAPIPassword")) 'Enter your TradingMotion credentials on the app.config file
            HistoricalDataAPIClient.Instance.SetUp("https://barserver.tradingmotion.com/WSHistoricalDatav2/webservice.asmx")

            Dim s As CerseiStrategy = New CerseiStrategy(New Chart(SymbolFactory.GetSymbol("IX"), BarPeriodType.Minute, 60), Nothing)

            DebugStrategy.RunBacktest(s, startBacktestDate, endBacktestDate)

        End Sub

    End Module

End Namespace
