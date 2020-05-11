module Handlers

open System
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open Giraffe
open Model
open Data

let handleGetTodos =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = getTodos()
            return! json response next ctx
        }

let handleGetTodo (id: int) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let todoItem = getTodo id

            match todoItem with
            | Some response ->
                return! json response next ctx
            | None ->
                return! (setStatusCode 404 >=> text "No todo item found") next ctx
        }
        
let handleCreateTodo =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! payload = ctx.BindJsonAsync<TodoPayload>()

            let response = createTodo payload
            return! json response next ctx
        }

let handleUpdateTodo =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! payload = ctx.BindJsonAsync<TodoItem>()

            let todoItem = updateTodo payload
            
            match todoItem with
            | Some response ->
                return! json response next ctx
            | None ->
                return! (setStatusCode 404 >=> text "No todo item found") next ctx
        }

let handleDeleteTodo (id: int) =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let todoItem = deleteTodo id

            match todoItem with
            | Some response ->
                return! json response next ctx
            | None ->
                return! (setStatusCode 404 >=> text "No todo item found") next ctx
        }