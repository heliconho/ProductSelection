namespace Shared

module TextHandle = 
    open Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter
    open System.Linq
    open System
    let trimString (pname : string) = 
        let convertPName = ChineseConverter.Convert(pname,ChineseConversionDirection.SimplifiedToTraditional)
        convertPName.ToLower().ToCharArray() |> Array.filter(fun c -> not(Char.IsWhiteSpace(c))) |> Array.groupBy(id) |> Array.unzip |> fst
    let checkString (kwC:char array)(pnameC:char array)= kwC.All(fun c1 -> pnameC.Any(fun c2 -> c2 = c1))
    let toAsync x = async { return x }

module Tokenize=
    open JiebaNet
    open JiebaNet.Analyser
    open JiebaNet.Segmenter
    let splitString s = 
        let segmentor = JiebaSegmenter()
        let segments = segmentor.Cut(s,cutAll = true)
        segments |> Array.ofSeq