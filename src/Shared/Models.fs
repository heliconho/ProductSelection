module Models
open System
open MongoDB.Bson
open Thoth.Json

type [<CLIMutable>] createdOn = {createdOn : string} with static member Now = {createdOn = DateTime.Now.ToShortDateString()}

type  [<CLIMutable>]  brand = {brand : string}

type [<CLIMutable>] KuaJingProduct = {
    _id : ObjectId
    productname : string
    producturl : string
    price1 : double
    amount1 : string
    price2 : double 
    amount2 : string
    price3 : double 
    amount3 : string
    createdOn : createdOn 
    brand : string
}
type [<CLIMutable>] KuaJingProductForView = {
    _id : string
    productname : string
    producturl : string
    price1 : double
    amount1 : string
    price2 : double 
    amount2 : string
    price3 : double 
    amount3 : string
    createdOn : string 
    brand : string
}


type [<CLIMutable>] HKTVProduct = 
    {
    _id : ObjectId
    productName : string
    productUrl : string
    productPrice : float
    Sales : string
    createdOn : createdOn
    }
type [<CLIMutable>] HKTVProductForView = 
    {
        _id : string
        productName : string
        productUrl : string
        productPrice : float
        Sales : string
        createdOn : string
    }
