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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace WpfArcFace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables

        private readonly Component arcFaceComponent = new();

        private CancellationTokenSource cts = new();

        private readonly VistaFolderBrowserDialog folderDialog = new();

        private readonly TaskDialog taskDialog = new()
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

        private readonly TaskDialog instructionDialog = new()
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

        private readonly TaskDialog cancellationDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Cancellation",
            Content = "Calculations were stopped by user.",
            MainIcon = TaskDialogIcon.Warning
        };

        private readonly TaskDialog errorDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Error while deleting image",
            Content = "Image to delete was not selected. " +
            "Please, select the image from list \"Images in database\" first.",
            MainIcon = TaskDialogIcon.Error
        };

        private string firstFolderPath = "";

        private string secondFolderPath = "";

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            InitializeImageInDataBaseList();
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
            }
            else if (source.Name == "SecondDialogButton")
            {
                folderDialog.ShowDialog();
                secondFolderPath = folderDialog.SelectedPath;
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
                // get hash code of image in bytes
                string hash = GetHashCode(bytes);

                using (var database = new ImageDataBase())
                {
                    // try find image from DB
                    var imgFromDb = database.Faces.Where(item => item.Hash == hash)
                                                        .Where(item => Enumerable.SequenceEqual(item.Image, bytes))
                                                        .SingleOrDefault();
                    // if image is already in database - load its embedding
                    if (imgFromDb != null)
                    {
                        embeddings.Add(GetEmbeddingFromString(imgFromDb.Embedding));
                        ProgressBar.Value += 100 / paths.Length;
                    }
                    // else - get embedding from arcFaceNuget
                    else
                    {
                        img.Mutate(x => x.Resize(112, 112));

                        cts = new CancellationTokenSource();
                        Progress<int> progress = new();

                        Thread.Sleep(1000);
                        // using arcFaceNugetPackage
                        var results = await arcFaceComponent.GetDistanceAndSimilarity(
                            new Image<Rgb24>[] { img }, cts.Token, progress);

                        if (!cts.Token.IsCancellationRequested)
                        {
                            // add embedding to the list
                            embeddings.Add(results.Item3.First());

                            // add new image to DB
                            var imgInDb = AddImageToDb(hash, bytes, path.Split('\\').Last(), results.Item3.First());

                            // add image to listbox
                            AddImageToImageInDataBaseList(imgInDb);

                            ProgressBar.Value += 100 / paths.Length;
                        }
                    }
                }
            }

            var (distance, similarity) = GetDistanceAndSimilarityFromEmbeddings(embeddings);

            try
            {
                Similarity.Text = $"{similarity[0, 1]}";
                Distance.Text = $"{distance[0, 1]}";
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

        public void CancelCalculations(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            cancellationDialog.Show();
            Similarity.Text = "Not stated";
            Distance.Text = "Not stated";
            ProgressBar.Value = 0;
        }

        public void DeleteImageFromDb(object sender, RoutedEventArgs e)
        {
            var selectedItem = (StackPanel)DatabaseImageList.SelectedItem;

            if (selectedItem == null)
            {
                errorDialog.Show();
            }
            else
            {
                using var database = new ImageDataBase();
                var deletingImage = database.Faces.Where(face =>
                face.Title == selectedItem.Children.OfType<TextBlock>().Last().Text).FirstOrDefault();
                if (deletingImage != null)
                {
                    // remove face from DB
                    database.Faces.Remove(deletingImage);
                    database.SaveChanges();
                    // remove face from list
                    DatabaseImageList.Items.Remove(selectedItem);
                }
                else
                {
                    errorDialog.Content = "Selected image was not find in database.";
                    errorDialog.Show();
                }
            }
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

        private static ImageInDb AddImageToDb(string hash, byte[] img, string title, float[] embedding)
        {
            using var database = new ImageDataBase();
            ImageInDb image = new()
            {
                Hash = hash,
                Image = img,
                Title = title,
                Embedding = GetStringFromEmbedding(embedding)
            };
            database.Add(image);
            database.SaveChanges();
            return image;
        }

        private static string GetHashCode(byte[] array)
        {
            // compute the hash
            byte[] data = SHA256.Create().ComputeHash(array);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

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

        private void InitializeImageInDataBaseList()
        {
            using var database = new ImageDataBase();
            foreach (var face in database.Faces)
            {
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
                DatabaseImageList.Items.Add(panel);
            }
        }

        private void AddImageToImageInDataBaseList(ImageInDb face)
        {
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
            DatabaseImageList.Items.Add(panel);
        }

        private static BitmapImage ToBitmapImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private static float[] GetEmbeddingFromString(string emb)
        {
            List<float> embedding = new();
            Array.ForEach(emb.Split(' '), token => { embedding.Add(float.Parse(token)); });
            return embedding.ToArray();
        }

        private static string GetStringFromEmbedding(float[] emb)
        {
            return string.Join(' ', emb);
        }
        #endregion
    }
}
