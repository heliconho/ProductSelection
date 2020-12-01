module Types
open Models
type ApiActions = {
    allBrand :unit -> Async<string list>
    allHKTVProduct : unit -> Async<HKTVProductForView list>
    allKJProduct : unit -> Async<KuaJingProductForView list>
    getFilterHKProductByBrandKW : string -> Async<(string*HKTVProductForView*string)list>
    //getFilterProduct : string -> Async<HKTVProductForView list>
}

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName