using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog in which the user can pick one or more output streams
  /// </summary>
  internal sealed partial class ActionSelectionDialog : Window {
    /// <summary>
    ///   Contains all the streams that have been selected
    /// </summary>
    internal IEnumerable<PluginObject> Streams =>
      this.streamListView.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as PluginObject);

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal ActionSelectionDialog() {
      InitializeComponent();
      Update();
    }

    /// <inheritdoc />
    /// <summary>Processes Windows messages.</summary>
    /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
    protected override void WndProc(ref Message m) {
      switch (m.Msg) {
        case (int) User32.WindowMessage.WM_NOTIFY:
          // get notification header
          var nmhdr = (User32.NMHDR) m.GetLParam(typeof(User32.NMHDR));

          switch ((uint) nmhdr.code) {
            case User32.LVN_GETEMPTYMARKUP: // ListViewEx is requesting empty markup
              if (FromHandle(nmhdr.hwndFrom) == this.streamListView) {
                // get markup data
                var markup = (User32.NMLVEMPTYMARKUP) m.GetLParam(typeof(User32.NMLVEMPTYMARKUP));

                // draw centered
                markup.dwFlags = (int) User32.EMF_CENTERED;

                // set markup string
                markup.szMarkup = Resources.OutputStreamSelectionDialog_EmptyMarkup;

                // write data back
                Marshal.StructureToPtr(markup, m.LParam, false);

                // handle window message
                m.Result = new IntPtr(1);
                return;
              }

              break;
          }

          break;
      }

      base.WndProc(ref m);
    }

    /// <summary>
    ///   Updates the output stream list
    /// </summary>
    private new void Update() {
      this.streamListView.Clear();
      this.streamIconList.Images.Clear();

      this.streamListView.View = View.Tile;

      this.streamListView.Columns.Add(new ColumnHeader {Name = "streamName"});
      this.streamListView.Columns.Add(new ColumnHeader {Name = "pluginName"});
      this.streamListView.Columns.Add(new ColumnHeader {Name = "publisherName"});

      this.streamListView.Items.AddRange(Application.PluginManager.Actions.Select((s, i) => {
        try {
          this.streamIconList.Images.Add(s.Type.GetInterface("IHasImage") != null
            ? ((IHasImage) FormatterServices.GetUninitializedObject(s.Type)).GetImage()
            : Resources.Placeholder);
        } catch {
          this.streamIconList.Images.Add(Resources.Placeholder);
        }

        var item = new ListViewItem(s.ToString()) {
          ImageIndex = i,
          UseItemStyleForSubItems = true,
          Tag = s
        };

        Assembly pluginAssembly = s.Type.Assembly;
        try {
          string title =
            ((AssemblyTitleAttribute) pluginAssembly.GetCustomAttribute(typeof(AssemblyTitleAttribute))).Title;
          item.SubItems.Add(new ListViewItem.ListViewSubItem(item, $"{title} ({pluginAssembly.GetName().Version})"));
        } catch {
          item.SubItems.Add(new ListViewItem.ListViewSubItem(item, pluginAssembly.GetName().Name));
        }

        try {
          string company =
            ((AssemblyCompanyAttribute) pluginAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute))).Company;
          item.SubItems.Add(new ListViewItem.ListViewSubItem(item, company));
        } catch {
          item.SubItems.Add(new ListViewItem.ListViewSubItem(item, Resources.Plugin_DefaultPublisherName));
        }

        return item;
      }).ToArray());
    }

    /// <summary>
    ///   Triggered when the stream selection has changed
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Arguments associated to this event</param>
    private void OnStreamSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs eventArgs) =>
      this.okButton.Enabled = this.streamListView.SelectedItems.Count > 0;

    /// <summary>
    ///   Triggered when the Cancel or OK button are clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnButtonClick(object sender, EventArgs eventArgs) => Close();

    /// <summary>
    ///   Triggered when an item is activated
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnStreamItemActivated(object sender, EventArgs eventArgs) => this.okButton.PerformClick();
  }
}