namespace TodoList.Core.Business

open Newtonsoft.Json

module Model =

    type TodoItem = {
        [<JsonProperty("id")>]
        Id: int;

        [<JsonProperty("content")>]
        Content: string;
    }

    type TodoPayload = {
        [<JsonProperty("content")>]
        Content: string;
    }