using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace WindowsFormsAero
{
    [System.ComponentModel.DesignerCategory("code")]
    [System.ComponentModel.Designer("WindowsFormsAero.Design.AeroTabControlDesigner, " + ThisAssembly.DesignAssemblyFullName)]
    [DefaultProperty("TabPages")]
    [DefaultEvent("SelectedTabChanged")]
    [Docking(DockingBehavior.AutoDock)]
    public partial class AeroTabControl : ContainerControl
    {
        private static readonly object EventCloseButtonClick = new object();
        private static readonly object EventNewTabButtonClick = new object();
        private static readonly object EventSelectedTabChanged = new object();

        private readonly List<AeroTabPage> _pages = new List<AeroTabPage>();
        private readonly TabStrip _tabStrip = new TabStrip()
        {
            Dock = DockStyle.Top,
            Renderer = new TabStripAeroRenderer(),
        };

        private TabPageCollection _pageCollection;
        private AeroTabPage _selectedPage;

        public AeroTabControl()
        {
            _pageCollection = new TabPageCollection(this);
            _tabStrip.NewTabButtonClicked +=InvokeNewTabButtonClicked;
            _tabStrip.SelectedTabChanged += InvokeSelectedTabChanged;
        }

        public event EventHandler<AeroTabPageEventArgs> CloseButtonClick
        {
            add { Events.AddHandler(EventCloseButtonClick, value); }
            remove { Events.RemoveHandler(EventCloseButtonClick, value); }
        }

        public event EventHandler NewTabButtonClick
        {
            add { Events.AddHandler(EventNewTabButtonClick, value); }
            remove { Events.RemoveHandler(EventNewTabButtonClick, value); }
        }

        public event EventHandler SelectedTabChanged
        {
            add { Events.AddHandler(EventSelectedTabChanged, value); }
            remove { Events.RemoveHandler(EventSelectedTabChanged, value); }
        }

        [Browsable(false)]
        [MergableProperty(false)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedTabIndex
        {
            get { return _tabStrip.SelectedTabIndex; }
            set { _tabStrip.SelectedTabIndex = value; }
        }

        [Browsable(true)]
        [MergableProperty(false)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public AeroTabPage SelectedTab
        {
            get { return _selectedPage; }
            set
            {
                if (_selectedPage != value)
                {
                    if (value != null)
                    {
                        _tabStrip.SelectedTab = value.TabStripButton;
                    }
                    else
                    {
                        _tabStrip.SelectedTab = null;
                    }
                }
            }
        }

        [MergableProperty(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabPageCollection TabPages
        {
            get 
            {
                if (_pageCollection == null)
                {
                    _pageCollection = new TabPageCollection(this);
                }

                return _pageCollection;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        protected internal virtual void OnCloseButtonClick(AeroTabPageEventArgs e)
        {
            var handler = Events[EventCloseButtonClick] as EventHandler<AeroTabPageEventArgs>;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            const Keys ControlTab = Keys.Control | Keys.Tab;

            if (TabPages.Count > 1)
            {
                if ((keyData & ControlTab) == ControlTab)
                {
                    int newIndex = SelectedTabIndex + 1;

                    if ((keyData & Keys.Shift) == Keys.Shift)
                    {
                        newIndex = SelectedTabIndex - 1;
                    }

                    if (newIndex < 0)
                    {
                        newIndex = TabPages.Count - 1;
                    }
                    else if (newIndex >= TabPages.Count)
                    {
                        newIndex = 0;
                    }

                    SelectedTabIndex = newIndex;
                    return true;
                }
            }


            return base.ProcessDialogKey(keyData);
        }

        protected virtual void OnNewTabButtonClick(EventArgs e)
        {
            var handler = Events[EventNewTabButtonClick] as EventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedTabChanged(EventArgs e)
        {
            var handler = Events[EventSelectedTabChanged] as EventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        protected override Size DefaultSize
        {
            get { return new Size(150, 150); }
        }

        protected override Padding DefaultPadding
        {
            get { return new Padding(2); }
        }

        internal TabStrip TabStrip
        {
            get { return _tabStrip; }
        }

        internal Color TabPageBackColor
        {
            get { return TabStrip.SelectedTabBottomColor; }
        }

        private void Add(AeroTabPage page)
        {
            page.Visible = false;
            page.Dock = DockStyle.Fill;

            _pages.Add(page);

            _tabStrip.SuspendLayout();
            _tabStrip.Items.Add(page.TabStripButton);
            _tabStrip.ResumeLayout();
        }

        private void AddRange(IList<AeroTabPage> pages)
        {
            var buttons = new TabStripButton[pages.Count];

            for (int i = 0; i < pages.Count; ++i)
            {
                pages[i].Visible = false;
                pages[i].Dock = DockStyle.Fill;
                buttons[i] = pages[i].TabStripButton;
            }

            _pages.AddRange(pages);

            _tabStrip.SuspendLayout();
            _tabStrip.Items.AddRange(buttons);
            _tabStrip.ResumeLayout();
        }

        private void Remove(AeroTabPage page)
        {
            if (_tabStrip.Items.Contains(page.TabStripButton))
            {
                _tabStrip.SuspendLayout();
                _tabStrip.Items.Remove(page.TabStripButton);
                _tabStrip.ResumeLayout();
            }

            if (_pages.Contains(page))
            {
                _pages.Remove(page);
            }
        }

        private void RemoveAllTabs()
        {
            System.Diagnostics.Debug.WriteLine("RemoveAllTabs");

            SuspendLayout();
            _tabStrip.SuspendLayout();

            SelectedTab = null;
            _tabStrip.RemoveAllTabs();
            _pages.Clear();

            _tabStrip.ResumeLayout();
            ResumeLayout();
        }

        private void InvokeNewTabButtonClicked(object sender, EventArgs e)
        {
            OnNewTabButtonClick(e);
        }

        private void InvokeSelectedTabChanged(object sender, EventArgs e)
        {
            if (_selectedPage != null)
            {
                _selectedPage.Visible = false;
            }

            _selectedPage = AeroTabPage.GetButtonPage(_tabStrip.SelectedTab);

            if (_selectedPage != null)
            {
                _selectedPage.Visible = true;

                _tabStrip.SendToBack();
                _tabStrip.SelectedTab = _selectedPage.TabStripButton;
            }

            OnSelectedTabChanged(e);
        }

    }
}