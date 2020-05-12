// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace TodoList.Mobile

#if APPSAVE
open System.Diagnostics
#endif
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open System.Net.Http
open TodoList.Core.Business.Model
open TodoList.Core.Business.Update

module App = 

    let daveoImage = 
        ImagePath("https://image.slidesharecdn.com/agencedaveopresentation2-181113135323/95/mini-slideshow-daveo-quelques-photos-de-lagence-1-638.jpg")

    let todoItemView (todoItem: TodoItem) dispatch =
        View.StackLayout(
            orientation = StackOrientation.Horizontal,
            horizontalOptions = LayoutOptions.CenterAndExpand,
            children = [
                View.Entry(
                    text = todoItem.Content,
                    width = 150.,
                    unfocused = (fun _ -> dispatch (UpdateTask todoItem))
                )
                View.Button(
                    text = "Delete", 
                    command = (fun () -> dispatch (DeleteTask todoItem)), 
                    horizontalOptions = LayoutOptions.Center
                )
            ]  
        )
        
    let view (model: Model) dispatch =
        View.ContentPage(
            backgroundColor = Color.White,
            content = View.StackLayout(
                padding = Thickness 20.0, 
                verticalOptions = LayoutOptions.Start,
                children = [ 
                    View.Image(source = daveoImage)
                    View.ScrollView(
                        View.StackLayout(
                            children = (
                                model.Tasks 
                                |> List.map (fun t -> todoItemView t dispatch)
                            )
                        )
                    )
                    View.Entry(
                        text = model.NewTaskContent, 
                        placeholder = "New things to do?", 
                        margin = Thickness(0., 40., 0., 0.),
                        textChanged =
                            (fun args -> dispatch (SetNewTaskContent args.NewTextValue)),
                        completed = 
                            (fun _ ->
                                if model.NewTaskContent.Length > 0 then
                                    let payload = {
                                        Content = model.NewTaskContent
                                    }
                                    let message = CreateNewTask payload
                                    dispatch message
                                else
                                    ()
                            )
                    )
                ])
        )

    let http = new HttpClient()
    let update = update http

    let program = XamarinFormsProgram.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    
    do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


