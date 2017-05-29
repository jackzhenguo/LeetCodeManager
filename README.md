# LeetCodeManager
This is a manager for leetcode issues based on Entity Framework, SQL Server and WinForm.

## Readers
 - If you are interested and ready to get started for Entity Framework, esp many-to-many relationship issue;
 - If you are lost in management for Leetcode problems you have done;
 - If you want to study a new ORM Framework;
 - If you find a or many bugs;
 -  If you want to give me a suggestion
 
 ## Install
 - Visual Studio 2013
 - .NET Framework 4.0+
 - Nuget install, furthermore Entity framework.

## BackGround
This article is a practical description of the problem, which uses the Entity Framework, based on WinForm data binding technology, and the SQL Server database to manage LeetCode problems that we have done. **LeetCode manager** will manage problems according to  "**LeetCode tags**", such as *Array*, *HashTable*, *Stack*, *Tree*, *Graph* etc... Under each category problem can be created, deleted, modified, and one problem may be associated with multiple tags, so this is a typical instance used to solve many-to-many relationship.

![这里写图片描述](http://img.blog.csdn.net/20170529222843520?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGFpZ3VhbHU=/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)

## Software Structure

![这里写图片描述](http://img.blog.csdn.net/20170529223348183?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGFpZ3VhbHU=/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)  

The main GUI of software is as follows, its main structure includes:

 - **Entity;**
 -  **DAL;**
 - **Controller;**
 - **View;**

Also include other folders:

 - **DataMd** is a template file that imports problems we have done;
 - **Lib** is the help object of software;
 - **Migrations** are done with the package management console after making changes to the entity's attributes;
 - **Assembly**: third-party control: Infragistics UltraCombo control.

## Details
Now, Let's introduce the folders we have involved above in detail.
### Entity
Building entity is based on Code First of EF. Entity folder includes following files: Problem entity and Tag entity.
Problem Entity: 
```C#
    public class Problem
    {
        public int ProblemId { get; set; }
        [MaxLength(50)]
        [Required(ErrorMessage = "Title is required.")]
        public string Number { get; set; }
        public string Title { get; set; }
        public string LtUrl { get; set; }
        public string CsdnAddress { get; set; }
        public string Content { get; set; }
        public string TagsCombine { get; set; }//it's separated by ';'
        // Tags is the navigation property
        public virtual IList<Tag> Tags { get; set; } //it's a navigation property
    }
```
Tag entity:

```C#
    public class Tag
    {
        public int TagId { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
        //Problems is the navigation property
        public virtual IList<Problem> Problems { get; set; }
    }
```
### DAL
DAL is the abbreviation of Data Access Layer, and we can simplify the code with EF. Now, we look at DAL objects.
First, it is MyDb object, which is inherited from ContextDb object: 

```C#
    public class MyDb:DbContext
    {
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Problem> Problems { get; set; }
        //many-to-many relationship building:
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>().HasMany(p => p.Problems).WithMany(r => r.Tags).Map(
                t => t.ToTable("TagProblem").
                    MapLeftKey("TagId").
                    MapRightKey("ProblemId"));
        }
    }
```
Next, let's create an instance of **MyDb**: _db, remember it's only one instance for operating Database.

```C#
    public class BaseDAL
    {
        protected static MyDb _db;
        protected BaseDAL()
        {
            _db = _db ?? new MyDb();
        }
    }
```
Next, we create an object named ProblemDAL, which inherited from BaseDAL and it mainly solves the issues about problem data access database. It's structure is following: 

![这里写图片描述](http://img.blog.csdn.net/20170529222357639?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGFpZ3VhbHU=/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)
```C#
public class ProblemDAL:BaseDAL
    {
        public IEnumerable<Tag> QueryAllTags()
        {
            DbSet<Tag> tags = _db.Tags;
            if (tags == null) return new List<Tag>();
            return tags.AsEnumerable<Tag>();
        }

        public IEnumerable<Problem> QueryAllProblems()
        {
            DbSet<Problem> problems = _db.Problems;
            if (problems == null) return new List<Problem>();
            return problems.AsEnumerable<Problem>();
        }

        public bool IsNew(Problem problem)
        {
            return problem.ProblemId == 0;
        }
        
        public Problem CreateAProblem(Problem problem)
        {
            if (!IsNew(problem)) return problem;
            _db.Problems.Add(problem);
            _db.SaveChanges();
            var problems = _db.Problems.OrderByDescending(r => r.ProblemId).AsEnumerable<Problem>();
            if (!SysHelper.CollectionNullOrEmpty<Problem>(problems))
                return problems.First();
            return new Problem();
        }
        
        public void UpdateProblems()
        {
            _db.SaveChanges();
        }

        public void DeleteAProblem(Problem problem)
        {
            _db.Problems.Remove(problem);
            _db.SaveChanges();
        }

        public void DeleteProblems(IList<Problem> problems)
        {
            int cnt = problems.Count();
            for(int i=0;i<cnt;i++)
            {
                if(_db.Problems.Contains(problems[i]))
                   _db.Problems.Remove(problems[i]);
            }
            _db.SaveChanges();
        }

    }
```
Similarly, we have the TagDAL object:

```C#
public class TagDAL:BaseDAL
    {
        public IEnumerable<Tag> QueryAllTags()
        {
            DbSet<Tag> tags = _db.Tags;
            if (tags == null) return new List<Tag>();
            return tags.AsEnumerable<Tag>();
        }
        
        public Tag QueryTagByName(string name)
        {
            DbSet<Tag> tags = _db.Tags;
            if (tags == null) return new Tag();
            return tags.FirstOrDefault(r => r.Name == name);
        }

        public Tag QueryTagById(int id)
        {
            DbSet<Tag> tags = _db.Tags;
            if (tags == null) return new Tag();
            return tags.FirstOrDefault(r => r.TagId == id);
        }

        public bool IsNew(Tag tag)
        {
            return tag.TagId == 0;
        }

        public Tag CreateATag(Tag tag)
        {
            if (!IsNew(tag)) return tag;
            _db.Tags.Add(tag);
            _db.SaveChanges();
            var tags = _db.Tags.OrderByDescending(r => r.TagId).AsEnumerable<Tag>();
            if (!SysHelper.CollectionNullOrEmpty<Tag>(tags))
                return tags.First();
            return new Tag();
        }

        public void DeleteATag(Tag tag)
        {
            _db.Tags.Remove(tag);
            _db.SaveChanges();
        }

        public void UpdateATagName(Tag tag)
        {
            Tag tagdb = _db.Tags.FirstOrDefault(r => r.TagId == tag.TagId);
            if (tag != null)
                tagdb.Name = tag.Name;
            _db.SaveChanges();
        }
    }
```
### Controller
Here, controllers are not same to the controllers of MVC, but it's a bit of similar to that of MVC. It is the middle layer between DAL and View layer. I donot want to write it in detail here and you could look at it if you have some interest in it. Following is its structure: 

![这里写图片描述](http://img.blog.csdn.net/20170529222303287?watermark/2/text/aHR0cDovL2Jsb2cuY3Nkbi5uZXQvZGFpZ3VhbHU=/font/5a6L5L2T/fontsize/400/fill/I0JBQkFCMA==/dissolve/70/gravity/SouthEast)

### View
Views are mainly including Index form, Tag input form, Problem input form. Here, I don't plan to write it in detail, because this is not the key point for this passage. 
## Main Function
LeetcodeManager mainly manage to the Tag and Problems on the management of many-to-many relationship, including the Tag of the creation, deletion, modification; Problem of the creation, deletion, modification. In addition, problems sort in accordance with the number of problem in an ascending and descending order, and  can be filtered by the number of problem. Also a click can clear all the data in database; we have quickly import problems in accordance with a fixed format on the problems.
## Next work
Asp.net version is implemented.
## Fork and Star
https://github.com/jackzhenguo/LeetCodeManager


