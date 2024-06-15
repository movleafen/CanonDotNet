using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EOSDigital.API;
using EOSDigital.SDK;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Wrapper
{
    public class Camera
    {
        CanonAPI? APIHandler = null;
        Camera? MainCamera = null;
        string? ImageSaveDirectory = null;
        ManualResetEvent WaitEvent = new ManualResetEvent(false);
        List<Camera>? cameras = null;
        string defaultErrorMessage = "error";
        string noError = "function is safe";

        
    }
}
