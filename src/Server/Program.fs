open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open WebApp
open Shared
open Shared.TextHandle
open Types
open Models
open Saturn

let srcKJRows = KuaJing.getAllBrandWithKeyword
let rawHKrows = HKTV.rawHKrows

let apiAction : ApiActions = {
    getFilterHKProductByBrandKW =  fun brandName -> (HKTV.filterHKTVProductByBrandAndKeyword brandName) |> toAsync           
    allBrand = fun () -> KuaJing.getAllBrand() |> List.ofArray |> toAsync
    allHKTVProduct = fun () -> rawHKrows |> List.ofArray |> toAsync
    allKJProduct = fun () -> (KuaJing.prdDetailByBrand |> Array.unzip |> snd |> Array.concat) |> List.ofArray |> toAsync
    //getFilterProduct = fun brandName -> (HKTV.filterHKTVProductByBrandAndKeyword brandName) 
}
let docs = Docs.createFor<ApiActions>()
let ApiDocs = 
    Remoting.documentation "API Docs" [
        docs.route <@ fun api -> api.allBrand @>
        |> docs.alias "AllBrand Route"

        docs.route <@ fun api -> api.getFilterHKProductByBrandKW @>
        |> docs.alias "get filter product"
        |> docs.example <@ fun api -> api.getFilterHKProductByBrandKW "nakaya" @>
    ]


let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue apiAction
    |> Remoting.withDiagnosticsLogger (printfn "%s")
    |> Remoting.withDocs "/api/apiactions/docs" ApiDocs
    |> Remoting.buildHttpHandler



let app =
    application {
        url "http://localhost:8085"
        use_router webApp
        //memory_cache
        //use_static "public"
        //use_gzip
    }

run app
