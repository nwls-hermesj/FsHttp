
module FsHttp.Testing

open FSharp.Data

/// tee operator: useful for chaining expectations
let ( ||> ) x f =
    f x |> ignore
    x

//let (>>>) (y: 'b) (x: Async<'a>) =
//    y

let inline private raisef<'a, 'b, 'c> : Printf.StringFormat<'a, 'b> -> 'a =
    let otype =
        [
            "Xunit.Sdk.XunitException, xunit.assert"
            "NUnit.Framework.AssertionException, nunit.framework"
            "Expecto.AssertException, expecto"
        ]
        |> List.tryPick(System.Type.GetType >> Option.ofObj)
    match otype with
    | None -> failwithf
    | Some t ->
        let ctor = t.GetConstructor [| typeof<string> |]
        let exnCtor msg = ctor.Invoke [| msg |] :?> exn
        Printf.kprintf (exnCtor >> raise)

type ArrayComparison = | RespectOrder | IgnoreOrder
type StructuralComparison = | Subset | Exact

let private compareJson (arrayComparison: ArrayComparison) (expectedJson: JsonValue) (resultJson: JsonValue) =

    let rec toPaths (currentPath: string) (jsonValue: JsonValue) : ((string * obj) list) =
        match jsonValue with
        | JsonValue.Null -> [currentPath, null :> obj]
        | JsonValue.Record properties ->
            seq {
                for pName, pValue in properties do
                for innerPath in toPaths (sprintf "%s/%s" currentPath pName) pValue do
                yield innerPath
            } |> Seq.toList
        | JsonValue.Array values ->
            let indexedValues = values |> Array.mapi (fun i x -> i,x)
            seq {
                for index,value in indexedValues do
                let printedIndex = match arrayComparison with | RespectOrder -> index.ToString() | IgnoreOrder -> ""
                for inner in toPaths (sprintf "%s[%s]" currentPath printedIndex) value do
                yield inner
            } |> Seq.toList
        | JsonValue.Boolean b -> [currentPath, b :> obj]
        | JsonValue.Float f -> [currentPath, f :> obj]
        | JsonValue.String s -> [currentPath, s :> obj]
        | JsonValue.Number n -> [currentPath, n :> obj]

    let getPaths x = x |> toPaths "" |> List.map (fun (path,value) -> sprintf "%s{%A}" path value)
    (getPaths expectedJson, getPaths resultJson)

let jsonShouldLookLike
        (arrayComparison: ArrayComparison)
        (structuralComparison: StructuralComparison)
        (expectedJson: string)
        (resultJson: JsonValue) =

    let expectedPaths,resultPaths = compareJson arrayComparison (JsonValue.Parse expectedJson) resultJson
    let aggregateUnmatchedElements list =
        match list with
        | [] -> ""
        | x::xs -> xs |> List.fold (fun curr next -> curr + "\n" + next) x
    match structuralComparison with
    | Subset ->
        let eMinusR = expectedPaths |> List.except resultPaths
        match eMinusR with
        | [] -> resultJson
        | _ -> raisef "Elements not contained in source: \n%s" (eMinusR |> aggregateUnmatchedElements)
    | Exact ->
        let eMinusR = expectedPaths |> List.except resultPaths
        let rMinusE = resultPaths |> List.except expectedPaths
        match eMinusR, rMinusE with
        | [],[] -> resultJson
        | _ ->
            let a1 = (sprintf "Elements not contained in source: \n%s" (eMinusR |> aggregateUnmatchedElements))
            let a2 = (sprintf "Elements not contained in expectation: \n%s" (rMinusE |> aggregateUnmatchedElements))
            raisef "%s\n%s" a1 a2

let jsonStringShouldLookLike
        (arrayComparison: ArrayComparison)
        (structuralComparison: StructuralComparison)
        (expectedJson: string)
        (resultJson: string) =

    jsonShouldLookLike arrayComparison structuralComparison expectedJson (resultJson |> JsonValue.Parse)
    |> ignore

    resultJson

type JsonValue with
    member this.HasProperty(propertyName: string) =
        let prop = this.TryGetProperty propertyName
        match prop with
        | Some _ -> true
        | None -> false

let statusCodeShouldBe (code: System.Net.HttpStatusCode) (response: Response) =
    if response.statusCode <> code then
        raisef "Expected status code of %A, but was %A" code response.statusCode
    ()


// #if INTERACTIVE

// open Microsoft.FSharp.Reflection

// fsi.AddPrinter
//     (fun (x: obj) ->
//         if x <> null && x.GetType().ReflectedType.Name = typeof<TestResult<_>>.Name then

//         match tr with
//         | Ok _ -> "Ok"
//         | Failed(a,b) -> sprintf "Failed: %s" b
//         | _ -> null
//     )

// #endif


// let cases = typeof<TestResult<_>>.Name
// (Ok 1).GetType().ReflectedType.Name
// (Failed (1,"j")).GetType().ReflectedType.Name
// (Failed (1,"j")).GetType().Name

