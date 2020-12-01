module WebApp
open Database
open Models
open Shared
open System

module KuaJing = 
    let convertKJProduct (prd:KuaJingProduct) : KuaJingProductForView = 
        { 
        _id = prd._id.ToString()
        productname = prd.productname
        producturl = prd.producturl
        price1 = prd.price1
        amount1 = prd.amount1
        price2 = prd.price2
        amount2 = prd.amount2
        price3 = prd.price3
        amount3 = prd.amount3
        createdOn = String.Empty
        brand = prd.brand
        }
    let rawKJRows = 
        let src = Mongo.Read.readKJAll()
        match src.IsNone with
        | false -> src.Value |> Seq.toArray
        | true -> [||]
    let trimRows (prdArray : KuaJingProduct array) = rawKJRows |> Array.filter(fun x -> x.brand.ToLower() <> "#n/a" && x.brand.ToLower() <> "0" && x.brand.ToLower() <>"other" && x.brand.ToLower() <>"other/其他" && x.brand.ToLower() <>"其他")
    let prdDetailByBrand = (trimRows rawKJRows) |> Array.map convertKJProduct |> Array.groupBy(fun x -> x.brand.ToLower()) 
    let getAllBrand() = prdDetailByBrand |> Array.unzip |> fst
    let getAllBrandWithKeyword (input : (string * KuaJingProductForView [])[])= 
            Array.collect (fun row -> 
                let brand,prdArr = row
                let keywordList = 
                    prdArr
                    |> Array.map(fun row -> 
                        let kwArr = Tokenize.splitString(row.productname)
                        let objId = row._id.ToString()
                        brand,kwArr,objId)
                keywordList) input
        
module HKTV = 
    let convertHKTVProduct (prd:HKTVProduct) : HKTVProductForView = 
        { 
        _id = prd._id.ToString()
        productName = prd.productName
        productUrl = prd.productUrl
        productPrice = prd.productPrice
        Sales = prd.Sales
        createdOn = prd.createdOn.createdOn
        }
        
    let rawHKrows = 
        let src = Mongo.Read.readHKTVAll()
        match src.IsNone with
        | false -> src.Value |> Seq.map convertHKTVProduct |> Seq.toArray
        | true -> [||]

    let getrelatedProduct (item : (string*string array *string)) = 
        let brandFromKJ,keywordArr,objId = item
        let output = 
            rawHKrows 
            |> Array.map(fun row -> row.productName,row)
            |> Array.filter(fun row -> (row |> fst).ToLower().Contains(brandFromKJ.ToLower())) // Check if the productname in hktv contains kj brand
        match Array.isEmpty output with // if doesnt contain this brand then skip
        | true -> [||]
        | false -> 
            output
            |> Array.filter(fun row -> 
                let pName = TextHandle.trimString (row|>fst)
                let kwArr = Array.collect TextHandle.trimString keywordArr |> Array.distinct
                TextHandle.checkString kwArr pName)
            |> Array.map(fun row -> 
                row|>fst,row|>snd,objId)
    let filterHKTVProductByBrandAndKeyword (item : string) = 
        match String.IsNullOrEmpty(item) with
        | true -> 
            rawHKrows |> Array.map(fun r -> "",r,"") |> List.ofArray
        | false ->
            let getSrc = KuaJing.prdDetailByBrand |> Array.filter(fun row -> (row |> fst )= item)
            let output = KuaJing.getAllBrandWithKeyword getSrc
            let result = output |> Array.map getrelatedProduct |> Array.concat
            result |> List.ofArray
