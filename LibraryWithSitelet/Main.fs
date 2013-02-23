namespace LibraryWithSitelet

open System
open System.IO
open System.Web
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Formlet
open PerfectShuffle.WebsharperExtensions

type NestedAction =
| Foo
| Bar of string
| Ignore // No page here, but we have to have a bijection...

module NestedSite =

  let sitelet (actionMap : NestedAction -> 'a) (templateWrapper:(Context<'a> -> Content.HtmlElement list) -> Content<NestedAction>) =
  
    let fooContent (ctx:Context<_>) =
      [
        Div [Text "Foo page"]
      ]
    
    let barContent (ctx:Context<_>) =
      [
        Div [Text "Bar page"]
      ]
    
    let sitelet =
     Sitelet.Infer <| fun action ->
     match action with
     | Foo -> templateWrapper fooContent
     | Bar x -> templateWrapper barContent
     | Ignore -> failwith "Should never end up here"

    sitelet |> Sitelet.FilterAction (function Ignore -> false | _ -> true)
