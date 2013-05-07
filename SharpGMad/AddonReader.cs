using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bootil;

namespace Addon
{
    class Reader
    {
        public Reader()
        {
            //Clear();
        }

        //
        // Load an addon (call Parse after this succeeds)
        //
        /*public bool ReadFromFile(string strName)
        {
            
        }*/

        protected AutoBuffer m_buffer;
        protected char m_fmtversion;
        protected string m_name;
        protected string m_author;
        protected string m_desc;
        protected string m_type;
        protected List<Format.FileEntry> m_index;
        protected ulong m_fileblock;

        List<string> m_tags;
    }
}
