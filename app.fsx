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
let hasData = FSharpx.Choice.choice
let noFilter = FSharpx.Functional.Prelude.konst ""

type JsonBugFormat = { Id : int; Details : string; Closed : System.Nullable<System.DateTime> }
let toJbug (bug : Bug) = { Id = bug.Id; Details = bug.Details; Closed = (Option.toNullable bug.Closed) }
let serializeBugs = Seq.map toJbug >> JsonConvert.SerializeObject >> OK
let okBug b = jsonMime >=> OK (b |> toJbug |> JsonConvert.SerializeObject)

let createBug = 
  request (fun r -> r.formData "details" |> hasData (Db.NewBug >> okBug) RequestErrors.BAD_REQUEST)

let closeBug b = Db.UpdateBug { b with Closed = Some System.DateTime.UtcNow } |> okBug
let updateBug b = 
  request (fun r -> r.formData "details" |> hasData (fun d -> Db.UpdateBug { b with Details = d } |> okBug) RequestErrors.BAD_REQUEST)

let getOrUpdate b = choose [ GET  >=> okBug b
                             POST >=> updateBug b]
let filterStatus = 
  let getBugsByStatus = function 
    | "open"   -> Db.GetOpenBugs ()
    | "closed" -> Db.GetClosedBugs ()
    | _        -> Db.GetAllBugs ()
  request (fun r -> r.queryParam "filter" 
                    |> hasData id noFilter
                    |> getBugsByStatus
                    |> serializeBugs)
let app = 
  choose [ 
    GET  >=> path "/" >=> OK "<html><head><title>Simple Bugs</title></head><body><h2>Simple (not for production) Bug API in Suave</h2></body></html>"
    GET  >=> path "/api/bugs" >=> jsonMime >=> filterStatus
    pathScan "/api/bugs/%d" (Db.GetBug >> ifFound getOrUpdate >> getOrElse bugNotFound) 
    POST >=> path "/api/bugs/create" >=> createBug 
    POST >=> pathScan "/api/bugs/%d/close" (Db.GetBug >> ifFound closeBug >> getOrElse bugNotFound) 
    RequestErrors.NOT_FOUND "Resource could not be found" ]