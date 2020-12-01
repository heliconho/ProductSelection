module Index
open Elmish
open Fable.Remoting.Client
open Models
open Types
open System
open Zanaptak.TypedCssClasses
open Feliz
open Fable.React
open Feliz.MaterialUI.MaterialTable
open Elmish.React
open Microsoft.FSharp.Core.Operators
open Fable.Core.Experimental

type PageModel = {
    ProductList : Result<HKTVProductForView list,string>
    KJProductList : Result<KuaJingProductForView list,string>
    BrandList : Result<string list,string>
    BrandSearch : string
}
with static member createDefault () = {ProductList = Ok [];KJProductList = Ok [];BrandList = Ok []; BrandSearch = ""}

type PageMsg = 
    | OnInitialLoadBrand
    | OnInitialLoadBrandSuccess of string list
    | OnInitialLoadBrandFailed of exn
    | OnInitialLoadProduct
    | OnInitialLoadProductSuccess of HKTVProductForView list
    | OnInitialLoadKJProduct
    | OnInitialLoadProductKJSuccess of KuaJingProductForView list
    | OnInitialLoadProductFailed of exn
    | OnFillInSearch of string
    | OnBrandSearch
    | OnSearchSuccess of HKTVProductForView list
    | OnSearchFailed of exn


let Api =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ApiActions>

let getInitialBrandData() = async{
    let! blist = Api.allBrand()
    match List.isEmpty blist with
    | false -> return blist
    | true -> return []
}
let getInitialProductData() = async{
    let! plist = Api.allHKTVProduct()
    match List.isEmpty plist with
    | false -> return plist
    | true -> return []
}

let getKJInitialProductData() = async{
    let! plist = Api.allKJProduct()
    match List.isEmpty plist with
    | false -> return plist
    | true -> return []
}

let filterProduct (brandName:string) = async{
    let! filterres = Api.getFilterHKProductByBrandKW brandName
    let _,Res,_ = filterres |> List.unzip3
    match List.isEmpty Res with
    | false -> return Res 
    | true -> return []
}

let update message model =
    match message with
    | OnInitialLoadBrand -> model,Cmd.OfAsync.either getInitialBrandData () OnInitialLoadBrandSuccess OnInitialLoadBrandFailed
    | OnInitialLoadBrandSuccess bList -> {model with BrandList = Ok bList}, Cmd.none
    | OnInitialLoadBrandFailed ex -> {model with BrandList = Error ex.Message },Cmd.none
    | OnInitialLoadProduct -> model,Cmd.OfAsync.either getInitialProductData () OnInitialLoadProductSuccess OnInitialLoadProductFailed
    | OnInitialLoadKJProduct -> model,Cmd.OfAsync.either getKJInitialProductData () OnInitialLoadProductKJSuccess OnInitialLoadProductFailed
    | OnInitialLoadProductSuccess pList -> {model with ProductList = Ok pList}, Cmd.none
    | OnInitialLoadProductKJSuccess pList -> {model with KJProductList = Ok pList}, Cmd.none
    | OnInitialLoadProductFailed ex -> {model with ProductList = Error ex.Message },Cmd.none
    | OnFillInSearch brandSearch -> {model with BrandSearch = brandSearch},Cmd.none
    | OnBrandSearch -> model,Cmd.OfAsync.either filterProduct model.BrandSearch OnSearchSuccess OnSearchFailed
    | OnSearchSuccess newPlist -> {model with ProductList = Ok newPlist},Cmd.none
    | _ -> model,Cmd.none
let init () = PageModel.createDefault(),Cmd.batch[Cmd.ofMsg OnInitialLoadBrand ; Cmd.ofMsg OnInitialLoadProduct ; Cmd.ofMsg OnInitialLoadKJProduct]


type Icon = CssClasses<"https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css", Naming.PascalCase>
type BulmaCSS = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.1/css/bulma.min.css",Naming.PascalCase>

let renderBList (model : PageModel) dispatch = 
    let bListView = 
        Html.div[
            prop.className BulmaCSS.Box
            prop.children[
                Html.ul[
                    Html.input[
                        prop.id "search-box"
                        prop.placeholder "Enter a brand name to start searching"
                        prop.valueOrDefault model.BrandSearch
                        prop.onTextChange(fun ev -> OnFillInSearch ev |> dispatch)
                        prop.onKeyUp(key.enter,fun ev -> OnBrandSearch |> dispatch)
                    ]
                ]
                Html.ul[
                    Html.button [
                        prop.id "searchBtn"
                        prop.classes ["button";"is-ghost"]
                        prop.text "Search"
                        prop.onClick(fun ev -> OnBrandSearch |> dispatch)
                    ]
                ]
                Html.ul[
                    match model.BrandList with
                    | Ok blist ->
                        Html.div[
                            Html.select[
                                for i in [0..blist.Length-1] do
                                Html.option[
                                    prop.value i
                                    prop.text blist.[i] ] ] ]
                    | Error er-> 
                    ul [ ] [
                        li [ ] [
                            str er ] ] ] ] ]
    bListView


let renderPTablerow (prd : HKTVProductForView) = 
    Html.tr [
        //Html.td prd._id
        Html.td prd.productName
        Html.td [ 
            prop.children[
                Html.a [
                prop.text "Go To Page"
                prop.href (String.Concat("https://hktvmall.com/",prd.productUrl))]
                ]
            ]
        Html.td prd.productPrice
        Html.td prd.Sales
    ]   

