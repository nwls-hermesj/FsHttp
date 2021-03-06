
# !IMPORTANT
This docu and the referenced samples is work in progress.
# !IMPORTANT

# FsHttp

FsHttp is a convenient library for consuming HTTP/REST endpoints via F#. It is based on System.Net.Http.

[![NuGet Badge](http://img.shields.io/nuget/v/SchlenkR.FsHttp.svg?style=flat)](https://www.nuget.org/packages/SchlenkR.FsHttp) [![Build Status](https://travis-ci.org/ronaldschlenker/FsHttp.svg?branch=master)](https://travis-ci.org/ronaldschlenker/FsHttp)

The goal of FsHttp is to provide ways for describing HTTP requests in a convenient way, and it is inspired by the RestClient VSCode extension. It can be used in production code, in tests, and in F# interactive.

Parts of the code is taken from the [HTTP utilities of FSharp.Data](http://fsharp.github.io/FSharp.Data/library/Http.html).

FsHttp comes in 3 'flavours' that can be used to describe HTTP requests. Although it is a good thing to have 1 solution for a problem instead of 3, it's up to you which style you prefer.


## Getting Started

Have a look at a simple use case using a POST and json as data. Each style is handles more detailed in the upcoming sections. All flavours are equivalent in functionality; they only differ in syntax.

**Important: The general use cases described here use the operator-less syntax, but the concepts work for the other flavours as well. Differences are handled in the upcoming sections of each flavour.**

See `src\Samples` folder for use cases.

### CE Flavour

```fsharp
http {
    POST "https://reqres.in/api/users"
    CacheControl "no-cache"
    body json """
    {
        "name": "morpheus",
        "job": "leader"
    }
    """
}
```

The CE flavour uses F# Computation Expressions to describe requests. Have a look at the [Sample Script for CE DSL](Samples/Demo.DslCE.fsx) for detailed use cases.

### Pipe Flavour

```fsharp
post "https://reqres.in/api/users"
--cacheControl "no-cache"
--body
--json """
{
    "name": "morpheus",
    "job": "leader"
}
"""
|> send
```

Have a look at the [Sample Script for Pipe DSL](Samples/Demo.DslPipe.fsx) for detailed use cases.

### Op-Less Flavour

```fsharp
post "https://reqres.in/api/users"
    cacheControl "no-cache"
    body json """
    {
        "name": "morpheus",
        "job": "leader"
    }
    """
    send
```

Have a look at the [Sample Script for OP-less DSL](Samples/Demo.Dsl.fsx) for detailed use cases.

## Explicit Headers

When you want to have intellisense for the header values, you can use `H` for header and `B` for body (not for CE flavour):

```fsharp
post "https://reqres.in/api/users"
    H.cacheControl "no-cache"
    body
    B.json """ { "name": "morpheus" } """
    send
```

## URL Formatting

You can split URL query parameters or comment lines out by using F# line-comment syntax:

```fsharp
// &skip=5 won't be a part of the final url.
// Line breaks and trailing / leading spaces will be removed.
get "https://reqres.in/api/users
        ?page=2
        //&skip=5
        &delay=3"
    send
```

## Response Handling

There are several convenience functions for transforming responses. They can be found in `FsHttp.ResponseHandling` module. The source can be found here: [Response Handling](src/FsHttp/ResponseHandling.fs)

```fsharp
let users =
    get "https://reqres.in/api/users?page=2" send
    |> toJson
```


* send / sendAsync





* TODO: Config
* 


## FSharp Interactive Usage

When using FsHttp in F# Interactive, you should not reference the dll, but rather load the `FsHttp.fsx` file to add pretty print for requests and responses, and you should open the FsHttp.Fsi module to have control over the printing:

```fsharp
#load @"./packages/SchlenkR.FsHttp/netstandard2.0/FsHttp.fsx"

open FsHttp
open FsHttp.Fsi
// open your desired flavour - see sample files!
```

### Response Printing

When working inside FSI, there are several 'hints' that can be given to specify the way FSI will print the response. Have a look at [FsiPrinting](src/FsHttp/Fsi.fs) for details.

To specify 

**Examples**

```fsharp
// Default print options (don't print request; print response headers, a formatted preview of the content)
get @"https://reqres.in/api/users?page=2&delay=3" run go

// Default print options (see above) + max. content length of 100
get @"https://reqres.in/api/users?page=2&delay=3" run (show 100)

// Default print options (don't print request; print response headers, whole content formatted)
get @"https://reqres.in/api/users?page=2&delay=3" run expand
```
