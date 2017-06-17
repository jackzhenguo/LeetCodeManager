using LeetcodeManager.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeetcodeManager.Lib;
using LeetcodeManager.Controller;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;

namespace LeetcodeManager.View
{
    public partial class ProblemInputForm : Form
    {
        private Problem _problem;
        private UltraCombo ultraCombo1;
        private readonly ProblemInputController _problemController;
        private readonly TagInputController _tagController;
        public ProblemInputForm()
        {
            InitializeComponent();
            _problemController = new ProblemInputController();
            _tagController = new TagInputController();
            _problem = new Problem();
            ultraCombo1 = new UltraCombo();
            ultraCombo1.Location = new Point(titleTxtbox.Location.X, 12);
            ultraCombo1.Size = new Size(titleTxtbox.Size.Width, 21);
            Controls.Add(ultraCombo1);
            ultraCombo1.DataSource = _tagController.TagsToTable();
            fillUltraCombo();
            numericUpDown1.ReadOnly = false;
        }

        public ProblemInputForm(Problem problem)
            : this()
        {
            if(!string.IsNullOrEmpty(problem.Number))
               numericUpDown1.Value = Convert.ToDecimal(problem.Number);
            titleTxtbox.Text = problem.Title;
            LtCodeTxtbox.Text = problem.LtUrl;
            CsdnTxtbox.Text = problem.CsdnAddress;
            contentTxtbox.Text = problem.Content;
            numericUpDown1.ReadOnly = true;
            _problem = problem; //fix a bug
            //fill combox
            if(problem.Tags!=null)
            {
                foreach (var item in problem.Tags)
                {
                    foreach(var row in ultraCombo1.Rows)
                    {
                        if ((int)row.Cells["TagId"].Value == item.TagId)
                        {
                            row.Cells["Selected"].Value = true;
                            break;
                        }
                    }
                }
            }
        }
        public Problem InputProblem
        {
            get
            {
                return _problem;
            }
        }
        //ok
        private void button1_Click(object sender, EventArgs e)
        {
            _problem.Number = numericUpDown1.Value.ToString();
            if(_problem.ProblemId==0) //new a problem
            {
                //check if question with same number has existed
                var tmpprob = _problemController.GetAllProblems().FirstOrDefault(r => r.Number == _problem.Number);
                if(tmpprob!=null)
                {
                   SysHelper.ShowMessageWarning(string.Format("question with number of {0} has existed!",tmpprob.Number));
                    return;
                }
            }
            _problem.Title = titleTxtbox.Text;
            _problem.LtUrl = LtCodeTxtbox.Text;
            _problem.Content = contentTxtbox.Text;
            _problem.CsdnAddress = CsdnTxtbox.Text;
            if (SysHelper.CollectionNullOrEmpty<UltraGridRow>(this.ultraCombo1.CheckedRows))
            {
                MessageBox.Show("not select any tag! please create a new tag.");
                return;
            }
            if (_problem.Tags != null)
                _problem.Tags.Clear();
            else _problem.Tags = new List<Tag>();
            foreach (UltraGridRow row in this.ultraCombo1.CheckedRows)
            {              
                //if (!_problem.Tags.Contains(tag))
                int tagid = (int)(row.ListObject as DataRowView).Row.ItemArray[0];
                Tag retriveTag =_tagController.GetATagById(tagid);
                if(retriveTag!=null) //write to log
                  _problem.Tags.Add(retriveTag);
            }

            if (string.IsNullOrEmpty(_problem.Title))
            {
                SysHelper.ShowMessageWarning("Title cannot be empty!");
                return;
            }
            DialogResult = DialogResult.OK;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void fillUltraCombo()
        {
            //Add an additional unbound column to WinCombo.
            //This will be used for the Selection of each Item
            UltraGridColumn c = this.ultraCombo1.DisplayLayout.Bands[0].Columns.Add();
            ultraCombo1.DisplayLayout.Bands[0].Override.ActiveRowAppearance.BackColor = Color.Orange;
            ultraCombo1.DisplayLayout.Bands[0].Override.BorderStyleRow = UIElementBorderStyle.Dashed;

            c.Key = "Selected";
            c.Header.Caption = string.Empty;

            //This allows end users to select / unselect ALL items
            c.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;

            c.DataType = typeof(bool);

            //Move the checkbox column to the first position.
            c.Header.VisiblePosition = 0;

            this.ultraCombo1.CheckedListSettings.CheckStateMember = "Selected";
            this.ultraCombo1.CheckedListSettings.EditorValueSource = Infragistics.Win.EditorWithComboValueSource.CheckedItems;
            // Set up the control to use a custom list delimiter
            this.ultraCombo1.CheckedListSettings.ListSeparator = " / ";
            // Set ItemCheckArea to Item, so that clicking directly on an item also checks the item
            this.ultraCombo1.CheckedListSettings.ItemCheckArea = Infragistics.Win.ItemCheckArea.Item;
            this.ultraCombo1.DisplayMember = "Name";
            this.ultraCombo1.ValueMember = "TagId";

        }

    }
}