let renderPTableKJrow (prd : KuaJingProductForView) = 
    Html.tr [
        //Html.td prd._id
        Html.td prd.productname
        Html.td [ 
            prop.children[
                Html.a [
                prop.text "Go To Page"
                prop.href prd.producturl]
                ]
            ]
        Html.td prd.brand
        Html.td prd.price1
        Html.td prd.amount1
    ]   

let renderPList (model : PageModel) dispatch = 
    let pListView = 
        match model.ProductList with
        | Ok plist ->
            Html.div[
                prop.className BulmaCSS.TableContainer
                prop.children[
                Html.table[
                    prop.className "table is-striped is-responsive"
                    prop.children[
                        Html.thead[
                            Html.tr[
                                //Html.th "Id"
                                Html.th "Product Name"
                                Html.th "Product Url"
                                Html.th "Product Price"
                                Html.th "Sales"] ]
                        Html.tbody[
                            for row in plist do
                            renderPTablerow row] ] ] ] ]
        | Error ex -> 
        Html.div[
            prop.className BulmaCSS.TableContainer
            prop.children[
            Html.table[
                prop.className "table is-striped is-responsive"
                prop.children[
                    Html.thead[
                        Html.tr[
                            Html.th "Product Name"
                            Html.th "Product Url"
                            Html.th "Product Price"
                            Html.th "Sales"] ] ] ] ] ] 
    pListView

let renderKJPlist (model : PageModel) dispatch = 
    let pListView =
        match model.KJProductList with
        | Ok plist ->
            Html.div[
                prop.className BulmaCSS.TableContainer
                prop.children[
                Html.table[
                    prop.className "table is-striped is-responsive"
                    prop.children[
                        Html.thead[
                            Html.tr[
                                //Html.th "Id"
                                Html.th "Product Name"
                                Html.th "Product Url"
                                Html.th "Brand"
                                Html.th "Product Price"
                                Html.th "Amount"] ]
                        Html.tbody[
                            for row in plist do
                            renderPTableKJrow row] ] ] ] ]
        | Error ex ->
            Html.div[
                prop.className BulmaCSS.TableContainer
                prop.children[
                Html.table[
                    prop.className "table is-striped is-responsive"
                    prop.children[
                        Html.thead[
                            Html.tr[
                                Html.th "Product Name"
                                Html.th "Product Url"
                                Html.th "brand"
                                Html.th "Product Price"
                                Html.th "Amount"] ] ] ] ] ] 
    pListView

let renderHKTVPListFeliz (model : PageModel) = 
    match model.ProductList with
    | Ok pList -> 
        Mui.materialTable [
            materialTable.title "HKTV Product"
            materialTable.columns [
                columns.column [
                    column.title "Product Name"
                    column.field<HKTVProductForView> (fun rd -> nameof rd.productName)
                ]
                columns.column [
                    column.title "Product Price"
                    column.field<HKTVProductForView> (fun rd -> nameof rd.productPrice)
                ]
                // columns.column [
                //     column.title "Product Page"
                //     column.field<HKTVProductForView> (fun rd -> nameof rd.productUrl)
                // ]
                columns.column [
                    column.title "Sales in Last Month"
                    column.field<HKTVProductForView> (fun rd -> nameof rd.Sales)
                ]
            ]
            
            materialTable.data[
                for i in [0..pList.Length-1] do
                pList.[i]
            ]
        ]
    | Error ex -> Html.div[]


let renderKJPListFeliz (model : PageModel) = 
    match model.KJProductList with
    | Ok pList -> 
        Mui.materialTable [
            materialTable.title "1688 Product"
            materialTable.columns [
                columns.column [
                    column.title "Product Name"
                    column.field<KuaJingProduct> (fun rd -> nameof rd.productname)
                ]
                columns.column [
                    column.title "Product Price"
                    column.field<KuaJingProduct> (fun rd -> nameof rd.price1)
                    column.type'.numeric
                ]
                columns.column [
                    column.title "Product Page"
                    column.field<KuaJingProduct> (fun rd -> nameof rd.brand)
                ]
                columns.column [
                    column.title "Product Page"
                    column.field<KuaJingProduct> (fun rd -> nameof rd.producturl)
                ]
                columns.column [
                    column.title "Amount Required"
                    column.field<KuaJingProduct> (fun rd -> nameof rd.amount1)
                ]
            ]
            
            materialTable.data[
                for i in [0..pList.Length-1] do
                pList.[i]
            ]
        ]
    | Error ex -> Html.div[]

let view (model:PageModel) dispatch = 
    Html.section [
        Html.section [
            prop.classes [ "hero"; "is-primary"]
            prop.children [
                Html.h1 [
                    prop.className "title"
                    prop.text "Home Page" ] ] ]
        Html.section [
            prop.className "hero-body"
            prop.children [
                Html.div [
                    prop.className "columns"
                    prop.children [
                        Html.div [
                            prop.classes ["column";"is-one-fifth"]
                            prop.children [
                                renderBList model dispatch
                            ]
                        ]
                        Html.div [
                            prop.classes ["columns";"is-four-fifths"]
                            prop.children [
                                Html.div[
                                    prop.classes["column";BulmaCSS.IsHalf]
                                    prop.children [
                                        //renderPList model dispatch
                                        renderHKTVPListFeliz model
                                    ]
                                ]
                                Html.div[
                                    prop.classes["column";BulmaCSS.IsHalf]
                                    prop.children[
                                        //renderKJPlist model dispatch
                                        renderKJPListFeliz model
                                    ] ] ] ] ] ] ] ] ]