using LeetcodeManager.Entity;
using LeetcodeManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetcodeManager.Controller
{
    public class IndexController
    {
        public List<Problem> ConvertToProblemsByImportTemplate(Tag t,string name)
        {
            {
                //35|[Search Insert Position](http://blog.csdn.net/daigualu/article/details/66995617)
                //118| [Pascal's Triangle](http://blog.csdn.net/daigualu/article/details/67006388)
                string str = SysHelper.ReadFile(Environment.CurrentDirectory, string.Format(@"DataMd\{0}.txt",name));
                if (string.IsNullOrEmpty(str))
                    throw new Exception("File not found!");
                string[] strs = str.Split('\n');
                StringBuilder sb = new StringBuilder();
                List<Problem> rtnlist = new List<Problem>();
                ProblemInputController probc = new ProblemInputController();
                foreach (var item in strs)
                {
                    Problem p = new Problem();
                    string[] itemsplits = item.Split('|');
                    if(itemsplits.Length<2)
                    {
                        sb.Append(string.Format("format error!\n", p.Number));
                        continue;
                    }
                    p.Number = itemsplits[0].Trim();
                    //check if question with same number has existed
                    Problem oldp = probc.GetAllProblems().FirstOrDefault(r => r.Number == p.Number);
                    if (oldp != null) 
                    {
                        if (oldp.Tags.Where(r => r.Name == t.Name).Count() == 0) //if oldp.Tags not including this tag t
                        {
                            oldp.TagsCombine += t.Name + ";";
                            probc.AddARelation(oldp, t);
                            continue;
                        }
                        else continue;
                    }
                    string titurl = itemsplits[1].Trim();
                    string[] titurls = titurl.Split(']');
                    if (titurls.Length < 2)
                    {
                        sb.Append(string.Format("number({0})'s title is not found!\n", p.Number));
                        continue;
                    }
                    p.Title= titurls[0].Substring(1);
                    p.CsdnAddress = titurls[1].Substring(1, titurls[1].Length - 2);
                    p.TagsCombine += t.Name + ";";
                    p.Tags = new List<Tag>() { t };
                    rtnlist.Add(p);
                }
                if (sb.Length > 0)
                    SysHelper.ShowMessageWarning(sb.ToString());
                return rtnlist;
            }
        }
    }
}
