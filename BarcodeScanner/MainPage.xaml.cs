#if WINDOWS
using Windows.Media.Capture;
#endif

using IronBarCode;

namespace BarcodeScanner;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
        License.LicenseKey = "ADD_YOUR_LICENSE";
        IronBarCode.Logging.Logger.LoggingMode = IronBarCode.Logging.Logger.LoggingModes.All;
        IronBarCode.Logging.Logger.LogFilePath = "BarcodeScanner.log";

    }

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

#if WINDOWS
    public async void ReadBarcode(object sender, EventArgs e)
    {
        var options = new MediaPickerOptions();
        var photo = await CaptureAsync(options, true);
        if (photo != null)
        {
            var result = await BarcodeReader.ReadAsync(photo.FullPath);
            if (result != null && result.Count() > 0)
            {
                Title.Text = "Reading from Webcam";
                Description.Text = $"Value = {result.First().Value}, Type = {result.First().BarcodeType.ToString()}";
            }
        }
        else
        {
            Title.Text = "Reading from Webcam";
            Description.Text = "Cannot read Barcode.";
        }
    }

    public async void TakePhoto(object sender, EventArgs e)
    {
        var options = new MediaPickerOptions();
        var photo = await CaptureAsync(options, true);
    }
    public async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
    {
        var captureUi = new CameraCaptureUI(options);

        var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

        if (file != null)
            return new FileResult(file.Path, file.ContentType);

        return null;
    }
#else
    public async void ReadBarcode(object sender, EventArgs e)
    {
        var options = new MediaPickerOptions();
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                // save the file into local storage
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using Stream sourceStream = await photo.OpenReadAsync();

                var result = await BarcodeReader.ReadAsync(sourceStream);
                if (result != null)
                {
                    Title.Text = "Reading from Webcam";
                    Description.Text = $"Value = {result.First().Value}, Type = {result.First().BarcodeType.ToString()}";
                }
            }
            else
            {
                // *** IT ALWAYS ENTERS IN THE ELSE CLAUSE ***
                // *** BECAUSE photo IS ALWAYS NULL ***
                CounterBtn.Text = $"Capture is supported but {photo} is null";
                Title.Text = "Reading from Webcam";
                Description.Text = "Cannot read Barcode.";
            }
        }
    }

    public async void TakePhoto(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                // save the file into local storage
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using Stream sourceStream = await photo.OpenReadAsync();
                using FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);
            }
            else
            {
                // *** IT ALWAYS ENTERS IN THE ELSE CLAUSE ***
                // *** BECAUSE photo IS ALWAYS NULL ***
                CounterBtn.Text = $"Capture is supported but {photo} is null";
            }
        }
    }
#endif
}

