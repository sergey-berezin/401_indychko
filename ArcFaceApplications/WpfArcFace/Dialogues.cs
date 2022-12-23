using Ookii.Dialogs.Wpf;

namespace WpfArcFace
{
    public static class Dialogues
    {
        public static readonly VistaFolderBrowserDialog folderDialog = new();

        public static readonly TaskDialog taskDialog = new()
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

        public static readonly TaskDialog instructionDialog = new()
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

        public static readonly TaskDialog cancellationDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Cancellation",
            Content = "Calculations were stopped by user.",
            MainIcon = TaskDialogIcon.Warning
        };

        public static readonly TaskDialog cleanDatabaseErrorDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Error while deleting images",
            Content = "Strange error occured while cleaning database.\n " +
                      "Please ensure that connection to database is OK.",
            MainIcon = TaskDialogIcon.Error
        };

        public static readonly TaskDialog emptyPathErrorDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Error while opening folder",
            Content = "No folder was selected! Please, select folder.",
            MainIcon = TaskDialogIcon.Error
        };

        public static readonly TaskDialog noEmbeddingErrorDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Error while analysing image",
            Content = "Image was not loaded from database correctly. " +
                      "So we can't extract image embedding for future analysis\n" +
                      "Possible errors: \n" +
                      "-- embedding can not be calculated\n" +
                      "-- no access to database",
            MainIcon = TaskDialogIcon.Error
        };

        public static readonly TaskDialog cleanDatabaseSuccessDialog = new()
        {
            Buttons =
            {
                new TaskDialogButton("OK")
            },
            WindowTitle = "Cleaning database",
            Content = "All images were successfully deleted from database.",
            MainIcon = (TaskDialogIcon)65528
        };
    }
}
