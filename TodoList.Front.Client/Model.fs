module Model

open Newtonsoft.Json

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