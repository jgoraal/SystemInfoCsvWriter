using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemInfoCsvWriter
{
    public partial class Form1 : Form
    {
        private Label labelCsvPath;
        private ListView listViewEntries;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadSystemInfoAsync().ConfigureAwait(false);
        }

        private void InitializeCustomComponents()
        {
            // Ustawienia okna
            this.Text = "Zapisz informacje do pliku .csv";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Ustawienie ikony
            try
            {
                this.Icon = new Icon("app_icon.ico");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania ikony: {ex.Message}");
            }

            int spacing = 10;
            int controlHeight = 20;
            int textBoxWidth = 260;

            // Label dla opcji dopisania do innego pliku csv
            Label labelAppendCsv = new Label
            {
                Text = "Dopisz do:",
                Location = new Point(spacing, spacing),
                AutoSize = true
            };
            this.Controls.Add(labelAppendCsv);

            // Etykieta dla ścieżki do pliku CSV
            labelCsvPath = new Label
            {
                Location = new Point(spacing, labelAppendCsv.Bottom + spacing),
                Width = textBoxWidth - 80,
                Height = controlHeight,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Brak wybranego pliku",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            this.Controls.Add(labelCsvPath);

            // Przycisk "Wybierz"
            Button buttonChooseFile = new Button
            {
                Text = "Wybierz",
                Location = new Point(labelCsvPath.Right + spacing, labelAppendCsv.Bottom + spacing),
                Width = 60,
                Height = controlHeight
            };
            buttonChooseFile.Click += ButtonChooseFile_Click;
            this.Controls.Add(buttonChooseFile);

            // Label dla grupy
            Label labelGroup = new Label
            {
                Text = "Grupa:",
                Location = new Point(spacing, labelCsvPath.Bottom + spacing),
                AutoSize = true
            };
            this.Controls.Add(labelGroup);

            // TextBox dla grupy
            TextBox textBoxGroup = new TextBox
            {
                Location = new Point(spacing, labelGroup.Bottom + spacing),
                Width = textBoxWidth,
                MaxLength = 45
            };
            this.Controls.Add(textBoxGroup);

            // Label dla podgrupy
            Label labelSubgroup = new Label
            {
                Text = "Podgrupa:",
                Location = new Point(spacing, textBoxGroup.Bottom + spacing),
                AutoSize = true
            };
            this.Controls.Add(labelSubgroup);

            // Panel dla przycisków
            Panel radioPanel = new Panel
            {
                Location = new Point(spacing, labelSubgroup.Bottom + spacing),
                Width = textBoxWidth,
                Height = controlHeight
            };

            // RadioButton dla hostname
            RadioButton radioButtonHostname = new RadioButton
            {
                Text = "Hostname",
                Location = new Point(0, 0),
                AutoSize = true,
                Checked = true
            };
            radioPanel.Controls.Add(radioButtonHostname);

            // RadioButton dla hostname - full username
            RadioButton radioButtonHostnameUsername = new RadioButton
            {
                Text = "Hostname - Full",
                Location = new Point(radioButtonHostname.Width + spacing, 0),
                AutoSize = true
            };
            radioPanel.Controls.Add(radioButtonHostnameUsername);

            this.Controls.Add(radioPanel);

            // Label dla znalezionych wpisów
            Label labelEntries = new Label
            {
                Text = "Znalezione wpisy:",
                Location = new Point(spacing, radioPanel.Bottom + spacing),
                AutoSize = true
            };
            this.Controls.Add(labelEntries);

            // ListView dla znalezionych wpisów
            listViewEntries = new ListView
            {
                Location = new Point(spacing, labelEntries.Bottom + spacing),
                Width = textBoxWidth,
                Height = 200,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewEntries.Columns.Add("Atrybut", 120);
            listViewEntries.Columns.Add("Wartość", 140);
            this.Controls.Add(listViewEntries);

            // Button zapisz
            Button buttonSave = new Button
            {
                Text = "Zapisz",
                Location = new Point(spacing, listViewEntries.Bottom + spacing),
                AutoSize = true
            };
            buttonSave.Click += (sender, e) =>
            {
                // Dodaj swoją logikę zapisywania tutaj
                MessageBox.Show("Informacje zapisane do pliku CSV!");
            };
            this.Controls.Add(buttonSave);

            this.ClientSize = new Size(textBoxWidth + 2 * spacing, buttonSave.Bottom + spacing * 2);
        }

        private void ButtonChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    labelCsvPath.Text = openFileDialog.FileName;
                }
            }
        }

        private async Task LoadSystemInfoAsync()
        {
            SystemInfo systemInfo = await SystemInfo.CreateAsync();
            DisplaySystemInfo(systemInfo);
            AutoResizeColumns();
        }

        private void DisplaySystemInfo(SystemInfo systemInfo)
        {
            listViewEntries.Items.Add(new ListViewItem(new[] { "Hostname", systemInfo.Hostname }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Username", systemInfo.Username }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Model", systemInfo.Model }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Serial", systemInfo.Serial }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "OS", systemInfo.Os }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "CPU", systemInfo.Cpu }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "RAM", systemInfo.Ram }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Memory", systemInfo.Memory }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "IP", systemInfo.Ip }));
        }

        private void AutoResizeColumns()
        {
            foreach (ColumnHeader column in listViewEntries.Columns)
            {
                column.Width = -2; // Auto resize to fit the content
            }
        }
    }
}
