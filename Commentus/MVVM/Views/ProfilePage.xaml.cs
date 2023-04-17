namespace Commentus.MVVM.Views;

using Commentus.Database;
using SkiaSharp;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

    private async void PickProfileImage(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Pick image",
            FileTypes = FilePickerFileType.Images
        });

        if (result == null)
            return;

        var stream = await result.OpenReadAsync();
        var bitmap = SKBitmap.Decode(stream);

        var roundedBitmap = bitmap.CreateCircularImage(bitmap.Width, bitmap.Height);

        byte[] byteArray = roundedBitmap.ToByteArray(SKEncodedImageFormat.Png);

        profileImage.Source = ImageSource.FromStream(() => roundedBitmap.Encode(SKEncodedImageFormat.Png, 80).AsStream());

        DatabaseCommands.SendUsersProfileImageToDb(byteArray);
    }
}