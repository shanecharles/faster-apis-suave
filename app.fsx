#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/fsharpx.extras/lib/40/fsharpx.extras.dll"
#r "packages/NewtonSoft.Json/lib/net45/newtonsoft.json.dll"
#load "db.fsx"

open Suave
open Suave.Operators  // Fish operator >=>
open Suave.Filters    // GET, POST, PUT, ...
open Suave.Successful
open BugDb
open BugDb.Models
open Newtonsoft.Json

let jsonMime = Writers.setMimeType "application/json"
let bugNotFound = RequestErrors.NOT_FOUND "No bug"
let ifFound = Option.map
let getOrElse = FSharpx.Option.getOrElse
let hasDetails = FSharpx.Choice.choice

type JsonBugFormat = { Id : int; Details : string; Closed : System.Nullable<System.DateTime> }
let toJbug (bug : Bug) = { Id = bug.Id; Details = bug.Details; Closed = (Option.toNullable bug.Closed) }
let serializeBugs = Seq.map toJbug >> JsonConvert.SerializeObject >> OK

let okBug b = jsonMime >=> OK (b |> toJbug |> JsonConvert.SerializeObject)
let getAllBugs = warbler (fun _ -> Db.GetAllBugs () |> serializeBugs)

let createBug = 
  request (fun r -> r.formData "details" |> hasDetails (Db.NewBug >> okBug) RequestErrors.BAD_REQUEST)

let closeBug b = Db.UpdateBug { b with Closed = Some System.DateTime.UtcNow } |> okBug

let updateBug b = 
  request (fun r -> r.formData "details" |> hasDetails (fun d -> Db.UpdateBug { b with Details = d } |> okBug) RequestErrors.BAD_REQUEST)

let app = 
  choose [ 
    GET  >=> path "/api/bugs" >=> jsonMime >=> getAllBugs 
    GET  >=> pathScan "/api/bugs/%d" (Db.GetBug >> ifFound okBug >> getOrElse bugNotFound) 
    POST >=> path "/api/bugs/create" >=> createBug 
    POST >=> pathScan "/api/bugs/%d/close" (Db.GetBug >> ifFound closeBug >> getOrElse bugNotFound) 
    POST >=> pathScan "/api/bugs/%d" (Db.GetBug >> ifFound updateBug >> getOrElse bugNotFound) ]