using CtrlCV.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CtrlCV
{
    public partial class FrmPopup : Form
    {
        string[] notes;

        public FrmPopup()
        {
            InitializeComponent();
        }
        private void FrmPopup_Shown(object sender, EventArgs e)
        {
            txtSearch.Focus();
            txtSearch.Text = "";
            UpdateNotesList();
        }

        private void FrmPopup_Leave(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void FrmPopup_Deactivate(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void UpdateNotesList()
        {
            notes = Files.SearchNotes(txtSearch.Text);
            tvItems.Nodes.Clear();

            TreeNode bookmark = new TreeNode("Favoritos");
            TreeNode last_node = null;
            string last_dir = "";
            foreach (string n in notes)
            {
                var full_dir_name = Path.GetDirectoryName(n);
                var dir_name = Path.GetFileName(full_dir_name);
                var filename = Path.GetFileName(n);
                var filename_we = Path.GetFileNameWithoutExtension(filename);
                var is_root = (Path.GetFullPath(full_dir_name).TrimEnd('\\').Equals(Files.GetNoteFolder().TrimEnd('\\')));

                if ((last_node == null || !last_node.Text.Equals(dir_name)) && !is_root)
                {
                    last_node = new TreeNode(dir_name);
                    tvItems.Nodes.Add(last_node);
                }

                if (is_root)
                {
                    bookmark.Nodes.Add(new TreeNode(filename_we));
                }
                else
                {
                    last_node.Nodes.Add(new TreeNode(filename_we));
                }
            }
            if (bookmark.Nodes.Count > 0)
            {
                tvItems.Nodes.Insert(0, bookmark);
            }

            tvItems.ExpandAll();
            SelectUp();
        }
        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
                SelectDown();
            else if (e.KeyCode == Keys.Up)
                SelectUp();
            else if (e.KeyCode == Keys.Enter)
                PasteFileNode();
            else if (e.KeyCode == Keys.Escape)
                this.Close();
            else
                UpdateNotesList();
        }
        private void tvItems_DoubleClick(object sender, EventArgs e)
        {
            PasteFileNode();
        }
        List<TreeNode> GetAllChildren()
        {
            var list = new List<TreeNode>();

            foreach (TreeNode parent in tvItems.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                {
                    list.Add(child);
                }
            }

            return list;
        }
        void SelectUp()
        {
            var nodes = GetAllChildren();

            if (nodes.Count == 0)
                return;

            TreeNode current = tvItems.SelectedNode;

            if (current == null || current.Parent == null)
            {
                tvItems.SelectedNode = nodes[0];
                nodes[0].EnsureVisible();
                return;
            }

            int index = nodes.IndexOf(current) - 1;

            if (index < 0)
                index = nodes.Count - 1;

            tvItems.SelectedNode = nodes[index];
            nodes[index].EnsureVisible();
        }
        void SelectDown()
        {
            var nodes = GetAllChildren();

            if (nodes.Count == 0)
                return;

            TreeNode current = tvItems.SelectedNode;

            if (current == null || current.Parent == null)
            {
                tvItems.SelectedNode = nodes[0];
                nodes[0].EnsureVisible();
                return;
            }

            int index = nodes.IndexOf(current) + 1;

            if (index >= nodes.Count)
                index = 0;

            tvItems.SelectedNode = nodes[index];
            nodes[index].EnsureVisible();
        }
        private void PasteFileNode(bool show_message = false)
        {
            var n = tvItems.SelectedNode;
            if (n.Nodes.Count == 0)
            {
                var full_nodepath = (n.FullPath.StartsWith("Favoritos\\") ? n.FullPath.Substring(10) : n.FullPath);
                var filepath = Files.GetNoteFolder() + full_nodepath + ".txt";

                CtrlCV.Util.System.PasteFile(filepath);
                this.Hide();
                this.Dispose();
            }
            else if (show_message)
            {
                MessageBox.Show("Selecione um item");
            }
        }

        private void tvItems_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            bool selected = (e.Node == tvItems.SelectedNode);

            Color backColor = selected ? SystemColors.Highlight : tvItems.BackColor;
            Color foreColor = selected ? SystemColors.HighlightText : tvItems.ForeColor;

            using (SolidBrush back = new SolidBrush(backColor))
                e.Graphics.FillRectangle(back, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                tvItems.Font,
                e.Bounds,
                foreColor,
                TextFormatFlags.GlyphOverhangPadding
            );
        }

        private void tvItems_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                PasteFileNode();
            else if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
