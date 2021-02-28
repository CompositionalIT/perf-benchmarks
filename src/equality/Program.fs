open System

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Jobs

/// A normal customer record
type Customer = { CustomerId : int; Name : string; Age : int; Town : string }

/// A customer record which has custom equality to only check the CustomerId field.
[<CustomComparison; CustomEquality>]
type CustomCustomer =
    { CustomerId : int; Name : string; Age : int; Town : string }

    override this.Equals other =
        match other with
        | :? CustomCustomer as p -> (this :> IEquatable<_>).Equals p
        | _ -> false

    override this.GetHashCode () = this.CustomerId.GetHashCode()

    interface IEquatable<CustomCustomer> with
        member this.Equals other = other.CustomerId.Equals this.CustomerId

    interface IComparable with
        member this.CompareTo other =
            match other with
            | :? CustomCustomer as p -> (this :> IComparable<_>).CompareTo p
            | _ -> -1

    interface IComparable<CustomCustomer> with
        member this.CompareTo other = other.CustomerId.CompareTo this.CustomerId

/// A simple customer that only contains an ID.
type SimpleCustomer = { CustomerId : int }

/// Creates a collection of n customers of different types and tests how long it takes to insert
/// and then extract them all from a dictionary.
[<SimpleJob (RuntimeMoniker.NetCoreApp50)>]
type Benchmarks() =
    let mutable customers = Array.empty
    let mutable customersQuick = Array.empty
    let mutable justAnId = Array.empty
    let mutable justAnInt = Array.empty

    [<Params(100, 1000, 10000, 100000, 1000000)>]
    member val size = 0 with get, set

    [<GlobalSetup>]
    member this.Setup() =
        let getAge =
            let r = Random 123456
            fun () -> r.Next (18, 66)

        customersQuick <- [| for p in 1 .. this.size -> { CustomCustomer.CustomerId = p; Name = $"Customer {p}"; Age = getAge(); Town = $"TOWN {p}" }, p |]
        customers <- [| for p in 1 .. this.size -> { Customer.CustomerId = p; Name = $"Customer {p}"; Age = getAge(); Town = $"TOWN {p}" }, p |]
        justAnId <- [| for p in 1 .. this.size -> { CustomerId = p }, p |]
        justAnInt <- [| for p in 1 .. this.size -> p, p |]

    [<Benchmark>]
    member _.AutoGen() =
        let dictionary = readOnlyDict customers
        for p, _ in customers do
            ignore dictionary.[p]
    [<Benchmark>]
    member _.ManualGen() =
        let dictionary = readOnlyDict customersQuick
        for p, _ in customersQuick do
            ignore dictionary.[p]
    [<Benchmark>]
    member _.JustAnId() =
        let dictionary = readOnlyDict justAnId
        for p, _ in justAnId do
            ignore dictionary.[p]
    [<Benchmark (Baseline = true)>]
    member _.JustAnInt() =
        let dictionary = readOnlyDict justAnInt
        for p, _ in justAnInt do
            ignore dictionary.[p]

BenchmarkRunner.Run<Benchmarks>() |> ignore