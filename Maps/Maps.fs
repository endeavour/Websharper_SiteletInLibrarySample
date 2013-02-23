namespace PerfectShuffle.WebsharperExtensions
open IntelliFactory.WebSharper.Sitelets

module Context =
  let map (context:Context<'a>) (inverseActionMap:'b -> 'a) : Context<'b> =
    let newContext:Context<'b> =
      {
        ApplicationPath = context.ApplicationPath
        Link = inverseActionMap >> context.Link
        Json = context.Json
        Metadata = context.Metadata
        ResolveUrl = context.ResolveUrl
        ResourceContext = context.ResourceContext
        Request = context.Request
      }
    newContext

module Content =
   let map (inverseActionMap:'b -> 'a) (content:Content<'b>) : Content<'a> =
     match content with
     | CustomContent f -> CustomContent (fun context -> f (Context.map context inverseActionMap))
     | PageContent f -> PageContent (fun context -> f (Context.map context inverseActionMap))

module Sitelet =
  let FilterAction (ok: 'T -> bool) (sitelet: Sitelet<'T>) =
    let route req =
        match sitelet.Router.Route(req) with
        | Some x when ok x -> Some x
        | _ -> None
    let link action =
        if ok action then
            sitelet.Router.Link(action)
        else None
    { sitelet with Router = Router.New route link }