open System.Net

let baseUri = "http://localhost:8083"

type Response = 
  | Ok of string
  | Error of HttpStatusCode

let getResponseData (r : HttpWebResponse) = 
  use stream = r.GetResponseStream ()
  use reader = new System.IO.StreamReader(stream)
  reader.ReadToEnd ()

let getReq (url : string) = 
  let wr = HttpWebRequest.Create(url)
  use resp = wr.GetResponse () :?> HttpWebResponse
  match resp.StatusCode with 
  | HttpStatusCode.OK -> resp |> getResponseData |> Ok
  | err               -> Error err

let postReq (url : string) (data : string) = 
  let wr = HttpWebRequest.Create(url)
  wr.Method <- "POST"
  wr.ContentLength <- int64 data.Length
  wr.ContentType <- "application/x-www-form-urlencoded"
  let reqstr = wr.GetRequestStream ()
  if data.Length > 0 then
    let byteArray = System.Text.Encoding.UTF8.GetBytes (data)
    reqstr.Write (byteArray, 0, byteArray.Length)
  reqstr.Close()

  use resp = wr.GetResponse () :?> HttpWebResponse
  match resp.StatusCode with
  | HttpStatusCode.OK -> resp |> getResponseData |> Ok
  | err               -> Error err

let details = sprintf "details=%s"

let getAll () = getReq (sprintf "%s/api/bugs" baseUri)
let create d = postReq (baseUri + "/api/bugs/create") (details d)
let close id = postReq (sprintf "%s/api/bugs/%d/close" baseUri id) ""
let update id d = postReq (sprintf "%s/api/bugs/%d" baseUri id) (details d)
let get id = getReq (sprintf "%s/api/bugs/%d" baseUri id)
let getStatus s = getReq (sprintf "%s/api/bugs/%s" baseUri s)