using CtrlCV.Collections;
using CtrlCV.Util;
using CtrlCV.ValueObjects;
using System.Diagnostics;

namespace CtrlCV
{
    public partial class FrmPopup : Form
    {
        private bool openwith = false;

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
            var notes = NoteCollection.Singleton.Contains(txtSearch.Text);
            tvItems.BeginUpdate();
            tvItems.Nodes.Clear();

            TreeNode bookmark = new TreeNode("Favoritos");
            TreeNode last_node = null;
            foreach (var n in notes)
            {
                if ((last_node == null || !last_node.Text.Equals(n.GetDirectoryName())) && !n.IsRootPath)
                {
                    last_node = new TreeNode(n.GetDirectoryName());
                    tvItems.Nodes.Add(last_node);
                }

                var tn = new TreeNode(n.GetFileNameTreeView());
                tn.Tag = n;
                if (n.IsRootPath)
                {
                    bookmark.Nodes.Add(tn);
                }
                else
                {
                    last_node.Nodes.Add(tn);
                }
            }
            if (bookmark.Nodes.Count > 0)
            {
                tvItems.Nodes.Insert(0, bookmark);
            }

            if (bookmark.Nodes.Count > 0 && txtSearch.Text.Trim().Length == 0)
            {
                bookmark.Expand();
            } else
            {
                tvItems.ExpandAll();
            }
            tvItems.EndUpdate();
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

            if (e.KeyCode == Keys.ShiftKey)
            {
                OpenWith(false);
            }
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
            var node = tvItems.SelectedNode;
            if (node.Nodes.Count == 0)
            {
                if (node.Tag == null) return;

                Note note = (Note) node.Tag;

                if (openwith)
                {
                    Execute.OpenWith(note.FullPath);
                }
                else if (note.AllowPast())
                {
                    CtrlCV.Util.System.PasteFile(note.FullPath);
                    return;
                }
                else
                {
                    Execute.Open(note.FullPath);
                }

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

            if (e.KeyCode == Keys.ShiftKey)
            {
                OpenWith(false);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            UpdateNotesList();
        }

        private void tvItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                OpenWith(true);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                OpenWith(true);
            }
        }

        private void OpenWith(bool enable)
        {
            if (enable)
            {
                openwith = true;
                this.Text = "Busca (Abrir com...)";
            } else
            {
                openwith = false;
                this.Text = "Busca";
            }

        }
    }
}
