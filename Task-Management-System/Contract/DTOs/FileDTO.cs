using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.DTOs
{
    public class FileDto
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }    
        public string FileName { get; set; }   
        public string ContentType { get; set; }
    }

}
