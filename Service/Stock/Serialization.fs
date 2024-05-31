/// Serialization and deserialization for types/values of the Stock component.
module StorageMachine.Stock.Serialization

open Thoth.Json.Net
open FsToolkit.ErrorHandling
open StorageMachine
open Common
open Bin
open Stock

/// JSON serialization of a bin.
let encoderBin : Encoder<Bin> = fun bin ->
    Encode.object [
        "binIdentifier", (let (BinIdentifier identifier) = bin.Identifier in Encode.string identifier)
        "content", (Encode.option (fun (PartNumber partNumber) -> Encode.string partNumber) bin.Content)
    ]


/// JSON deserialization of a bin identifier.
let decoderBinIdentifier : Decoder<BinIdentifier> =
    Decode.string
    |> Decode.andThen (fun s ->
        match BinIdentifier.make s with
        | Ok binIdentifier -> Decode.succeed binIdentifier
        | Error validationMessage -> Decode.fail validationMessage
    )
// EXERCISE: Is this decoder in the right place (in the architecture) here?
    
/// JSON deserialization of a part number.
let decoderPartNumber : Decoder<PartNumber> =
    Decode.string
    |> Decode.andThen (fun s ->
        match PartNumber.make s with
        | Ok partNumber -> Decode.succeed partNumber
        | Error validationMessage -> Decode.fail validationMessage
    )

/// JSON serialization of a stock product.
let encoderProduct : Encoder<Product> = fun (Product partNumber) ->
    Encode.object [
        "partNumber", (let (PartNumber partNumber) = partNumber in Encode.string partNumber)
    ]

/// JSON serialization of a complete products overview.
let encoderProductsOverview : Encoder<ProductsOverview> = fun productsOverview ->
    Encode.seq [
        for (product, quantity) in productsOverview do
            yield Encode.object [
                "product", encoderProduct product
                "total", Encode.int quantity
            ]
    ]
    
    
let decoderBin : Decoder<Bin> =
    Decode.object (fun get ->
        {
            Identifier = get.Required.Field "Identifier" decoderBinIdentifier
            Content = get.Optional.Field "Content" decoderPartNumber
        }
    ) 