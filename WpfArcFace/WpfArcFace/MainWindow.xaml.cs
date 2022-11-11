using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ArcFaceNuget;
using Ookii.Dialogs.Wpf;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using ImageControl = System.Windows.Controls.Image;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace WpfArcFace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables

        private readonly Component arcFaceComponent = new();

        private CancellationTokenSource cts;

        private VistaFolderBrowserDialog folderDialog = new();

        private TaskDialog taskDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("Yes"),
                new TaskDialogButton("No")
            },
            WindowTitle = "Folder select",
            Content = "Do you want to open this folder for second image too?",
            MainIcon = TaskDialogIcon.Information
        };

        private TaskDialog instructionDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("Let's go!")
            },
            WindowTitle = "How does this work?",
            Content = "1. Select folder with your images by clicking on 📁 button\n" +
                      "2. Select fisrt and secong images to compare\n" +
                      "3. Press Analyse Images button\n\n" +
                      "Voilà!🙂",
            MainIcon = TaskDialogIcon.Information
        };

        private TaskDialog cancellationDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Cancellation",
            Content = "Calculations were stopped by user.",
            MainIcon = TaskDialogIcon.Warning
        };

        private string firstFolderPath = "";

        private string secondFolderPath = "";

        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OpenInstructionDialog(object sender, RoutedEventArgs e)
        {
            instructionDialog.Show();
        }


        public void OpenFolderDialog(object sender, RoutedEventArgs e)
        {
            Button source = (Button)e.Source;
            if (source.Name == "FirstDialogButton")
            {
                folderDialog.ShowDialog();
                firstFolderPath = folderDialog.SelectedPath;
                FirstPathTextBlock.Text = firstFolderPath.Split('\\').Last();
                FillImageListFromFolder(firstFolderPath, FirstImageList);
                // ask if user wants to open this folder for second image
                var pressedButton = taskDialog.Show();
                if (pressedButton?.Text == "Yes")
                {
                    secondFolderPath = firstFolderPath;
                    SecondPathTextBlock.Text = FirstPathTextBlock.Text;
                    FillImageListFromFolder(secondFolderPath, SecondImageList);
                }
            } else if (source.Name == "SecondDialogButton")
            {
                folderDialog.ShowDialog();
                secondFolderPath = folderDialog.SelectedPath;
                SecondPathTextBlock.Text = secondFolderPath.Split('\\').Last();
                FillImageListFromFolder(secondFolderPath, SecondImageList);
            } else
            {
                throw new Exception("Unexpected call for folder dialog");
            }

        }

        public async void AnalyseImages(object sender, RoutedEventArgs e)
        {
            // disable Analyse Button while doing computations
            AnalyseButton.IsEnabled = false;
            CancellationButton.IsEnabled = true;
            ProgressBar.Value = 0;

            var firstSelectedItem = (StackPanel)FirstImageList.SelectedItem;
            var secondSelectedItem = (StackPanel)SecondImageList.SelectedItem;

            // loading images
            using var face1 = Image.Load<Rgb24>(firstFolderPath + "\\"
                + firstSelectedItem.Children.OfType<TextBlock>().Last().Text);
            using var face2 = Image.Load<Rgb24>(secondFolderPath + "\\"
                + secondSelectedItem.Children.OfType<TextBlock>().Last().Text);
            // resizing images
            face1.Mutate(x => x.Resize(112, 112));
            face2.Mutate(x => x.Resize(112, 112));

            var images = new Image<Rgb24>[] { face1, face2 };

            cts = new CancellationTokenSource();
            Progress<int> progress = new();
            progress.ProgressChanged += ReportProgress;

            Thread.Sleep(100);
            // using arcFaceNugetPackage
            var results = await arcFaceComponent.GetDistanceAndSimilarity(images, cts.Token, progress);
            Thread.Sleep(100);

            if (cts.Token.IsCancellationRequested)
            {
                ProgressBar.Value = 0;
                Similarity.Text = "Not stated";
                Distance.Text = "Not stated";
            }
            else
            {
                Similarity.Text = $"{results.Item2[0, 1]}";
                Distance.Text = results.Item1[0, 1].ToString();
            }

            // enable Analyse Button after computations end
            AnalyseButton.IsEnabled = true;
            CancellationButton.IsEnabled = false;
        }

        public void CancelCalculations(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cancellationDialog.Show();
            Similarity.Text = "Not stated";
            Distance.Text = "Not stated";
            ProgressBar.Value = 0;
        }

        #region Private methods

        private void ListSelectionChange(object? sender, SelectionChangedEventArgs e) 
        {
            if (FirstImageList.SelectedItems.Count > 0 && SecondImageList.SelectedItems.Count > 0)
            {
                AnalyseButton.IsEnabled = true;
            }
        }

        private void ReportProgress(object? sender, int e)
        {
            ProgressBar.Value = e;
        }


        private void FillImageListFromFolder(string path, ListBox list)
        {
            // get all .png and .jpg items from directory 
            var imagePaths = Directory.GetFiles(path)
                .Where(path => path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg"));
            // put images in list
            list.Items.Clear();
            foreach (var imgpath in imagePaths)
            {
                // create new image element
                ImageControl image = new()
                {
                    Source = new BitmapImage(new Uri(imgpath)),
                    Width = 60
                };

                // create textBlock element with image name
                TextBlock textBlock = new TextBlock();
                textBlock.Text = imgpath.Split('\\').Last();
                textBlock.Padding = new Thickness(10, 25, 0, 0);
                // create new stackpanel
                StackPanel panel = new();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(image);
                panel.Children.Add(textBlock);
                // add stackpanel to list
                list.Items.Add(panel);
            }
        }

        #endregion
    }
}
