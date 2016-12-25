using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Connectivity;
using Plugin.Media;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Plugin.Media.Abstractions;

namespace FaceEmotionRecognition
{
    public partial class MainPage : ContentPage
    {
        private readonly IFaceServiceClient faceServiceClient;
        private readonly EmotionServiceClient emotionServiceClient;
        public MainPage()
        {
            InitializeComponent();

            this.faceServiceClient = new FaceServiceClient("a5c068c4fa8b4730bfc4afbf24e94a0f");
            this.emotionServiceClient = new EmotionServiceClient("cf7da475d38b4e669a4e2792f650a8b6");
        }
        private async Task<FaceEmotionDetection> DetectFaceAndEmotionsAsync(MediaFile inputFile)
        {
            
            try
            {
                // Get emotions from the specified stream
                Emotion[] emotionResult = await
                  emotionServiceClient.RecognizeAsync(inputFile.GetStream());
                // Assuming the picture has one face, retrieve emotions for the
                // first item in the returned array
                var faceEmotion = emotionResult[0]?.Scores.ToRankedList();

                // Create a list of face attributes that the
                // app will need to retrieve
                var requiredFaceAttributes = new FaceAttributeType[] {
                  FaceAttributeType.Age,
                  FaceAttributeType.Gender,
                  FaceAttributeType.Smile,
                  FaceAttributeType.FacialHair,
                  FaceAttributeType.HeadPose,
                  FaceAttributeType.Glasses
                  };

                // Get a list of faces in a picture
                var faces = await faceServiceClient.DetectAsync(inputFile.GetStream(),
                  false, false, requiredFaceAttributes);
                // Assuming there is only one face, store its attributes
                var faceAttributes = faces[0]?.FaceAttributes;

                FaceEmotionDetection faceEmotionDetection = new FaceEmotionDetection();
                faceEmotionDetection.Age = faceAttributes.Age;
                faceEmotionDetection.Emotion = faceEmotion.FirstOrDefault().Key;
                faceEmotionDetection.Glasses = faceAttributes.Glasses.ToString();
                faceEmotionDetection.Smile = faceAttributes.Smile;
                faceEmotionDetection.Gender = faceAttributes.Gender;
                faceEmotionDetection.Moustache = faceAttributes.FacialHair.Moustache;
                faceEmotionDetection.Beard = faceAttributes.FacialHair.Beard;

                return faceEmotionDetection;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                return null;
            }
        }

        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;
            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
            Image1.Source = ImageSource.FromStream(() => file.GetStream());
            FaceEmotionDetection theData = await DetectFaceAndEmotionsAsync(file);
            this.BindingContext = theData;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }

        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.
              IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = true,
                Name = "test.jpg"
            });
            if (file == null)
                return;
            this.Indicator1.IsVisible = true;
            this.Indicator1.IsRunning = true;
            Image1.Source = ImageSource.FromStream(() => file.GetStream());
            FaceEmotionDetection theData = await DetectFaceAndEmotionsAsync(file);
            this.BindingContext = theData;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
        }
    }
}
