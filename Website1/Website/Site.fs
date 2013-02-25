namespace Website

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets
open PerfectShuffle.WebsharperExtensions

open LibraryWithSitelet

type Action =
    | Home
    | About
    | Nested of NestedAction

module UrlHelpers =
    let ( => ) text url =
        A [HRef url] -< [Text text]

module Skin =
    open System.Web
    open UrlHelpers

    let TemplateLoadFrequency =
        #if DEBUG
        Content.Template.PerRequest
        #else
        Content.Template.Once
        #endif

    type Page =
        {
            Title : string
            Body : list<Content.HtmlElement>
            Navigation : list<Content.HtmlElement>
        }

    let MainTemplate =
        let path = HttpContext.Current.Server.MapPath("~/Main.html")
        Content.Template<Page>(path, TemplateLoadFrequency)
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)
            .With("navigation", fun x -> x.Navigation)

    let WithTemplate title body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Title = title
                Body = body context
                Navigation =
                  [
                    H2 [Text "Navigation Menu"]
                    LI ["Home" => context.Link Home] // Fails when browsing nested sitelet
                    LI ["Foo" => context.Link (Nested(NestedAction.Foo))]
                  ]
            }

module Site =
    open UrlHelpers

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
            LI ["Foo" => ctx.Link (Nested(NestedAction.Foo))]
            LI ["Bar" => ctx.Link (Nested(NestedAction.Bar("abc123")))]
        ]

    let HomePage =
        Skin.WithTemplate "HomePage" <| fun ctx ->
            [
                Div [Text "HOME"]
                Links ctx
            ]

    let AboutPage =
        Skin.WithTemplate "AboutPage" <| fun ctx ->
            [
                Div [Text "ABOUT"]
                Links ctx
            ]

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
            Sitelet.Content "/About" About AboutPage
            NestedSite.sitelet
                <| function Nested n -> n | _ -> failwith "Can't happen: all cases already treated above"
                <| fun f -> Skin.WithTemplate "Home" (f << Context.map Nested)
        ]

type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; About]

[<assembly: WebsiteAttribute(typeof<Website>)>]
do ()
