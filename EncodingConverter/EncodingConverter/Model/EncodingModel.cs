using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncodingConverter.Model
{
    class EncodingModel
    {
        public bool IsSelected { get; set; }
        public string EncodingName { get; set; }
        public Encoding Encoding { get; set; }
    }
}
