using EncodingConverter.Model;
using EncodingConverter.Properties;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EncodingConverter.ViewModel
{
    class EncodingConverterViewModel : INotifyPropertyChanged
    {
        private readonly string fileName;
        private Encoding fileEncoding;
        private EncodingModel _selectedEncoding;

        public string CurrentEncodingText { get; set; }
        public ObservableCollection<EncodingModel> EncodingCollection { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ConvertCommand { get; set; }

        public EncodingModel SelectedEncoding
        {
            get { return _selectedEncoding; }
            set
            {
                if (_selectedEncoding == value) return;
                _selectedEncoding = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedEncoding"));
            }
        }
        public Action CloseAction { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public EncodingConverterViewModel(string selectedFile)
        {
            fileName = selectedFile;
        }

        public void LoadData()
        {
            CancelCommand = new RelayCommand(CancelCommandExecute);

            ConvertCommand = new RelayCommand(ConvertCommandExecute);

            fileEncoding = FindEncoding(fileName);

            CurrentEncodingText = string.Format("Current Encoding: {0}", fileEncoding != null ? fileEncoding.EncodingName : "unindentified");

            InitializeEncodingList();
        }

        private void InitializeEncodingList()
        {
            EncodingCollection = new ObservableCollection<EncodingModel>
            {
                //new EncodingModel { EncodingName = Resources.UTF7Encoding, Encoding = Encoding.UTF7 },
                new EncodingModel { EncodingName = Resources.UTF8Encoding, Encoding = Encoding.UTF8 },
                new EncodingModel { EncodingName = Resources.UnicodeEncoding, Encoding = Encoding.Unicode },
                new EncodingModel { EncodingName = Resources.BigEndianUnicodeEncoding, Encoding = Encoding.BigEndianUnicode },
                //new EncodingModel { EncodingName = Resources.UTF32Encoding, Encoding = Encoding.UTF32 }
            };

            SelectedEncoding = EncodingCollection.FirstOrDefault(e => e.Encoding == fileEncoding);
        }

        private void CancelCommandExecute()
        {
            CloseAction?.Invoke();
        }

        private void ConvertCommandExecute()
        {
            ConvertFile();
            CancelCommandExecute();
        }

        private void ConvertFile()
        {
            if (SelectedEncoding == null || SelectedEncoding.Encoding == fileEncoding)
            {
                return;
            }

            StreamReader streamReader = new StreamReader(fileName);
            string fileContent = streamReader.ReadToEnd();
            streamReader.Close();

            StreamWriter streamWriter = new StreamWriter(fileName, false, SelectedEncoding.Encoding);
            streamWriter.Write(fileContent);
            streamWriter.Close();
        }

        private Encoding FindEncoding(string fileName)
        {
            var bom = new byte[4];
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }
            // Analyze the BOM
            //if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            //if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE

            return null;
        }
    }
}
