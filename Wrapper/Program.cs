using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EOSDigital.API;
using EOSDigital.SDK;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});


CanonAPI? APIHandler = null;
Camera? MainCamera = null;
string? ImageSaveDirectory = null;
ManualResetEvent WaitEvent = new ManualResetEvent(false);
List<Camera>? cameras = null;
string defaultErrorMessage = "error";
string noError = "function is safe";


app.MapGet("/getcanonapi", () =>
{
    string message = "no camera detected";
    int cameraCount = 0;

    try
    {
        APIHandler = new CanonAPI();
        List<Camera> cameras = APIHandler.GetCameraList();

        void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            try
            {
                Console.WriteLine("Starting image download...");
                sender.DownloadFile(Info, ImageSaveDirectory);
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            finally { WaitEvent.Set(); }
        }

        if (cameras.Count > 0)
        {
            // set dir
            ImageSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
            // open session
            cameraCount = cameras.Count;
            MainCamera = cameras.First();
            MainCamera.DownloadReady += MainCamera_DownloadReady;
            MainCamera.OpenSession();
            message = "Open Session with Camera: " + MainCamera.DeviceName;
            
            MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
            MainCamera.SetCapacity(4096, int.MaxValue);

            // take picture
            CameraValue tv = TvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Tv));
            if (tv == TvValues.Bulb) MainCamera.TakePhotoBulb(2);
            else MainCamera.TakePhoto();
            WaitEvent.WaitOne();
        }
        else
        {
        }

    }
    catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
    finally
    {
        MainCamera?.Dispose();
        APIHandler?.Dispose();
    }


    return new { cameraCount = cameraCount, message = message, capturedDir = $"picture captured at {ImageSaveDirectory}" };
});

// this Api gets a list of cameras connection 
app.MapGet("/getcameralist", () =>
{
    string message = "no camera detected";
    string[]? cameraNames = null;

    try
    {
        APIHandler = new CanonAPI();
        cameras = APIHandler.GetCameraList();
        message = cameras.Count == 0 ? message : $"There is {cameras.Count} camera(s) detected";
        cameraNames = cameras.Select(camera => camera.DeviceName).ToArray();

    }
    catch { }
    finally
    {
        APIHandler?.Dispose();
    }

    return new { cameras = cameraNames, message };

});

// connect session with the selected camera
app.MapGet("/connectcamera", () => 
{
    string errorMessage = defaultErrorMessage;
    

});


app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
