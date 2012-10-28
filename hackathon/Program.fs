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
        match DX.CheckHitKeyAll() with
        | DX.KEY_INPUT_LEFT -> x - 2
        | DX.KEY_INPUT_RIGHT -> x + 2
        | _ -> x
    if (Control.MouseButtons.HasFlag MouseButtons.Left <> false) then do 
//        drawMethod()
        Thread.Sleep 100
        testloop2 (count + 1) x2 drawMethod



// 点を描画するための高階関数
let testDrawFunc() =
    ignore(DX.ScreenFlip())
    Thread.Sleep (1000 / 60)

//let nextGen (points:((int * int) List)) =
//    seq { for i in [0..maxTiles] -> for j in [0..maxTiles]  do List.filter((List.filter(fun (x, y) -> abs(i - x) <= 1 && abs(j + y) <= 1) points).Length >= 3) [(0,0)..(maxTiles,maxTiles)] done
//        }

// 点を無限に増やして描画
let rec drawPoints points drawfunc =
    let nextPoints = lazy
        (List.filter (fun i -> 0 <= fst i && fst i <= maxTiles && 0 <= snd i && snd i <= maxTiles)
        << List.collect(fun (x, y) -> [(x-1, y); (x+1, y); (x, y-1); (x, y+1)])) points
    drawfunc points
    if (points.Length < 32) then drawPoints (nextPoints.Force()) drawfunc
    drawfunc points

// 点を描画するための高階関数
let DrawPointsFunc points =
    for i in points do
        ignore(DX.DrawCircle (fst i * tileSize, snd i * tileSize, 6, 0x22FF0000))
    done
    ignore(DX.ScreenFlip())
    Thread.Sleep (1000 / 12)

// 関数を指定してDXLibを呼び出す
let UseDxLib (form:Form) targetFunc = 
    ignore(DX.SetUserWindow(form.Handle))
    let result = DX.DxLib_Init()
    if result = -1 then raise (Runtime.InteropServices.ExternalException("DXLib initialize failed."))
    ignore(DX.SetDrawScreen(DX.DX_SCREEN_BACK),
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 22))

    targetFunc()        // DXLibを使った処理実行

    DX.DxLib_End()

[<EntryPoint>]
let main argv = 
    let hoge = 12
    let form = new Form()

    // ゲームループとか
    let GameMainThread() =
        let points = [(4, 3)]
        let graphBG = DX.LoadGraph "testgrf.png"

        testloop2 0 0 testDrawFunc

        while form.IsDisposed = false do
            ignore(DX.ClearDrawScreen())
            drawPoints points (fun points ->
                    ignore(DX.ClearDrawScreen(),
                            DX.DrawGraph(400, 480 - 400, graphBG, DX.TRUE))
                    testloop()
                    DrawPointsFunc points
                    )
            testloop()
            ignore(DX.ScreenFlip())
            Thread.Sleep (1000 / 60)
        done

    let gameThread = Thread(new ThreadStart(GameMainThread))
    
    form.Width <- 640
    form.Height <- 480
    form.FormBorderStyle <- FormBorderStyle.Fixed3D
    form.Show()

    UseDxLib form (fun () ->
        gameThread.Start()
        Application.Run form
        gameThread.Join())

