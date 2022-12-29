#if WINDOWS
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.System;
using Microsoft.Maui.Platform;
using WinRT.Interop;

public class CameraCaptureUI
{
    private LauncherOptions _launcherOptions;

    public CameraCaptureUI(MediaPickerOptions options)
    {
        var hndl = WindowStateManager.Default.GetActiveWindow().GetWindowHandle();

        _launcherOptions = new LauncherOptions();
        InitializeWithWindow.Initialize(_launcherOptions, hndl);

        _launcherOptions.TreatAsUntrusted = false;
        _launcherOptions.DisplayApplicationPicker = false;
        _launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsCamera_8wekyb3d8bbwe";
    }

    public async Task<StorageFile> CaptureFileAsync(CameraCaptureUIMode mode)
    {
        var extension = mode == CameraCaptureUIMode.Photo ? ".jpg" : ".mp4";

        var currentAppData = ApplicationData.Current;
        var tempLocation = currentAppData.LocalCacheFolder;
        var tempFileName = $"CCapture{extension}";
        var tempFile = await tempLocation.CreateFileAsync(tempFileName, CreationCollisionOption.GenerateUniqueName);
        var token = Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.AddFile(tempFile);

        var set = new ValueSet();
        if (mode == CameraCaptureUIMode.Photo)
        {
            set.Add("MediaType", "photo");
            set.Add("PhotoFileToken", token);
        }
        else
        {
            set.Add("MediaType", "video");
            set.Add("VideoFileToken", token);
        }

        var uri = new Uri("microsoft.windows.camera.picker:");
        var result = await Windows.System.Launcher.LaunchUriForResultsAsync(uri, _launcherOptions, set);
        if (result.Status == LaunchUriStatus.Success && result.Result != null)
        {
            return tempFile;
        }

        return null;
    }
}
#endif