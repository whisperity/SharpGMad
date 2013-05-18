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
    class DescriptionJSON
    {
        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "tags")]
        public List<string> Tags;
    }

    [DataContract]
    class AddonJSON
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
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Type { get; private set; }
        private List<string> _Ignores;
        public List<string> Ignores { get { return new List<string>(_Ignores); } }
        private List<string> _Tags;
        public List<string> Tags { get { return new List<string>(_Tags); } }

        public Json(string infoFile)
        {
            _Ignores = new List<string>();
            _Tags = new List<string>();
            string fileContents;
            
            // Try to open the file
            try
            {
                fileContents = File.ReadAllText(infoFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't find file", ex);
            }

            // Parse the JSON
            AddonJSON tree = new AddonJSON();
            using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(fileContents)))
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(AddonJSON));

                try
                {
                    tree = (AddonJSON)jsonFormatter.ReadObject(stream);
                }
                catch ( SerializationException ex)
                {
                    throw new Exception("Couldn't parse json",ex);
                }
            }

            // Check the title
            if ( tree.Title == String.Empty || tree.Title == null )
                throw new Exception("title is empty!");
            else
                Title = tree.Title;

            // Get the description
            Description = tree.Description;

            // Load the addon type
            if ( tree.Type.ToLowerInvariant() == String.Empty || tree.Type.ToLowerInvariant() == null )
                throw new Exception("type is empty!");
            else
            {
                if (!SharpGMad.Tags.TypeExists(tree.Type.ToLowerInvariant()))
                    throw new Exception("type isn't a supported type!");
                else
                    Type = tree.Type.ToLowerInvariant();
            }

            // Parse the tags
            if ( tree.Tags.Count > 2 )
                throw new Exception("too many tags - specify 2 only!");
            else
            {
                foreach ( string tag in tree.Tags)
                {
                    if ( tag == String.Empty || tag == null ) continue;

                    if ( !SharpGMad.Tags.TagExists(tag.ToLowerInvariant()))
                        throw new Exception("tag isn't a supported word!");
                    else
                        _Tags.Add(tag.ToLowerInvariant());
                }
            }

            // Parse the ignores
            if ( tree.Ignore != null )
                _Ignores.AddRange(tree.Ignore);
        }

        public Json(string title, string description, string type, List<string> tags, List<string> ignores)
        {
            Title = title;
            Description = description;
            Type = type;
            _Tags = new List<string>(tags);
            _Ignores = new List<string>(ignores);
        }

        //
        // Build a JSON description to store in the GMA
        //
        public static string BuildDescription(Addon a)
        {
            DescriptionJSON tree = new DescriptionJSON();
            tree.Description = a.Description;
            tree.Type = a.Type;
            tree.Tags = a.Tags;

            string strOutput;

            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DescriptionJSON));
                
                try
                {
                    jsonFormatter.WriteObject(stream, tree);
                }
                catch (SerializationException ex)
                {
                    throw new Exception("Couldn't parse json", ex);
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
