namespace TodoList.Core.Business

open Model
open TodoApi
open System.Net.Http
open Elmish

module Update =

    /// The Elmish application's model.
    type Model = {
        Tasks: TodoItem list;
        NewTaskContent: string;
        Error: string option;
    }
    
    let initialModel =
        {
            Tasks = []
            NewTaskContent = ""
            Error = None
        }
    
    /// The Elmish application's update messages.
    type Message =
        | LoadTasks
        | TaskLoaded of TodoItem list
        | CreateNewTask of TodoPayload
        | TaskCreated of TodoItem
        | UpdateTask of TodoItem
        | TaskUpdated of TodoItem
        | DeleteTask of TodoItem
        | TaskDeleted of TodoItem
        | SetTaskContent of TodoItem * string
        | SetNewTaskContent of string
        | Error of exn
        
    let update (http: HttpClient) message model =
        match message with
        | LoadTasks ->
            let cmd = Cmd.ofAsync getTodos http TaskLoaded Error
            model, cmd
    
        | TaskLoaded tasks ->
            { model with Tasks = tasks |> List.sortBy (fun t -> t.Id) }, Cmd.none
            
        | CreateNewTask payload ->
            let cmd = Cmd.ofAsync (createTodo payload) http TaskCreated Error
            { model with NewTaskContent = "" }, cmd
            
        | TaskCreated task ->
            let newTasks = 
                model.Tasks @ [ task ]
                |> List.sortBy (fun t -> t.Id)
            
            { model with Tasks = newTasks }, Cmd.none
            
        | UpdateTask payload ->
            let cmd = Cmd.ofAsync (updateTodo payload) http TaskUpdated Error
            model, cmd
                
        | TaskUpdated task ->
            let oldTasks = 
                model.Tasks 
                |> List.filter (fun t -> t.Id <> task.Id)
               
            let newTasks = 
                oldTasks @ [ task ]
                |> List.sortBy (fun t -> t.Id)
                
            { model with Tasks = newTasks }, Cmd.none
    
        | DeleteTask task ->
            let cmd = Cmd.ofAsync (deleteTodo task) http TaskDeleted Error
            model, cmd
            
        | TaskDeleted task ->
            let newTasks = 
                model.Tasks 
                |> List.filter (fun t -> t.Id <> task.Id)
                |> List.sortBy (fun t -> t.Id)
    
            { model with Tasks = newTasks }, Cmd.none
    
        | SetTaskContent (task, content) ->
            let newTasks = 
                model.Tasks 
                |> List.map 
                    (fun t ->
                        if (t.Id = task.Id) then
                            { t with Content = content }
                        else
                            t
                    )
                |> List.sortBy (fun t -> t.Id)
    
            { model with Tasks = newTasks }, Cmd.none
    
        | SetNewTaskContent content ->
            { model with NewTaskContent = content }, Cmd.none
    
        | Error exn ->
            { model with Error = Some exn.Message }, Cmd.none

    let init = (fun _ -> initialModel, Cmd.ofMsg LoadTasks)