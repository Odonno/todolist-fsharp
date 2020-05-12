module TodoList.Front.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Templating.Client
open System.Net.Http
open TodoList.Core.Business.Model
open TodoList.Core.Business.Update

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

        Program.mkProgram init update view
#if DEBUG
        |> Program.withHotReload
#endif
