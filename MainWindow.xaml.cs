using System;
using System.Timers;
using System.Windows;

namespace PropertyHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsCapturing
        {
            set
            {
                this.Dispatcher.Invoke(() => Status.Text = value ? "Включен" : "Выключен");
            }
        }

        Timer CaptureTimer = new Timer(100);
        public MainWindow()
        {
            InitializeComponent();
            CaptureTimer.Elapsed += Capture_Elapsed;
            IsCapturing = false;
        }
        delegate void DoMainThread();

        private void GetINotifyPropertyChangedToClipboard(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText("#region INotifyPropertyChanged\r\n        public event PropertyChangedEventHandler PropertyChanged;\r\n        protected void OnPropertyChanged([CallerMemberName] String prop = \"\")\r\n        {\r\n            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));\r\n        }\r\n        protected Boolean SetProperty<T>(ref T backingStore, T value,\r\n            [CallerMemberName] String propertyName = \"\",\r\n            Action onChanged = null)\r\n        {\r\n            if (EqualityComparer<T>.Default.Equals(backingStore, value))\r\n                return false;\r\n\r\n            backingStore = value;\r\n            onChanged?.Invoke();\r\n            OnPropertyChanged(propertyName);\r\n            return true;\r\n        }\r\n        #endregion");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось уставить в буфер обмена", ex.Message, MessageBoxButton.OK);
            }

        }
        private void EnableCapture(object sender, RoutedEventArgs e)
        {
            CaptureTimer.Start();
            IsCapturing = true;
        }

        private void DisableCapture(object sender, RoutedEventArgs e)
        {
            CaptureTimer.Stop();
            IsCapturing = false;
        }
        private void Capture_Elapsed(object sender, ElapsedEventArgs e)
        {


            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    string Text = Clipboard.GetText();
                    if (Text.Contains("{ get; set; }"))
                    {
                        string[] textData = Text.Split("{ get; set; }", StringSplitOptions.RemoveEmptyEntries);
                        string[] leftData = textData[0].Split(" ",StringSplitOptions.RemoveEmptyEntries);
                        string type = leftData[leftData.Length - 2];
                        string varName = leftData[leftData.Length - 1];
                        //2-5 { get; set; }
                        string varData = String.Empty;
                        if (textData.Length > 1)
                        {
                            varData = String.Join(" ", textData[1..]);
                        }
                        varData += ";";
                        try
                        {
                            if (varData[varData.Length - 2] == ';') varData = varData.Substring(0, varData.Length - 1);
                        } 
                        catch { }
                        string modifires = string.Empty;
                        if (leftData.Length > 2)
                            modifires = String.Join(" ", leftData[0..(leftData.Length - 2)]);
                        string subVar = $"{modifires.Replace("public","private")} {type} _{varName} {varData}";
                        string nomVar = modifires+" "+type + " " + varName;//основа
                        nomVar += "\r\n{\r\nget { return _" + varName + "; }";//get
                        nomVar += "\r\nset { SetProperty(ref _" + varName + ", value, \"" + varName + "\"); }\r\n}";//set
                        Clipboard.SetText(subVar + "\r\n" + nomVar);
                    }
                }
                catch { }
            });
        }
    }
}
