using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model;

namespace XmlParser
{
    public static class SitichkoXmlParser
    {
        public static List<Test> Parse(FileInfo fileInfo)
        {
            var dtos = new List<Test>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(fileInfo.FullName);

            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                foreach (XmlElement xnode in xRoot)
                {
                    foreach (XmlNode tempnode in xnode.ChildNodes)
                    {
                        var test = new Test();
                        foreach (XmlNode childnode in tempnode.ChildNodes)
                        {
                            if (childnode.Name == "id")
                            {
                                // TODO
                            }
                            else if (childnode.Name == "project_id")
                            {
                                // TODO
                            }
                            else if (childnode.Name == "name")
                            {
                                test.Name = childnode.InnerText;
                                test.
                            }
                            else if (childnode.Name == "description")
                            {
                                test.Comment = childnode.InnerText;
                            }
                            else if (childnode.Name == "abbreviation")
                            {
                                // TODO
                            }
                            else if (childnode.Name == "regularity_id")
                            {
                                // TODO
                            }
                            else if (childnode.Name == "completed")
                            {
                                // TODO
                            }
                        }
                    }
                }
            }
        }
    }

    internal class checklistDto
    {
        public long id;
        public long project_id;
        public string name;
        public string description;
        public string abbreviation;
        public int regularity_id;
        public int completed;
    }
}
