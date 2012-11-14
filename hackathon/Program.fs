// F# の詳細については、http://fsharp.net を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。

// adjust these as needed for your latest installed version of ManagedDirectX
 
open System
open System.Drawing
open System.Threading
open System.Windows.Forms
open Microsoft.FSharp.Control.CommonExtensions
open DxLibDLL

let maxTiles = 8
let tileSize = 32

// ループのテスト。後ろのグリッド部分を描画。
let testloop() = 
    for i in [0..maxTiles] do
        let p1 = (i * tileSize, 0)
        let p2 = (fst p1 + 4, maxTiles * tileSize + 4)
        ignore(DX.DrawBox(fst p1, snd p1, fst p2, snd p2, 0xCCCCCC, 1),
               DX.DrawBox(snd p1, fst p1, snd p2, fst p2, 0xCCCCCC, 1))
    done

let rec testloop2 count x drawMethod =
    let x2 =
        match Control.ModifierKeys with
        | Keys.Left -> x - 2
        | Keys.Right -> x + 2
        | _ -> x
    if (Control.MouseButtons.HasFlag MouseButtons.Left <> false) then do 
        drawMethod x
        testloop2 (count + 1) x2 drawMethod



let testDrawFunc x =
    ignore <| DX.DrawCircle (x + 12, 150, 6, 0x22FF0000)
    ignore <| DX.ScreenFlip()
    Thread.Sleep (1000 / 60)

// ライフゲームの次の世代を計算
let nextGen points =
     [
         for i in [0..maxTiles] do for j in [0..maxTiles] do
             let nearby = List.filter(fun (x, y) -> abs(i - x) <= 1 && abs(j - y) <= 1) points
             if (match (List.tryFind <| (=)(i, j) <| nearby) with
             | Some(_) -> nearby.Length - 1 = 3 || nearby.Length - 1 = 2
             | None -> nearby.Length = 3) then yield (i, j)
     ]

// 点を無限に増やして描画
let rec drawPoints points drawfunc =
    let nextPoints = lazy
        List.filter (fun (x, y) -> 0 <= x && x <= maxTiles && 0 <= y && y <= maxTiles)
        << List.collect(fun (x, y) -> [(x-1, y); (x+1, y); (x, y-1); (x, y+1)]) <| points
    drawfunc points
    if (points.Length < 32) then do
        drawPoints <| nextPoints.Force() <| drawfunc
        drawfunc points

// ライフゲーム点
let rec drawlgPoints (form:Form) points drawfunc =
    let nextPoints = lazy nextGen points
    drawfunc points
    if (form.IsDisposed = false) then drawlgPoints form <| nextPoints.Force() <| drawfunc

// 点を描画するための関数
let DrawPointsFunc points =
    for i in points do
        ignore <| DX.DrawCircle (fst i * tileSize, snd i * tileSize, 6, 0x22FF0000)
    done
    ignore <| DX.ScreenFlip()
    Thread.Sleep (1000 / 3)

// 関数を指定してDXLibを呼び出す
let UseDxLibIn (form:Form) targetFunc = 
    ignore(DX.SetUserWindow(form.Handle),
           DX.SetMultiThreadFlag(DX.TRUE))
    let result = DX.DxLib_Init() in
    if result = -1 then raise <| Runtime.InteropServices.ExternalException("DXLib initialize failed.")
    ignore(DX.SetDrawScreen(DX.DX_SCREEN_BACK),
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 22))

    targetFunc()        // DXLibを使った処理実行

    DX.DxLib_End()

// ゲームループとか
let GameMainThread (form:Form) =
    let points = [(4, 3);(5,3);(4,2);(6,3);(5,1)]
    let graphBG = DX.LoadGraph "testgrf.png"

    testloop2 0 0 testDrawFunc

    while form.IsDisposed = false do
        ignore <| DX.ClearDrawScreen()
        drawlgPoints form points <| fun points ->
                ignore(DX.ClearDrawScreen(),
                        DX.DrawGraph(400, 480 - 400, graphBG, DX.TRUE))
                testloop()
                DrawPointsFunc points
        testloop()
        ignore <| DX.ScreenFlip()
        Thread.Sleep (1000 / 60)
    done

[<EntryPoint>]
let main argv = 
    let hoge = 12
    let form = new Form(Width = 640,
                        Height = 480,
                        FormBorderStyle = FormBorderStyle.Fixed3D)

    let gameThread = Thread(new ThreadStart(fun () -> GameMainThread form))
    form.Show()
    UseDxLibIn form <| fun () ->
        gameThread.Start()
        Application.Run form
        gameThread.Join()

