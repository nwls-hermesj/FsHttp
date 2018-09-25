
#r "netstandard"
#r "System.Net.Http"
#r "./FsHttp.dll"

open FsHttp

fsi.AddPrinter
    (fun (r: Response) ->
        let content =
            match r.printHint with
            | Preview maxLength -> toStringN maxLength r
            | Expand -> toString r
            | Header -> toStringN 500 r
        sprintf "%s\n%s" (headerToString r) content
    )
    