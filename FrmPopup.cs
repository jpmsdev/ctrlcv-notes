using CtrlCV.Util;
using System.Diagnostics;

namespace CtrlCV
{
    public partial class FrmPopup : Form
    {
        private string[] notes = {};
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
            notes = Files.SearchNotes(txtSearch.Text);
            tvItems.BeginUpdate();
            tvItems.Nodes.Clear();

            TreeNode bookmark = new TreeNode("Favoritos");
            TreeNode last_node = null;
            foreach (string n in notes)
            {
                var full_dir_name = Path.GetDirectoryName(n);
                var dir_name = Path.GetFileName(full_dir_name);
                var filename = Path.GetFileName(n);
                var extension = Path.GetExtension(filename);
                var filename_tn = (extension.Equals(".txt")) ? Path.GetFileNameWithoutExtension(filename) : filename;
                var is_root = (Path.GetFullPath(full_dir_name).TrimEnd('\\').Equals(Files.GetNoteFolder().TrimEnd('\\')));

                if ((last_node == null || !last_node.Text.Equals(dir_name)) && !is_root)
                {
                    last_node = new TreeNode(dir_name);
                    tvItems.Nodes.Add(last_node);
                }

                if (is_root)
                {
                    bookmark.Nodes.Add(new TreeNode(filename_tn));
                }
                else
                {
                    last_node.Nodes.Add(new TreeNode(filename_tn));
                }
            }
            if (bookmark.Nodes.Count > 0)
            {
                tvItems.Nodes.Insert(0, bookmark);
            }

            tvItems.ExpandAll();
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
            var n = tvItems.SelectedNode;
            if (n.Nodes.Count == 0)
            {
                var full_nodepath = (n.FullPath.StartsWith("Favoritos\\") ? n.FullPath.Substring(10) : n.FullPath);
                var extension = Path.GetExtension(full_nodepath);
                var filepath = Files.GetNoteFolder() + full_nodepath;

                if (string.IsNullOrEmpty(extension))
                {
                    filepath += ".txt";
                    if (!openwith)
                    {
                        CtrlCV.Util.System.PasteFile(filepath);
                        return;
                    }
                }

                if (openwith)
                {
                    Process.Start("rundll32.exe", $"shell32.dll,OpenAs_RunDLL {filepath}");
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $@"{filepath}",
                        UseShellExecute = true
                    });
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
