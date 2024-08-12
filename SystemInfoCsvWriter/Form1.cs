using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemInfoCsvWriter
{
    public partial class Form1 : Form
    {
        private SystemInfo SystemInfo;
        private TextBox textBoxCsvPath;
        private ListView listViewEntries;
        private TextBox textBoxGroup;
        private RadioButton radioButtonHostname;
        private RadioButton radioButtonHostnameUsername;
        private ListView listViewIpAddresses; // ListView do wyboru IP

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
                this.Icon = Properties.Resources.app_icon;
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
                Text = "Dopisz do pliku:",
                Location = new Point(spacing, spacing),
                AutoSize = true
            };
            this.Controls.Add(labelAppendCsv);

            // TextBox dla ścieżki do pliku CSV
            textBoxCsvPath = new TextBox
            {
                Location = new Point(spacing, labelAppendCsv.Bottom + spacing),
                Width = textBoxWidth - 80,
                Enabled = false
            };
            this.Controls.Add(textBoxCsvPath);

            // Przycisk "Wybierz"
            Button buttonChooseFile = new Button
            {
                Text = "Wybierz",
                Location = new Point(textBoxCsvPath.Right + spacing, labelAppendCsv.Bottom + spacing),
                Width = 60,
                Height = controlHeight
            };
            buttonChooseFile.Click += ButtonChooseFile_Click;
            this.Controls.Add(buttonChooseFile);

            // Label dla grupy
            Label labelGroup = new Label
            {
                Text = "Grupa:",
                Location = new Point(spacing, textBoxCsvPath.Bottom + spacing),
                AutoSize = true
            };
            this.Controls.Add(labelGroup);

            // TextBox dla grupy
            textBoxGroup = new TextBox
            {
                Location = new Point(spacing, labelGroup.Bottom + spacing),
                MaxLength = 30,
                Width = textBoxWidth
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

            // Panel dla przycisków radiowych
            Panel radioPanel = new Panel
            {
                Location = new Point(spacing, labelSubgroup.Bottom + spacing),
                Width = textBoxWidth,
                Height = controlHeight
            };

            // RadioButton dla hostname
            radioButtonHostname = new RadioButton
            {
                Text = "Hostname",
                Checked = true,
                Location = new Point(0, 0),
                AutoSize = true
            };
            radioPanel.Controls.Add(radioButtonHostname);

            // RadioButton dla hostname - full username
            radioButtonHostnameUsername = new RadioButton
            {
                Text = "Hostname - Full username",
                Location = new Point(radioButtonHostname.Width + spacing, 0),
                AutoSize = true
            };
            radioPanel.Controls.Add(radioButtonHostnameUsername);

            this.Controls.Add(radioPanel);

            // Label dla znalezionych wpisów
            Label labelEntries = new Label
            {
                Text = "Znalezione informacje o systemie:",
                Location = new Point(spacing, radioPanel.Bottom + spacing),
                AutoSize = true
            };
            this.Controls.Add(labelEntries);

            // ListView dla znalezionych wpisów
            listViewEntries = new ListView
            {
                Location = new Point(spacing, labelEntries.Bottom + spacing),
                Width = textBoxWidth,
                Height = 190,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewEntries.Columns.Add("Atrybut", 120);
            listViewEntries.Columns.Add("Wartość", 140);
            this.Controls.Add(listViewEntries);

            // ListView dla wyboru adresu IP
            listViewIpAddresses = new ListView
            {
                Location = new Point(spacing, listViewEntries.Bottom + spacing),
                Width = textBoxWidth,
                Height = 70,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            listViewIpAddresses.Columns.Add("Interfejs", 120);
            listViewIpAddresses.Columns.Add("Adres IP", 140);
            this.Controls.Add(listViewIpAddresses);

            // Button zapisz
            Button buttonSave = new Button
            {
                Text = "Zapisz",
                Location = new Point(spacing, listViewIpAddresses.Bottom + spacing),
                AutoSize = true
            };
            buttonSave.Click += ButtonSave_Click;
            this.Controls.Add(buttonSave);

            this.ClientSize = new Size(textBoxWidth + 2 * spacing, buttonSave.Bottom + spacing * 2);
        }

        private void ButtonChooseFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxCsvPath.Text = openFileDialog.FileName;
                }
            }
        }

        private async Task LoadSystemInfoAsync()
        {
            SystemInfo = await SystemInfo.CreateAsync();
            DisplaySystemInfo();
            PopulateIpListView(); // Dodajemy wypełnienie ListView
            AutoResizeColumns();
        }

        private void DisplaySystemInfo()
        {
            listViewEntries.Items.Add(new ListViewItem(new[] { "Hostname", SystemInfo.Hostname }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Username", SystemInfo.Username }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "Model", SystemInfo.Model }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "S/N", SystemInfo.Serial }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "OS", SystemInfo.Os }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "CPU", SystemInfo.Cpu }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "RAM", SystemInfo.Ram }));
            listViewEntries.Items.Add(new ListViewItem(new[] { "HDD/SSD", SystemInfo.Memory }));
        }

        private void PopulateIpListView()
        {
            foreach (var entry in SystemInfo.Ip)
            {
                var listItem = new ListViewItem(entry.Key);
                listItem.SubItems.Add(entry.Value);
                listViewIpAddresses.Items.Add(listItem);
            }   

            if (listViewIpAddresses.Items.Count > 0)
            {
                listViewIpAddresses.Items[0].Selected = true; // Ustawiamy pierwszy adres jako domyślny
            }
        }

        private void AutoResizeColumns()
        {
            foreach (ColumnHeader column in listViewEntries.Columns)
            {
                column.Width = -2;
            }

            foreach (ColumnHeader column in listViewIpAddresses.Columns)
            {
                column.Width = -2;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxGroup.Text))
            {
                MessageBox.Show("Wpisz nazwę grupy!");
                return;
            }

            if (listViewIpAddresses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Wybierz adres IP!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxCsvPath.Text))
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.FileName = $"{SystemInfo.Hostname}_{SystemInfo.Username}.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        WriteDataToFile(saveFileDialog.FileName);
                        MessageBox.Show("Informacje zapisane do pliku CSV!");
                    }
                }
            }
            else
            {
                WriteDataToFile(textBoxCsvPath.Text, true);
                MessageBox.Show("Informacje zapisane do pliku CSV!");
            }
        }

        private void WriteDataToFile(string path, bool append = false)
        {
            using (var writer = new StreamWriter(path, append))
            {
                string group = textBoxGroup.Text;
                string subgroup = radioButtonHostname.Checked
                    ? SystemInfo.Hostname
                    : $"{SystemInfo.Hostname}-{SystemInfo.Username}";

                string selectedIp = listViewIpAddresses.SelectedItems[0].SubItems[1].Text;

                if (new FileInfo(path).Length == 0)
                {
                    writer.WriteLine("Group;Title;Username;Password;URL;Notes;Model;S\\N;OS;CPU;RAM;HDD/SSD");
                }

                // Main entry: Title: Hostname with advanced fields
                writer.WriteLine($"\"{group}/{subgroup}\";" + // Group/Subgroup
                                 $"\"{SystemInfo.Hostname}\";" + // Title: Hostname
                                 $"\"{SystemInfo.Username}\";" + // Username: Username
                                 $"\"\";" + // Password: empty
                                 $"\"{selectedIp}\";" + // URL: selected IP address
                                 $"\"\";" + // Notes: empty
                                 $"\"{SystemInfo.Model}\";" + // Model: SystemInfo.Model
                                 $"\"{SystemInfo.Serial}\";" + // S\N: SystemInfo.Serial
                                 $"\"{SystemInfo.Os}\";" + // OS: SystemInfo.Os
                                 $"\"{SystemInfo.Cpu}\";" + // CPU: SystemInfo.Cpu
                                 $"\"{SystemInfo.Ram}\";" + // RAM: SystemInfo.Ram
                                 $"\"{SystemInfo.Memory}\";"); // HDD/SSD: SystemInfo.Memory
            }
        }
    }
}
