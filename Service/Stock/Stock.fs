/// This module exposes use-cases of the Stock component as an HTTP Web service using Giraffe.
module StorageMachine.Stock.Stock

open System.Text.RegularExpressions
open Giraffe
open Microsoft.AspNetCore.Http
open Thoth.Json.Net
open Thoth.Json.Giraffe
open Stock

/// An overview of all bins currently stored in the Storage Machine.
let binOverview (next: HttpFunc) (ctx: HttpContext) =
    task {
        let dataAccess = ctx.GetService<IStockDataAccess> ()
        let bins = Stock.binOverview dataAccess
        return! ThothSerializer.RespondJsonSeq bins Serialization.encoderBin next ctx 
    }

/// An overview of actual stock currently stored in the Storage Machine. Actual stock is defined as all non-empty bins.
let stockOverview (next: HttpFunc) (ctx: HttpContext) =
    task {
        let dataAccess = ctx.GetService<IStockDataAccess> ()
        let bins = Stock.stockOverview dataAccess
        return! ThothSerializer.RespondJsonSeq bins Serialization.encoderBin next ctx 
    }

/// An overview of all products stored in the Storage Machine, regardless what bins contain them.
let productsInStock (next: HttpFunc) (ctx: HttpContext) =
    task {
        let dataAccess = ctx.GetService<IStockDataAccess> ()
        let productsOverview = Stock.productsInStock dataAccess
        return! ThothSerializer.RespondJson productsOverview Serialization.encoderProductsOverview next ctx 
    }
    
let storeBin (next: HttpFunc) (ctx: HttpContext) =
    task {
        let dataAccess = ctx.GetService<IStockDataAccess> ()
        let! binData = ThothSerializer.ReadBody ctx Serialization.decoderBin
        match binData with
        | Ok bin ->
            let databaseResponse = Stock.addBin dataAccess bin
            match databaseResponse  with
            | Some addBin -> return! ThothSerializer.RespondJson addBin Serialization.encoderBin next ctx
            | None -> return! RequestErrors.badRequest (text "test") earlyReturn ctx
        | Error _ ->
            return! RequestErrors.badRequest (text "Post unexpected input") earlyReturn ctx
    }
    

/// Defines URLs for functionality of the Stock component and dispatches HTTP requests to those URLs.
let handlers : HttpHandler =
    choose [
        GET >=> route "/bins" >=> binOverview
        GET >=> route "/stock" >=> stockOverview
        GET >=> route "/stock/products" >=> productsInStock
        POST >=> route "/stock/addBin" >=> storeBin
    ]