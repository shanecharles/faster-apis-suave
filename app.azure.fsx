// --------------------------------------------------------------------------------------
// Start the 'app' WebPart defined in 'app.fsx' on Azure using %HTTP_PLATFORM_PORT%
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
#load "app.fsx"
open App
open Fake
open System
open Suave

let serverConfig =
  let port = uint16 (getBuildParam "port")

  { Web.defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
      bindings = [ Suave.Http.HttpBinding.mk HTTP (Net.IPAddress.Parse("127.0.0.1")) port ] }

Web.startWebServer serverConfig app