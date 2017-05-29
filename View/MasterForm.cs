using LeetcodeManager.Controller;
using LeetcodeManager.Entity;
using LeetcodeManager.Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeetcodeManager.View
{
    public partial class MasterForm : Form
    {
        private readonly ProblemInputController _problemController;
        private readonly TagInputController _tagController;
        public MasterForm()
        {
            InitializeComponent();
            _problemController = new ProblemInputController();
            _tagController = new TagInputController();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //binding to tree
            var tags = _tagController.GetAllTags();
            TreeNode root = treeViewTag.Nodes[0];
            if (tags == null) return;
            int index=0;
            foreach (var tag in tags)
            {
                TreeNode node = new TreeNode(tag.Name) { Tag = tag,ImageIndex =(index++)%5  };
                root.Nodes.Add(node);
            }
            treeViewTag.ExpandAll();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //MyDb: dispose
        }

        private void newTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var input = new TagInputForm();
            if (input.ShowDialog() != DialogResult.OK)
                return;
            var tag = new Tag() { Name = input.InputTag };
            var tagdb = _tagController.GetATagByName(input.InputTag);
            if (!_tagController.IsNew(tagdb))
            {
                SysHelper.ShowMessageWarning("Existed tag with this name, create failure!");
                return;
            }
            tagdb =_tagController.SaveTagToDb(tag);
            treeViewTag.Nodes[0].Nodes.Add(new TreeNode(input.InputTag) { Tag = tagdb });
            treeViewTag.ExpandAll();
        }

        private void updateTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewTag.SelectedNode.Name == "rootNode")
            {
                MessageBox.Show("Root name cannot be revised!");
                return;
            }
            var tagdel = treeViewTag.SelectedNode.Tag as Tag;
            if (tagdel == null) return;
            var rettag = _tagController.GetATagById(tagdel.TagId);
            if (rettag == null)
            {
                SysHelper.ShowMessageWarning("Unable to retrive from db!");
                return;
            }
            var input = new TagInputForm(tagdel.Name);
            if (input.ShowDialog() != DialogResult.OK)
                return;
            rettag.Name = input.InputTag;
            _tagController.UpdateTagName(rettag);
            treeViewTag.SelectedNode.Text = input.InputTag;
            treeViewTag.ExpandAll();
        }

        private void deleteTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewTag.SelectedNode.Name == "rootNode")
            {
                MessageBox.Show("Root of tree cannot be deleted!");
                return;
            }
            try
            {
                var tagdel = treeViewTag.SelectedNode.Tag as Tag;
                if (tagdel == null) return;
                var rettag = _tagController.GetATagById(tagdel.TagId);
                if (rettag == null)
                {
                    SysHelper.ShowMessageWarning("Unable to retrive from db!");
                    return;
                }
                _tagController.DeleteTag(rettag);
                treeViewTag.SelectedNode.Remove();
                SysHelper.ShowMessageOK("deletion success");
            }
            catch (Exception ex)
            {
                MessageBox.Show("deletion failure"+"("+ex.Message+")");
            }
            treeViewTag.ExpandAll();
        }

        private void treeViewTag_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Tag tag = e.Node.Tag as Tag;
            if (tag == null)
                return;
            if (SysHelper.CollectionNullOrEmpty<Problem>(tag.Problems))
            {
                problemBindingSource.DataSource = tag.Problems;
                return;
            }
            problemBindingSource.DataSource = tag.Problems.OrderBy(r => Convert.ToInt32(r.Number)).ToList();
            problemDataGridView.Refresh();
        }

        //save question
        private void problemBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            foreach (DataGridViewRow row in problemDataGridView.Rows)
            {
                Problem obj = row.DataBoundItem as Problem;
                if (obj == null) continue;
                if (string.IsNullOrEmpty(obj.Title)) continue;
                _problemController.SaveProblem(obj);
            }
            _problemController.UpdateProblems(); //inlcude delete(this is the ability EF provides) 
            problemDataGridView.Refresh();
        }

        private void toolStripButtonDelAll_Click(object sender, EventArgs e)
        {
            this.Validate();
            if (DialogResult.No == SysHelper.ShowMessageYesOrNo("Are you sure to delete all problems in this grid?"))
                return;           
            IList<Problem> deletes = new List<Problem>();
            foreach (DataGridViewRow row in problemDataGridView.Rows)
            {
                var problem = row.DataBoundItem as Problem;
                if (problem != null && !_problemController.IsNew(problem))
                    deletes.Add(problem);
            }
            problemBindingSource.DataSource = new List<Problem>();
            _problemController.DeleteProblems(deletes);
            SysHelper.ShowMessageOK("Delete all Okay!");
        }

        private void deleteALLLocalDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.No == MessageBox.Show("Are you sure to delete all data in database?", "LeetCodeManager", MessageBoxButtons.YesNo))
                return;
            _problemController.DeleteALLProblems();
            problemDataGridView.DataSource = new List<Problem>();
            _tagController.DeleteAllTags();
            treeViewTag.Nodes[0].Remove();
            treeViewTag.Nodes.Add(new TreeNode("Tag") { Name = "rootNode" });
        }
        //New
        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            ProblemInputForm problemInput = new ProblemInputForm();
            int rowscnt = problemDataGridView.Rows.Count;
            if (problemInput.ShowDialog() == DialogResult.Cancel)
            {
                if(rowscnt-1>=0)
                   problemDataGridView.Rows.RemoveAt(rowscnt - 2);
                return;
            }
            Problem newproblem = problemInput.InputProblem;
            int cnt = problemDataGridView.Rows.Count;
            DataGridViewRow fillrow = problemDataGridView.Rows[cnt - 2];
            fillrow.Cells[1].Value = newproblem.Number;
            fillrow.Cells[2].Value = newproblem.Title;
            fillrow.Cells[3].Value = newproblem.LtUrl;
            fillrow.Cells[4].Value = newproblem.CsdnAddress;
            fillrow.Cells[6].Value = newproblem.Content;
            IEnumerable<string> tagnames = newproblem.Tags.Select(r => r.Name);
            string combinestr = string.Empty;
            foreach (var str in tagnames) combinestr += str + ";";
            fillrow.Cells[5].Value = combinestr;
            fillrow.Cells[7].Value = newproblem.Tags;
            problemBindingNavigatorSaveItem_Click(problemDataGridView, null);
        }

        //Edit
        private void toolStripButtonEdit_Click(object sender, EventArgs e)
        {
            var selectrows = problemDataGridView.SelectedRows;
            if(selectrows==null || selectrows.Count==0)
            {
                SysHelper.ShowMessageWarning("unselect any row, please select at least one row!");
                return;
            }
            foreach(DataGridViewRow row in selectrows)
            {
                var frm = new ProblemInputForm(row.DataBoundItem as Problem);
                if (frm.ShowDialog() == DialogResult.Cancel)
                    continue;
                Problem newproblem = frm.InputProblem;
                row.Cells[1].Value = newproblem.Number;
                row.Cells[2].Value = newproblem.Title;
                row.Cells[3].Value = newproblem.LtUrl;
                row.Cells[4].Value = newproblem.CsdnAddress;
                row.Cells[6].Value = newproblem.Content;
                IEnumerable<string> tagnames = newproblem.Tags.Select(r => r.Name);
                string combinestr = string.Empty;
                foreach (var str in tagnames) combinestr += str + ";";
                row.Cells[5].Value = combinestr;
                row.Cells[7].Value = newproblem.Tags;
            }
            _problemController.UpdateProblems();
        }

        private void problemDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 3 || e.ColumnIndex > 4) return;
                if (e.RowIndex < 0) return;
                System.Diagnostics.Process.Start(problemDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }
           catch(Exception ex)
            {
                SysHelper.ShowMessageWarning(ex.Message);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var problems = problemBindingSource.DataSource as List<Problem>;
            if (toolStripButton1.Text == "SortAsc")
            {
                problems = problems.OrderBy(r => Convert.ToInt32(r.Number)).ToList();
                toolStripButton1.Text = "SortDesc";
            }
            else
            {
                problems = problems.OrderByDescending(r => Convert.ToInt32(r.Number)).ToList();
                toolStripButton1.Text = "SortAsc";
            }
            problemBindingSource.DataSource = problems;
            problemDataGridView.Refresh();
            
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            problemBindingSource.DataSource = _problemController.GetAllProblems().OrderBy(r => Convert.ToInt32(r.Number)).ToList();
            problemDataGridView.Refresh();
        }
        //import problems by a file that has a certain format
        private void templeImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //35|[Search Insert Position](http://blog.csdn.net/daigualu/article/details/66995617)
            //118| [Pascal's Triangle](http://blog.csdn.net/daigualu/article/details/67006388)
            if (SysHelper.ShowMessageYesOrNo("Are you sure to import txt with same name for selected node?") == DialogResult.No)
                return;
            try
            {
                var tag = treeViewTag.SelectedNode.Tag as Tag;
                if (tag == null)
                {
                    SysHelper.ShowMessageWarning("please select a non-root node!");
                    return;
                }
                IndexController ic = new IndexController();
                var list = ic.ConvertToProblemsByImportTemplate(tag, tag.Name);
                foreach (var item in list)
                {
                    _problemController.SaveProblem(item);
                }
                SysHelper.ShowMessageOK("Import Okay!");
            }
            catch(Exception ex)
            {
                SysHelper.ShowMessageWarning(ex.Message);
            }

        }

        private void exportTemplateToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }  

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            _problemController.UpdateProblems(); //inlcude delete(this is the ability EF provides) 
            problemDataGridView.Refresh();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(toolStripTextBox1.Text))
            {
                if(treeViewTag.SelectedNode.Tag!=null)
                {
                    problemBindingSource.DataSource = (treeViewTag.SelectedNode.Tag as Tag).Problems.OrderBy(r=>Convert.ToInt32(r.Number)).ToList();
                    problemDataGridView.Refresh();
                }
                else
                {
                    problemBindingSource.DataSource = _problemController.GetAllProblems().OrderBy(r => Convert.ToInt32(r.Number)).ToList();
                    problemDataGridView.Refresh();
                }
                return;
            }
            Problem searchedProb = _problemController.GetAProblemByNumber(toolStripTextBox1.Text);
            if (searchedProb != null)
            {
                problemBindingSource.DataSource = new List<Problem>() { searchedProb };
                problemDataGridView.Refresh();
            }

        }


    }
}
