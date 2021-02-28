open System

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Jobs

[<SimpleJob (RuntimeMoniker.NetCoreApp50)>]
type Benchmarks () =
    let pickValue =
        let r = Random()
        fun n -> r.Next(1, n).ToString()

    let mutable customerPair = Array.empty
    let mutable customerSingle = Array.empty

    [<Params(100, 1000, 10000, 100000)>] member val Customers = 0 with get, set
    [<Params(1, 10, 100, 1000, 10000)>] member val Orders = 0 with get, set

    [<GlobalSetup>]
    member this.Setup() =
        customerPair <- [| for i in 1 .. this.Customers -> string i, i |]
        customerSingle <- [| for i in 1 .. this.Customers -> string i |]

    [<Benchmark (Baseline = true)>]
    member this.Map () =
        let hs = Map customerPair
        for _ = 1 to this.Orders do
            hs.[pickValue this.Customers] |> ignore
    [<Benchmark>]
    member this.ReadOnlyDict () =
        let hs = readOnlyDict customerPair
        for _ = 1 to this.Orders do
            hs.[pickValue this.Customers] |> ignore
    [<Benchmark>]
    member this.Set () =
        let hs = Set customerSingle
        for _ = 1 to this.Orders do
            hs.Contains (pickValue this.Customers) |> ignore
    [<Benchmark>]
    member this.Array () =
        for _ = 1 to this.Orders do
            let get = pickValue this.Customers
            customerSingle |> Array.find ((=) get) |> ignore

BenchmarkRunner.Run<Benchmarks>() |> ignore