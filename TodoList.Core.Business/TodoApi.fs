namespace TodoList.Core.Business

open Newtonsoft.Json
open Model
open System.Net.Http
open System.Text

module TodoApi =

    let useLocalUrl = false
    
    let localUrl = "https://localhost:5001/api"
    let distantUrl = "https://todolistwebapifsharp.azurewebsites.net/api"
    let baseUrl = if useLocalUrl then localUrl else distantUrl

    let getTodos (http: HttpClient) =
        async {
            let url = baseUrl + "/todos"
   
            let! response = http.GetAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
   
            return JsonConvert.DeserializeObject<TodoItem list>(content)
        }

    let createTodo (payload: TodoPayload) (http: HttpClient) =
        async {
            let url = baseUrl + "/todos"
   
            let json = JsonConvert.SerializeObject(payload)
            use content = new StringContent(json, Encoding.UTF8, "application/json")
   
            let! response = http.PostAsync(url, content) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
   
            return JsonConvert.DeserializeObject<TodoItem>(content)
        }
   
    let updateTodo (todoItem: TodoItem) (http: HttpClient) =
        async {
            let url = baseUrl + "/todos"
   
            let json = JsonConvert.SerializeObject(todoItem)
            use content = new StringContent(json, Encoding.UTF8, "application/json")
   
            let! response = http.PutAsync(url, content) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
   
            return JsonConvert.DeserializeObject<TodoItem>(content)
        }
   
    let deleteTodo (todoItem: TodoItem) (http: HttpClient) =
        async {
            let url = baseUrl + "/todos/" + todoItem.Id.ToString()
   
            let! response = http.DeleteAsync(url) |> Async.AwaitTask
            response.EnsureSuccessStatusCode () |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
   
            return JsonConvert.DeserializeObject<TodoItem>(content)
        }