using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ArcFaceNuget;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading;
using ImageControl = System.Windows.Controls.Image;
using Image = SixLabors.ImageSharp.Image;
using System.Collections.Generic;
using WPFArcFaceApi;

namespace WpfArcFace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables

        private Service service = new();

        private readonly Component arcFaceComponent = new();

        private CancellationTokenSource cts = new();

       

        private string firstFolderPath = "";

        private string secondFolderPath = "";

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            FillImageInDataBaseList();
        }

        public void OpenInstructionDialog(object sender, RoutedEventArgs e)
        {
            Dialogues.instructionDialog.Show();
        }


        public void OpenFolderDialog(object sender, RoutedEventArgs e)
        {
            Button source = (Button)e.Source;
            if (source.Name == "FirstDialogButton")
            {
                Dialogues.folderDialog.ShowDialog();
                firstFolderPath = Dialogues.folderDialog.SelectedPath;
                if (String.IsNullOrEmpty(firstFolderPath))
                {
                    Dialogues.emptyPathErrorDialog.Show();
                    return;
                }
                FirstPathTextBlock.Text = firstFolderPath.Split('\\').Last();
                FillImageListFromFolder(firstFolderPath, FirstImageList);
                // ask if user wants to open this folder for second image
                var pressedButton = Dialogues.taskDialog.Show();
                if (pressedButton?.Text == "Yes")
                {
                    secondFolderPath = firstFolderPath;
                    SecondPathTextBlock.Text = FirstPathTextBlock.Text;
                    FillImageListFromFolder(secondFolderPath, SecondImageList);
                }
            }
            else if (source.Name == "SecondDialogButton")
            {
                Dialogues.folderDialog.ShowDialog();
                secondFolderPath = Dialogues.folderDialog.SelectedPath;
                if (String.IsNullOrEmpty(secondFolderPath))
                {
                    Dialogues.emptyPathErrorDialog.Show();
                    return;
                }
                SecondPathTextBlock.Text = secondFolderPath.Split('\\').Last();
                FillImageListFromFolder(secondFolderPath, SecondImageList);
            }
            else
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

            // creating paths to images 
            var paths = new string[]
            {
                firstFolderPath + "\\"
                + firstSelectedItem.Children.OfType<TextBlock>().Last().Text,
                secondFolderPath + "\\"
                + secondSelectedItem.Children.OfType<TextBlock>().Last().Text
            };

            var embeddings = new List<float[]>();

            foreach (var path in paths)
            {
                // load image as Image<Rgb24>
                using var img = Image.Load<Rgb24>(path);
                // load image as bytes array
                byte[] bytes = File.ReadAllBytes(path);

                string message = string.Empty;

                cts = new CancellationTokenSource();

                ProgressBar.Value += 100 / (3 * paths.Length);

                if (cts.IsCancellationRequested) return;

                // get image ID in database
                var id = await service.GetImageId(bytes, path.Split('\\').Last(), cts.Token);

                ProgressBar.Value += 100 / (3 * paths.Length);

                if (cts.IsCancellationRequested) return;

                // get info about image from Db by ID
                var imageFromDb = await service.GetImageInfoById(id, cts.Token);

                if (imageFromDb != null)
                {
                    embeddings.Add(GetEmbeddingFromString(imageFromDb.Embedding));
                    AddImageToImageInDatabaseList(imageFromDb);
                    ProgressBar.Value += 100 / (3 * paths.Length);
                }
                else
                {
                    Dialogues.noEmbeddingErrorDialog.Show();
                    return;
                }
            }

            if (cts.IsCancellationRequested) return;

            var (distance, similarity) = GetDistanceAndSimilarityFromEmbeddings(embeddings);

            FillDistanceAndSimilarity(distance, similarity);
        }

        public void CancelCalculations(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            Dialogues.cancellationDialog.Show();
            Similarity.Text = "Not stated";
            Distance.Text = "Not stated";
            ProgressBar.Value = 0;
            AnalyseButton.IsEnabled = true;
            CancellationButton.IsEnabled = false;
        }

        public async void DeleteImageFromDb(object sender, RoutedEventArgs e)
        {
            if (await service.DeleteAllFromDatabase())
            {
                Dialogues.cleanDatabaseSuccessDialog.Show();
            }
            else
            {
                Dialogues.cleanDatabaseErrorDialog.Show();
            }

            FillImageInDataBaseList();
        }

        #region Private methods

        private (float[,] Distance, float[,] Similarity) GetDistanceAndSimilarityFromEmbeddings(List<float[]> embeddings)
        {
            float[,] distanceMatrix = new float[embeddings.Count, embeddings.Count];

            float[,] similarityMatrix = new float[embeddings.Count, embeddings.Count];

            int i = 0;

            foreach (var emb1 in embeddings)
            {
                int j = 0;
                foreach (var emb2 in embeddings)
                {
                    distanceMatrix[i, j] = arcFaceComponent.Distance(emb1, emb2);
                    similarityMatrix[i, j] = arcFaceComponent.Similarity(emb1, emb2);
                    j++;
                }
                i++;
            }

            return (distanceMatrix, similarityMatrix);
        }

        private void ListSelectionChange(object? sender, SelectionChangedEventArgs e)
        {
            if (FirstImageList.SelectedItems.Count > 0 && SecondImageList.SelectedItems.Count > 0)
            {
                AnalyseButton.IsEnabled = true;
            }
        }

        private static void FillImageListFromFolder(string path, ListBox list)
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
                TextBlock textBlock = new TextBlock
                {
                    Text = imgpath.Split('\\').Last(),
                    Padding = new Thickness(10, 25, 0, 0)
                };
                // create new stackpanel
                StackPanel panel = new()
                {
                    Orientation = Orientation.Horizontal
                };
                panel.Children.Add(image);
                panel.Children.Add(textBlock);
                // add stackpanel to list
                list.Items.Add(panel);
            }
        }

        private async void FillImageInDataBaseList()
        {
            DatabaseImageList.Items.Clear();

            // get all IDs from Db
            var ids = await service.GetAllImagesIdsFromDatabase();

            if (ids == null) return;

            foreach (var id in ids)
            {
                // get info about image from Db by ID
                var imageFromDb = await service.GetImageInfoById(id, new CancellationToken());

                if (imageFromDb == null)
                {
                    Dialogues.noEmbeddingErrorDialog.Show();
                    continue;
                }

                AddImageToImageInDatabaseList(imageFromDb);
            }
        }

        private void AddImageToImageInDatabaseList(ImageInDb face)
        {
            if (CheckIsImageInImageInDatabaseList(face.Id)) return;

            // create new image element
            ImageControl image = new()
            {
                Source = ToBitmapImage(face.Image),
                Width = 60
            };

            // create textBlock element with image name
            TextBlock textBlock = new()
            {
                Text = face.Title,
                Padding = new Thickness(10, 10, 0, 0)
            };

            // create textBlock element with image ID
            TextBlock textBlockId = new()
            {
                Text = $"id: {face.Id}",
                Padding = new Thickness(10, 10, 0, 0)
            };

            // create new stackpanel for text
            StackPanel textPanel = new()
            {
                Orientation = Orientation.Vertical
            };

            textPanel.Children.Add(textBlock);
            textPanel.Children.Add(textBlockId);

            // create new stackpanel
            StackPanel panel = new()
            {
                Orientation = Orientation.Horizontal
            };
            panel.Children.Add(image);
            panel.Children.Add(textPanel);
            // add stackpanel to list
            DatabaseImageList.Items.Add(panel);
        }

        private bool CheckIsImageInImageInDatabaseList(int Id)
        {
            foreach (StackPanel stackPanel in DatabaseImageList.Items)
            {
                var imgId = GetImageIdFromStackPanel(stackPanel);
                if (imgId == Id) return true;
            }

            return false;
        }


        private static int GetImageIdFromStackPanel(StackPanel panel)
        {
            var textPanel = panel.Children[1] as StackPanel;

            var idString = (textPanel.Children[1] as TextBlock).Text;

            return int.Parse(idString.Split(' ').Last());
        }

        private static BitmapImage ToBitmapImage(byte[] array)
        {
            using var ms = new MemoryStream(array);

            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad; 
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        private static float[] GetEmbeddingFromString(string emb)
        {
            List<float> embedding = new();
            Array.ForEach(emb.Split(' '), token => { embedding.Add(float.Parse(token)); });
            return embedding.ToArray();
        }

        private void FillDistanceAndSimilarity(float[,] distance, float[,] similarity)
        {
            try
            {
                Similarity.Text = $"{similarity[0, 1]}";
                Distance.Text = $"{distance[0, 1]}";
                ProgressBar.Value = 100;
            }
            catch
            {
                Similarity.Text = "Not stated";
                Distance.Text = "Not stated";
                ProgressBar.Value = 0;
            }
            finally
            {
                // enable Analyse Button after computations end
                AnalyseButton.IsEnabled = true;
                CancellationButton.IsEnabled = false;
            }
        }


        #endregion
    }
}
