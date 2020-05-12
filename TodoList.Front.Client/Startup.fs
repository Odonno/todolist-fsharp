namespace TodoList.Front.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.TodoApp>("#main")
        builder.Build().RunAsync() |> ignore
        0
