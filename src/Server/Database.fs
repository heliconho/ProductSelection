module Database

open MongoDB.Driver
open MongoDB.Bson
open MongoDB.Bson.IO
open Shared
open Models

module Mongo =
    [<Literal>]
    let ConnectionString = "mongodb+srv://dbAdmin:HardcodePassword@cluster0.xenqv.mongodb.net/test"
    [<Literal>]
    let DbName = "FsPs"
    [<Literal>]
    let HKTVCollection = "HKTVProducts"
    [<Literal>]
    let KuaJingCollection = "KuaJingProduct"
    //fssnip
    let readAll collectionName (filter : string option) =
        let client = MongoClient(ConnectionString)
        let db = DbName |> client.GetDatabase
        let collection = collectionName |> db.GetCollection<BsonDocument>
        let cursor : IAsyncCursor<BsonDocument> =
            collection.FindSync(
                filter
                |> Option.map (BsonDocument.Parse >> FilterDefinition.op_Implicit)
                |> Option.defaultValue FilterDefinition.Empty)
        let settings = JsonWriterSettings()
        settings.OutputMode <- JsonOutputMode.CanonicalExtendedJson
        let rec readDoc docs =
            if cursor.MoveNext() |> not then
                docs
            else
                let batch = cursor.Current
                batch
                |> Seq.toArray
                |> Array.map (fun d -> d.ToJson(settings))
                |> Array.append docs
                |> readDoc
        readDoc [|  |]

    [<AutoOpen>]
    module Prelude =
        let ($) f a = f a
        let konst a _ = a
    module Option =
        open System
        let fromNullable (x : 'a) : Option<'a> = 
            if Object.ReferenceEquals(x, null) 
                then None
                else Some x

    module connect = 
        let client         = MongoClient(ConnectionString)
        let db             = client.GetDatabase(DbName)
        let hktvCollection = db.GetCollection<HKTVProduct>(HKTVCollection)
        let kuajingCollection = db.GetCollection<KuaJingProduct>(KuaJingCollection)

    module Create = 
        // Single Creation
        let create ( product : 'a ) (dbCollection : IMongoCollection<'a>) = dbCollection.InsertOne( product )
        // Multiple Creation
        let createMany ( productList : 'a list ) (dbCollection : IMongoCollection<'a>)= dbCollection.InsertManyAsync( productList )

    module Read = 
        // Read Based On Id 
        let readHKTVOnId ( id : ObjectId ) = connect.hktvCollection.Find( fun x -> x._id = id ).ToEnumerable() 
        let readKJOnId (id : ObjectId) = connect.kuajingCollection.Find(fun x -> x._id = id).ToEnumerable()
        // Read Based On Name 
        let readOnHKTVName ( pName : string ) = connect.hktvCollection.Find( fun x -> x.productName = pName ).ToEnumerable() 
        let readOnKJName ( pName : string ) = connect.kuajingCollection.Find( fun x -> x.productname = pName ).ToEnumerable()
        // Read All
        let readHKTVAll() = Option.fromNullable $ connect.hktvCollection.Find(Builders.Filter.Empty).ToEnumerable()
        let readKJAll() = Option.fromNullable $ connect.kuajingCollection.Find(Builders.Filter.Empty).ToEnumerable()