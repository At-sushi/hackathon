// F# の詳細については、http://fsharp.net を参照してください
// 詳細については、'F# チュートリアル' プロジェクトを参照してください。

// adjust these as needed for your latest installed version of ManagedDirectX
 
open System
open System.Drawing
open System.Windows.Forms
open Microsoft.FSharp.Control.CommonExtensions
open DxLibDLL

let maxTiles = 8
let tileSize = 32

// ループのテスト。後ろのグリッド部分を描画。
let testloop() = 
    for i in [0..maxTiles] do
        let p1 = (i * tileSize, 0)
        let p2 = (fst p1 + 4, maxTiles * tileSize)
        ignore(DX.DrawBox(fst p1, snd p1, fst p2, snd p2, 0xCCCCCC, 1),
               DX.DrawBox(snd p1, fst p1, snd p2, fst p2, 0xCCCCCC, 1))
    done

// 点を無限に増やして描画
let rec drawPoints points drawfunc =
    let nextPoints =
        (List.filter (fun i -> 0 <= fst i && fst i <= maxTiles && 0 <= snd i && snd i <= maxTiles)
        << List.collect(fun (x, y) -> [(x-1, y); (x+1, y); (x, y-1); (x, y+1)])) points
    drawfunc points
    drawPoints nextPoints drawfunc

// 点を描画するための高階関数
let DrawPointsFunc points =
    for i in points do
        ignore(DX.DrawCircle (fst i * tileSize, snd i * tileSize, 6, 0xFF0000))
    done
    ignore(DX.ScreenFlip())
    System.Threading.Thread.Sleep (1000)

// DXLib初期化
let InitDxLib (form:Form) = 
    ignore(DX.SetUserWindow(form.Handle))
    let result = DX.DxLib_Init()
    ignore(DX.SetDrawScreen(DX.DX_SCREEN_BACK))
    result

[<EntryPoint>]
let main argv = 
    let hoge = 12
    let form = new Form()

    let GameMainThread() =
        let points = [(4, 2)]

        while form.IsDisposed = false do
            ignore(DX.ClearDrawScreen())
            drawPoints points (fun points ->
                    ignore(DX.ClearDrawScreen())
                    testloop()
                    DrawPointsFunc points
                    )
            testloop()
            ignore(DX.ScreenFlip())
            System.Threading.Thread.Sleep (1000 / 60)
        done

    let gameThread = System.Threading.Thread(new Threading.ThreadStart(GameMainThread))
    
    form.Width <- 640
    form.Height <- 480
    form.FormBorderStyle <- FormBorderStyle.Fixed3D

    ignore(InitDxLib form)
    form.Show()
    gameThread.Start()

    Application.Run form

    gameThread.Join()
    DX.DxLib_End()
