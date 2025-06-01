module CleanupUserProfile.Tests.PhotoTimestamperTests

open System
open CleanupUserProfile
open NUnit.Framework

[<Test>]
[<TestCase("IMG_20190222_211533_Bokeh.jpg", "2019-02-22 21:15:33", "20190222_211533")>]
[<TestCase("2019-02-13_17-55-11_IMG-EFFECTS.jpg", "2019-02-13 17:55:11", "2019-02-13_17-55-11")>]
[<TestCase("Jipai-20190722-21.54.51.mp4", "2019-07-22 21:54:51", "20190722-21.54.51")>]
let Test1 fileName expected substitute =

    let expected = DateTime.ParseExact(expected, "yyyy-MM-dd HH:mm:ss", null)

    let result = PhotoTimestamper.tryGetFromFileName fileName
    Assert.That(result, Is.EqualTo(PhotoTimestamper.Result.Substitute(expected, substitute)))
