module FCSParallel.MSBuild

open System.IO
open System.Runtime.CompilerServices
open FSharp.Compiler.Benchmarks
open HistoricalBenchmark
open Microsoft.Build.Definition
open Microsoft.Build.Evaluation

[<MethodImpl(MethodImplOptions.NoInlining ||| MethodImplOptions.NoOptimization)>]
let Bar () : unit =
    let baseDir = "d:/projekty/parallel_test"
    let topPath = Path.Combine(baseDir, "top.fsproj")
    let n = 50
    
    let top = Microsoft.Build.Evaluation.Project.FromFile(topPath, ProjectOptions())
    top.RemoveItems(top.GetItems("ProjectReference")) |> ignore
    Array.init n id
    |> Array.iter (fun i ->
        let name = $"leaf_{i}"
        let files = [
            "Library.fs", $"{name}.fs"
            "leaf.fsproj", $"{name}.fsproj"
        ]
        let dir = baseDir
        Directory.CreateDirectory(dir) |> ignore
        files
        |> List.iter (fun (src, tgt) ->
            let combine = Path.Combine(dir, tgt)
            if File.Exists(combine) then File.Delete(combine)
            File.Copy(Path.Combine(dir, src), combine)
            File.WriteAllText(combine, File.ReadAllText(combine).Replace("module Library", $"module {name}"))
        )
        top.AddItem("ProjectReference", Path.Combine(dir, $"{name}.fsproj")) |> ignore
    )
    top.Save()
    
    // let c =
    //     SingleFileCompilerWithILCacheClearing(
    //         Path.Combine(__SOURCE_DIRECTORY__, topPath),
    //         OptionsCreationMethod.FromScript
    //     )
    // ()

[<MethodImpl(MethodImplOptions.NoInlining ||| MethodImplOptions.NoOptimization)>]
let Foo () : unit =
    Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults() |> ignore

let main () =
    Foo()
    Bar()
    ()