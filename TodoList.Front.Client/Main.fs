module TodoList.Front.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System.Net.Http
open Model
open Newtonsoft.Json
open System.Text

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

let getTodos (http: HttpClient) =
    async {
        let url = "https://localhost:5001/api/todos"

        let! response = http.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<TodoItem list>(content)
    }

let createTodo (payload: TodoPayload) (http: HttpClient) =
    async {
        let url = "https://localhost:5001/api/todos"

        let json = JsonConvert.SerializeObject(payload)
        use content = new StringContent(json, Encoding.UTF8, "application/json")

        let! response = http.PostAsync(url, content) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<TodoItem>(content)
    }

let updateTodo (todoItem: TodoItem) (http: HttpClient) =
    async {
        let url = "https://localhost:5001/api/todos"

        let json = JsonConvert.SerializeObject(todoItem)
        use content = new StringContent(json, Encoding.UTF8, "application/json")

        let! response = http.PutAsync(url, content) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<TodoItem>(content)
    }

let deleteTodo (todoItem: TodoItem) (http: HttpClient) =
    async {
        let url = "https://localhost:5001/api/todos/" + todoItem.Id.ToString()

        let! response = http.DeleteAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<TodoItem>(content)
    }

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

type Main = Template<"wwwroot/main.html">

let todoItemView (todoItem: TodoItem) dispatch =
    Main.TodoTemplate()
        .Content(todoItem.Content, fun content -> dispatch (SetTaskContent (todoItem, content)))
        .HandleTodoBlur(fun _ -> dispatch (UpdateTask todoItem))
        .HandleRemoveTodoButtonClicked(fun _ -> dispatch (DeleteTask todoItem))
        .Elt()

let view (model: Model) dispatch =
    Main()
        .Tasks(
            forEach model.Tasks (fun t -> todoItemView t dispatch)
        )
        .NewTaskContent(model.NewTaskContent, fun content -> dispatch (SetNewTaskContent content))
        .HandleNewTodoKeyPressed(
            fun e ->
                match e.Key.ToLower() with
                | "enter" when model.NewTaskContent.Length > 0 ->
                    let payload = {
                        Content = model.NewTaskContent
                    }
                    let message = CreateNewTask payload
                    dispatch message
                | _ -> ()
        )
        .Elt()

type TodoApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let http = new HttpClient()
        let update = update http
        Program.mkProgram (fun _ -> initialModel, Cmd.ofMsg LoadTasks) update view
#if DEBUG
        |> Program.withHotReload
#endif
