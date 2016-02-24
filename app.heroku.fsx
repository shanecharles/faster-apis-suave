// --------------------------------------------------------------------------------------
// // Start the 'app' WebPart defined in 'app.fsx' on Heroku using %PORT%
// // --------------------------------------------------------------------------------------
//
#load "app.fsx"
open App
open System
open Suave

let serverConfig =
  let port = uint16 (Environment.GetEnvironmentVariable("PORT"))
  { Web.defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
      bindings = [ HttpBinding.mk HTTP (Net.IPAddress.Parse("0.0.0.0")) port ] }

Web.startWebServer serverConfig app
