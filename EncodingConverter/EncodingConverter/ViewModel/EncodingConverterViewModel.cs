using EncodingConverter.Model;
using EncodingConverter.Properties;
using EnvDTE;
using EnvDTE80;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EncodingConverter.ViewModel
{
    class EncodingConverterViewModel
    {
        public ObservableCollection<EncodingModel> EncodingCollection { get; set; }
        public Action CloseAction { get; set; }

        private string fileName;

        private byte[] fileData;

        private Encoding fileEncoding;

        private int markerLength;
        public string EncodingText { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ConvertCommand { get; set; }

        public EncodingConverterViewModel(string selectedFile)
        {
            fileName = selectedFile;
        }

        public void LoadData()
        {
            CancelCommand = new RelayCommand(CancelCommandExecute);

            ConvertCommand = new RelayCommand(ConvertCommandExecute);

            fileEncoding = FindEncoding(fileName);
           
            EncodingText = string.Format("Current Encoding: {0}", fileEncoding != null ? fileEncoding.EncodingName : "unindentified");

            InitializeEncodingList();
        }

        private void InitializeEncodingList()
        {
            EncodingCollection = new ObservableCollection<EncodingModel>();

            EncodingCollection.Add(new EncodingModel { IsSelected = fileEncoding == Encoding.UTF7, EncodingName = Resources.UTF7Encoding, Encoding = Encoding.UTF7});
            EncodingCollection.Add(new EncodingModel { IsSelected = fileEncoding == Encoding.UTF8, EncodingName = Resources.UTF8Encoding, Encoding = Encoding.UTF8});
            EncodingCollection.Add(new EncodingModel { IsSelected = fileEncoding == Encoding.Unicode, EncodingName = Resources.UnicodeEncoding, Encoding = Encoding.Unicode });
            EncodingCollection.Add(new EncodingModel { IsSelected = fileEncoding == Encoding.BigEndianUnicode, EncodingName = Resources.BigEndianUnicodeEncoding, Encoding = Encoding.BigEndianUnicode });
            EncodingCollection.Add(new EncodingModel { IsSelected = fileEncoding == Encoding.ASCII, EncodingName = Resources.AsciiEncoding, Encoding = Encoding.ASCII });
        }

        private void CancelCommandExecute()
        {
            CloseAction?.Invoke();
        }

        private void ConvertCommandExecute()
        {
            ConvertFile();
        }

        private void ConvertFile()
        {
            //Encoding fileEncoding = FindEncoding(file);

            if (fileEncoding != null)
            {
                byte[] strippedBytes = new byte[fileData.Length - markerLength];
                Buffer.BlockCopy(fileData, markerLength, strippedBytes, 0, strippedBytes.Length);
                byte[] convertedBytes = Encoding.Convert(fileEncoding, Encoding.UTF8, strippedBytes);
                File.WriteAllBytes(fileName, convertedBytes);
            }
        }

        private Encoding FindEncoding(string fileName)
        {
            var bom = new byte[4];
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }



    }
}
