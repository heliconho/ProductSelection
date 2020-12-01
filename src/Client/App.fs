module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Feliz
open Browser.Dom
open Index
#if DEBUG
open Elmish.Debug
open Elmish.HMR

#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
