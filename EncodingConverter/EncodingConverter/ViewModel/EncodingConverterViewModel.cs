using EncodingConverter.Command;
using EncodingConverter.Model;
using EncodingConverter.Properties;
//using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EncodingConverter.ViewModel
{
  class EncodingConverterViewModel : INotifyPropertyChanged
  {
    private readonly List<string> fileNames = new List<string>();
    private HashSet<Encoding> fileEncodings = new HashSet<Encoding>();
    private EncodingModel _selectedEncoding;
    private bool _isConvertButtonEnabled;

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
        IsConvertButtonEnabled = fileEncodings.Count() == 1 && SelectedEncoding.EncodingName == fileEncodings.First().EncodingName;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedEncoding"));
      }
    }

    public bool IsConvertButtonEnabled
    {
      get { return _isConvertButtonEnabled; }
      set
      {
        if (_isConvertButtonEnabled == value) return;
        _isConvertButtonEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConvertButtonEnabled"));
      }
    }
    public Action CloseAction { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;

    public EncodingConverterViewModel(List<string> selectedDocuments)
    {
      foreach (string document in selectedDocuments)
      {
        GetAllFiles(document);
      }

    }

    private void GetAllFiles(string document)
    {
      if (!Directory.Exists(document))
      {
        if (File.Exists(document))
        {
          fileNames.Add(document);
        }
        return;
      }

      DirectoryInfo directory = new DirectoryInfo(document);

      var allFiles = directory.GetFiles();
      foreach (var file in allFiles)
      {
        fileNames.Add(file.FullName);
      }

      var allDirectories = directory.GetDirectories();
      foreach (var dir in allDirectories)
      {
        GetAllFiles(dir.FullName);
      }
    }

    public async Task LoadData()
    {
      //CancelCommand = new RelayCommand(CancelCommandExecute);
      CancelCommand = new RelayCommand(o => { CancelCommandExecute(); });
      ConvertCommand = new RelayCommand(o => { ConvertCommandExecute(); });

      foreach (var file in fileNames)
      {
        fileEncodings.Add(GetEncoding(file));
      }

      CurrentEncodingText = string.Format("Current Encoding: {0}", fileEncodings.Count() == 1 ? fileEncodings.First().EncodingName : "multiple encodings");

      InitializeEncodingList();
    }

    private void InitializeEncodingList()
    {
      EncodingCollection = new ObservableCollection<EncodingModel>();

      AddEncoding(Encoding.UTF8, "UTF-8");
      AddEncoding(Encoding.UTF32, "UTF-32");
      AddEncoding(Encoding.Unicode, "Unicode");
      AddEncoding(Encoding.BigEndianUnicode, "Big Endian");
      AddEncoding(Encoding.GetEncoding("utf-32BE"), "UTF-32 Big Endian");

      //foreach (EncodingInfo ei in Encoding.GetEncodings())
      //{
      //    AddEncoding(ei.GetEncoding());
      //}

      SelectedEncoding = EncodingCollection.FirstOrDefault(e => e.Encoding.EncodingName == Resources.UTF8Encoding);
    }

    private void AddEncoding(Encoding encoding, string encodingName)
    {
      EncodingCollection.Add(new EncodingModel { EncodingName = encodingName, Encoding = encoding });
    }

    private void CancelCommandExecute()
    {
      CloseAction?.Invoke();
    }

    private void ConvertCommandExecute()
    {
      foreach (var file in fileNames)
      {
        ConvertFile(file);
      }

      CancelCommandExecute();
    }

    private void ConvertFile(string file)
    {
      if (SelectedEncoding == null || fileEncodings.Count() == 1 && SelectedEncoding.Encoding == fileEncodings.First())
      {
        return;
      }

      StreamReader streamReader = new StreamReader(file);
      string fileContent = streamReader.ReadToEnd();
      streamReader.Close();
      File.WriteAllText(file, fileContent, SelectedEncoding.Encoding);
    }

    private Encoding GetEncoding(string fileName)
    {
      using (var reader = new StreamReader(fileName, Encoding.Default, true))
      {
        if (reader.Peek() >= 0)
        {
          reader.Read();
        }
        return reader.CurrentEncoding;
      }
    }
  }
}
