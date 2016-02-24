#r "packages/Suave/lib/net40/Suave.dll"
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

type JsonBugFormat = { Id : int; Details : string; Closed : System.Nullable<System.DateTime> }
let toJbug (bug : Bug) = { Id = bug.Id; Details = bug.Details; Closed = (Option.toNullable bug.Closed) }
let serializeBugs = Seq.map toJbug >> JsonConvert.SerializeObject >> OK

let getAllBugs = warbler (fun _ -> Db.GetAllBugs () |> serializeBugs)

let app = GET >=> path "/api/bugs" >=> jsonMime >=> getAllBugs