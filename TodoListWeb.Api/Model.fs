module Model

type TodoItem = {
    Id: int;
    Content: string;
}

type TodoPayload = {
    Content: string;
}