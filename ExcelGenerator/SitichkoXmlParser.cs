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
            var tmp = new temp();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(fileInfo.FullName);
            Dictionary<long, questionTemp> qtmp = new Dictionary<long, questionTemp>();
            // получим корневой элемент
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                // цикл 1
                foreach (XmlElement xnode in xRoot) // checklist 
                {
                    if (xnode.Name == "checklist_group")
                        continue;

                    // цикл 2
                    foreach (XmlNode itemnode in xnode.ChildNodes) // item 0
                    {
                        var test = new Test();
                        
                        // цикл 3
                        foreach (XmlNode childnode in itemnode.ChildNodes) // id
                        {
                            if (xnode.Name == "checklist")
                            {
                                if (childnode.Name == "id")
                                {
                                    tmp.test_id = long.Parse(childnode.InnerText);
                                }
                                else if (childnode.Name == "name")
                                {
                                    test.Name = childnode.InnerText;
                                }
                                else if (childnode.Name == "description")
                                {
                                    test.Comment = childnode.InnerText;
                                }
                            }
                            else if (xnode.Name == "checklist_option") // внутренности чек-листов (вопрос\ож и т.д.)
                            {
                                if (childnode.Name == "id") // id вопроса
                                {
                                    tmp.question_id = long.Parse(childnode.InnerText);
                                    qtmp.Add(tmp.question_id, new questionTemp());
                                }
                                else if (childnode.Name == "action") // вопрос
                                {
                                    qtmp[tmp.question_id].Question = childnode.InnerText;
                                }
                                else if (childnode.Name == "result") // ожидаемый результат
                                {
                                    qtmp[tmp.question_id].ExpectedResult = childnode.InnerText;
                                    dtos.Add(test);
                                }
                            }// конец цикла 3
                            
                        }// конец цикла 2
                        
                    }// конец цикла 1
                    
                }


            }

            return dtos;
        }
    }

    internal class temp
    {
        public long test_id;
        public string name;
        public long question_id;
        public string description;
        public string abbreviation;
    }
    internal class questionTemp
    {
        public string Question { get; set; }
        public string ExpectedResult { get; set; }
    }
}
