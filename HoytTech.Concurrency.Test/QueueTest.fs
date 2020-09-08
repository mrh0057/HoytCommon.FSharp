module HoytTech.Concurrency.Test.QueueTest

open System.Threading
open NUnit.Framework
open HoytTech.Concurrency.Queue;

[<TestFixture>]
type QueueTestClass () =

    [<Test>]
    member this.AllFillTest() =
        let buffer = Mpmc.make 0x1000
        let rec l i =
            if i < 0x1000 then
                let result = Mpmc.offer buffer i
                Assert.IsTrue(result)
                l (i + 1)
        l 0
       
    [<Test>]
    member this.TakeTest() =
        let size = 0x10
        let buffer = Mpmc.make size
        let rec l i =
            if i < 0x10 then
                let result = Mpmc.offer buffer i
                Assert.IsTrue(result)
                l (i + 1)
        l 0
        
        let rec l i =
            if i < 0x10 then
                let result = Mpmc.poll buffer
                Assert.AreEqual(Some(i), result)
                l (i + 1)
        l 0
        
        let rec l i =
            if i < 0x10 then
                let result = Mpmc.offer buffer i
                Assert.IsTrue(result)
                l (i + 1)
        l 0
        
        let rec l i =
            if i < 0x10 then
                let result = Mpmc.poll buffer
                Assert.AreEqual(Some(i), result)
                l (i + 1)
        l 0
        
    [<Test>]
    member this.BrutualTest() =
        let buffer = Mpmc.make 0x10000
        let goThrough = 100_000
        let threadCount = 8
        let workerWriter (a: obj) =
            let rec l i =
                if i < goThrough then
                    Mpmc.offer buffer i |> ignore
                    l (i + 1)
            l 0
        let worker (a: obj) = 
             let rec l i =
                 if i < goThrough then
                     Mpmc.poll buffer |> ignore
                     l (i + 1)
             l 0
        let writeThreads = [|
            for i in 1 .. threadCount ->
                if i % 2 = 0 then 
                    Thread(ParameterizedThreadStart workerWriter)
                else
                    Thread(ParameterizedThreadStart worker)
        |]
        for thread in writeThreads do
            thread.Start() |> ignore
            
        for thread in writeThreads do
            Assert.IsTrue(thread.Join(4000))
