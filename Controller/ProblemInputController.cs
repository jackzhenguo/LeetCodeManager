using LeetcodeManager.Controller;
using LeetcodeManager.DAL;
using LeetcodeManager.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeetcodeManager.Controller
{
    public class ProblemInputController
    {
        private readonly TagDAL _tagDal = new TagDAL();
        private readonly ProblemDAL _problemDal = new ProblemDAL();

        public bool IsNew(Problem problem)
        {
            if (problem == null) return true;
            return _problemDal.IsNew(problem);
        }
        public Problem SaveProblem(Problem problem)
        {
            return _problemDal.CreateAProblem(problem);
        }
        public Problem AddARelation(Problem p,Tag t)
        {
            Tag tt = p.Tags.FirstOrDefault(r => r.Name == t.Name);
            if (tt != null) return p;
            p.Tags.Add(t);
            UpdateProblems();
            return p;
        }
        public void UpdateProblems()
        {
            _problemDal.UpdateProblems();
        }
        public void DeleteProblem(Problem problem)
        {
            _problemDal.DeleteAProblem(problem);
        }
        public void DeleteProblems(IList<Problem> problems)
        {
            _problemDal.DeleteProblems(problems);
        }

        public void DeleteALLProblems()
        {
            DeleteProblems(_problemDal.QueryAllProblems().ToList());
        }

        public IList<Problem> GetAllProblems()
        {
           var enumable = _problemDal.QueryAllProblems();
           return enumable.ToList();       
        }

        public Problem GetAProblemByNumber(string number)
        {
            return GetAllProblems().FirstOrDefault(r => r.Number == number);
        }
        
    }
}
