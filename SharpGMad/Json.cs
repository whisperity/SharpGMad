using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SharpGMad
{
    /// <summary>
    /// Represents the GMA's embedded JSON field.
    /// </summary>
    [DataContract]
    class DescriptionJSON
    {
        /// <summary>
        /// Gets or sets the description of the addon.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description;

        /// <summary>
        /// Gets or sets the type of the addon.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type;

        /// <summary>
        /// Contains a list of strings, the tags of the addon.
        /// </summary>
        [DataMember(Name = "tags")]
        public List<string> Tags;
    }

    /// <summary>
    /// Represents the addon metadata declaring addon.json file.
    /// </summary>
    [DataContract]
    class AddonJSON : DescriptionJSON
    {
        // Description, Type and Tags is inherited.

        /// <summary>
        /// Gets or sets the title of the addon.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title;

        /// <summary>
        /// Contains a list of string, the ignore patterns of files that should not be compiled.
        /// </summary>
        [DataMember(Name = "ignore")]
        public List<string> Ignore;
    }

    /// <summary>
    /// The exception thrown when the JSON file read/write encounters an error.
    /// </summary>
    [Serializable]
    public class AddonJSONException : Exception
    {
        public AddonJSONException() { }
        public AddonJSONException(string message) : base(message) { }
        public AddonJSONException(string message, Exception inner) : base(message, inner) { }
        protected AddonJSONException(
          SerializationInfo info,
          StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Provides methods to parse and create addon metadata JSONs.
    /// </summary>
    class Json
    {
        /// <summary>
        /// Gets the title from the read JSON.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Gets the description from the read JSON.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Gets the addon type from the read JSON.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Contains a list of strings, the ignore patterns of files that should not be compiled.
        /// </summary>
        private List<string> _Ignores;
        /// <summary>
        /// Gets a list of ignored file patterns.
        /// </summary>
        public List<string> Ignores { get { return new List<string>(_Ignores); } }
        /// <summary>
        /// Contains a list of strings, the tags of the addon.
        /// </summary>
        private List<string> _Tags;
        /// <summary>
        /// Gets a list of addon tags.
        /// </summary>
        public List<string> Tags { get { return new List<string>(_Tags); } }

        /// <summary>
        /// Initializes a JSON reader instance, reading the specified file.
        /// </summary>
        /// <param name="infoFile">The addon.json file to read.</param>
        /// <exception cref="AddonJSONException">Errors regarding reading/parsing the JSON.</exception>
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
            catch (IOException ex)
            {
                throw new AddonJSONException("Couldn't find file", ex);
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
                catch (SerializationException ex)
                {
                    throw new AddonJSONException("Couldn't parse json", ex);
                }
            }

            // Check the title
            if (tree.Title == String.Empty || tree.Title == null)
                throw new AddonJSONException("title is empty!");
            else
                Title = tree.Title;

            // Get the description
            Description = tree.Description;

            // Load the addon type
            if (tree.Type.ToLowerInvariant() == String.Empty || tree.Type.ToLowerInvariant() == null)
                throw new AddonJSONException("type is empty!");
            else
            {
                if (!SharpGMad.Tags.TypeExists(tree.Type.ToLowerInvariant()))
                    throw new AddonJSONException("type isn't a supported type!");
                else
                    Type = tree.Type.ToLowerInvariant();
            }

            // Parse the tags
            if (tree.Tags.Count > 2)
                throw new AddonJSONException("too many tags - specify 2 only!");
            else
            {
                foreach (string tag in tree.Tags)
                {
                    if (tag == String.Empty || tag == null) continue;

                    if (!SharpGMad.Tags.TagExists(tag.ToLowerInvariant()))
                        throw new AddonJSONException("tag isn't a supported word!");
                    else
                        _Tags.Add(tag.ToLowerInvariant());
                }
            }

            // Parse the ignores
            if (tree.Ignore != null)
                _Ignores.AddRange(tree.Ignore);
        }
        
        /// <summary>
        /// Creates a JSON string using the properties of the provided Addon.
        /// </summary>
        /// <param name="addon">The addon which metadata is to be used.</param>
        /// <returns>The compiled JSON string.</returns>
        /// <exception cref="AddonJSONException">Errors regarding creating the JSON.</exception>
        public static string BuildDescription(Addon addon)
        {
            DescriptionJSON tree = new DescriptionJSON();
            tree.Description = addon.Description;

            // Load the addon type
            if (addon.Type.ToLowerInvariant() == String.Empty || addon.Type.ToLowerInvariant() == null)
                throw new AddonJSONException("type is empty!");
            else
            {
                if (!SharpGMad.Tags.TypeExists(addon.Type.ToLowerInvariant()))
                    throw new AddonJSONException("type isn't a supported type!");
                else
                    tree.Type = addon.Type.ToLowerInvariant();
            }

            // Parse the tags
            tree.Tags = new List<string>();
            if (addon.Tags.Count > 2)
                throw new AddonJSONException("too many tags - specify 2 only!");
            else
            {
                foreach (string tag in addon.Tags)
                {
                    if (tag == String.Empty || tag == null) continue;

                    if (!SharpGMad.Tags.TagExists(tag.ToLowerInvariant()))
                        throw new AddonJSONException("tag isn't a supported word!");
                    else
                        tree.Tags.Add(tag.ToLowerInvariant());
                }
            }

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
                    throw new AddonJSONException("Couldn't create json", ex);
                }

                stream.Seek(0, SeekOrigin.Begin);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                strOutput = Encoding.ASCII.GetString(bytes);
            }

            return strOutput;
        }
    }
}