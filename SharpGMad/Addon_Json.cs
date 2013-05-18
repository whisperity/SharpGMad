using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SharpGMad
{
    [DataContract]
    public class DescriptionJSON
    {
        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "tags")]
        public List<string> Tags;
    }

    [DataContract]
    public class AddonJSON
    {
        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "tags")]
        public List<string> Tags;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "ignore")]
        public List<string> Ignore;
    }

    class Json
    {
        string m_strError;

        string m_Title;
        string m_Description;
        string m_AddonType;
        List<string> m_Ignores = new List<string>();
        List<string> m_Tags = new List<string>();

        // Getters... should be removed later on
        public string GetError() { return m_strError; }
        public string GetTitle() { return m_Title; }
        public string GetDescription() { return m_Description; }
        public new string GetType() { return m_AddonType; }

        public Json(string title, string description, string type, List<string> tags, List<string> ignores)
        {
            m_Title = title;
            m_Description = description;
            m_AddonType = type;
            m_Tags = tags;
            m_Ignores = ignores;
        }

        public Json(string strInfoFile)
        {
            string strFileContents;

            //
            // Try to open the file
            //
            try
            {
                strFileContents = File.ReadAllText(strInfoFile);
            }
            catch (Exception)
            {
                m_strError = "Couldn't find file";
                return;
            }

            //
            // Parse the JSON
            //
            AddonJSON tree = new AddonJSON();
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(strFileContents));

            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(AddonJSON));
            try
            {
                tree = (AddonJSON)jsonFormatter.ReadObject(stream);
            }
            catch (SerializationException)
            {
                m_strError = "Couldn't parse json";
                return;
            }

            //
            // Check the title
            //
            m_Title = tree.Title;

            if (m_Title == String.Empty || m_Title == null)
            {
                m_strError = "title is empty!";
                return;
            }

            //
            // Get the description
            //
            m_Description = tree.Description;
            //
            // Load the addon type
            //
            m_AddonType = tree.Type;
            {
                m_AddonType = m_AddonType.ToLowerInvariant();

                if (m_AddonType == String.Empty || m_AddonType == null)
                {
                    m_strError = "type is empty!";
                    return;
                }

                //
                // Verify that the addon type is valid by checking it against the list of valids
                //
                if (!Tags.TypeExists(m_AddonType))
                {
                    m_strError = "type isn't a supported type!";
                    return;
                }
            }
            //
            // Parse the tags
            //
            {
                List<string> tags = tree.Tags;

                //
                // Max 2 tags
                //
                if (tags.Count > 2)
                {
                    m_strError = "too many tags - specify 2 only!";
                    return;
                }

                //
                // Collate and check the tags
                //
                foreach (string child in tags)
                {
                    if (child == String.Empty || child == null) continue;

                    m_Tags.Add(child.ToLowerInvariant());

                    if (!Tags.TagExists(child.ToLowerInvariant()))
                    {
                        m_strError = "tag isn't a supported word!";
                        return;
                    }
                }
            }
            //
            // Parse the ignores
            //
            List<string> ignores;
            if (tree.Ignore == null)
                ignores = new List<string>();
            else
                ignores = tree.Ignore;


            foreach (string child in ignores)
            {
                m_Ignores.Add(child);
            }
        }

        public void RemoveIgnoredFiles(ref List<string> files)
        {
            List<string> old_files = new List<string>(files);
            files.Clear();

            foreach (string f in old_files)
            {
                bool bSkipFile = false;

                //
                // Never include our json file!
                //
                if ( f == "addon.json" ) continue;

                //
                // Check against our loaded ignores list
                //
                foreach (string ignore in m_Ignores)
                {
                    if (Whitelist.TestWildcard(ignore, f))
                    {
                        bSkipFile = true;
                        break;
                    }
                }

                if (!bSkipFile)
                    files.Add(f);
            }
        }

        //
        // Build a JSON description to store in the GMA
        //
        public string BuildDescription()
        {
            DescriptionJSON tree = new DescriptionJSON();
            tree.Description = GetDescription();
            tree.Type = GetType();
            tree.Tags = new List<string>(m_Tags);
            string strOutput = String.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DescriptionJSON));
                try
                {
                    jsonFormatter.WriteObject(stream, tree);
                }
                catch (SerializationException)
                {
                    m_strError = "Couldn't parse json";
                    return String.Empty;
                }
                stream.Seek(0, SeekOrigin.Begin);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                strOutput = Encoding.ASCII.GetString(bytes);
            }

            //Console.Write("\n\n" + strOutput + "\n\n");
            return strOutput;
        }
    }
}
