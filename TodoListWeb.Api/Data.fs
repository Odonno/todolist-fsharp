module Data

open Model

let mutable private nextId = 1
let mutable private todos: TodoItem list = []

let getTodos () =
    todos

let getTodo id =
    todos |> List.tryFind (fun t -> t.Id = id)

let createTodo payload =
    let todo = {
        Id = nextId
        Content = payload.Content
    }

    nextId <- nextId + 1
    todos <- todos @ [todo]

    todo

let updateTodo payload = 
    todos <-
        todos
        |> List.filter (fun t -> t.Id <> payload.Id)
        |> List.append [payload]

    getTodo payload.Id

let deleteTodo id =
    let removedTodo = getTodo id

    todos <-
        todos
        |> List.filter (fun t -> t.Id <> id)
        
    removedTodo