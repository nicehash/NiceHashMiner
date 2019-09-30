namespace NiceHashMiner.Forms
{
    partial class Form_Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

#region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Settings));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonSaveClose = new System.Windows.Forms.Button();
            this.buttonDefaults = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox_Misc = new System.Windows.Forms.GroupBox();
            this.pictureBox_ShowGPUPCIeBusIDs = new System.Windows.Forms.PictureBox();
            this.checkBox_ShowGPUPCIeBusIDs = new System.Windows.Forms.CheckBox();
            this.pictureBox_WindowAlwaysOnTop = new System.Windows.Forms.PictureBox();
            this.checkBox_WindowAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.pictureBox_RunAtStartup = new System.Windows.Forms.PictureBox();
            this.checkBox_RunAtStartup = new System.Windows.Forms.CheckBox();
            this.checkBox_AllowMultipleInstances = new System.Windows.Forms.CheckBox();
            this.pictureBox_AllowMultipleInstances = new System.Windows.Forms.PictureBox();
            this.pictureBox_ShowInternetConnectionWarning = new System.Windows.Forms.PictureBox();
            this.checkBox_ShowInternetConnectionWarning = new System.Windows.Forms.CheckBox();
            this.checkBox_AutoStartMining = new System.Windows.Forms.CheckBox();
            this.checkBox_MinimizeToTray = new System.Windows.Forms.CheckBox();
            this.pictureBox_DisableWindowsErrorReporting = new System.Windows.Forms.PictureBox();
            this.pictureBox_ShowDriverVersionWarning = new System.Windows.Forms.PictureBox();
            this.pictureBox_AutoScaleBTCValues = new System.Windows.Forms.PictureBox();
            this.pictureBox_AutoStartMining = new System.Windows.Forms.PictureBox();
            this.pictureBox_MinimizeToTray = new System.Windows.Forms.PictureBox();
            this.checkBox_AutoScaleBTCValues = new System.Windows.Forms.CheckBox();
            this.checkBox_DisableWindowsErrorReporting = new System.Windows.Forms.CheckBox();
            this.checkBox_ShowDriverVersionWarning = new System.Windows.Forms.CheckBox();
            this.groupBoxDeviceMonitoring = new System.Windows.Forms.GroupBox();
            this.checkBox_DisableDevicePowerModeSettings = new System.Windows.Forms.CheckBox();
            this.pictureBox_DisableDevicePowerModeSettings = new System.Windows.Forms.PictureBox();
            this.checkBox_DisableDeviceStatusMonitoring = new System.Windows.Forms.CheckBox();
            this.pictureBox_DisableDeviceStatusMonitoring = new System.Windows.Forms.PictureBox();
            this.groupBox_Logging = new System.Windows.Forms.GroupBox();
            this.label_LogMaxFileSize = new System.Windows.Forms.Label();
            this.textBox_LogMaxFileSize = new System.Windows.Forms.TextBox();
            this.checkBox_LogToFile = new System.Windows.Forms.CheckBox();
            this.pictureBox_DebugConsole = new System.Windows.Forms.PictureBox();
            this.pictureBox_LogMaxFileSize = new System.Windows.Forms.PictureBox();
            this.pictureBox_LogToFile = new System.Windows.Forms.PictureBox();
            this.checkBox_DebugConsole = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_IdleWhenNoInternetAccess = new System.Windows.Forms.CheckBox();
            this.pictureBox_IdleWhenNoInternetAccess = new System.Windows.Forms.PictureBox();
            this.pictureBox_StartMiningWhenIdle = new System.Windows.Forms.PictureBox();
            this.checkBox_StartMiningWhenIdle = new System.Windows.Forms.CheckBox();
            this.pictureBox_MinIdleSeconds = new System.Windows.Forms.PictureBox();
            this.label_MinIdleSeconds = new System.Windows.Forms.Label();
            this.textBox_MinIdleSeconds = new System.Windows.Forms.TextBox();
            this.pictureBox_IdleType = new System.Windows.Forms.PictureBox();
            this.comboBox_IdleType = new System.Windows.Forms.ComboBox();
            this.label_IdleType = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.pictureBox_RunScriptOnCUDA_GPU_Lost = new System.Windows.Forms.PictureBox();
            this.checkBox_RunScriptOnCUDA_GPU_Lost = new System.Windows.Forms.CheckBox();
            this.pictureBox_NVIDIAP0State = new System.Windows.Forms.PictureBox();
            this.checkBox_NVIDIAP0State = new System.Windows.Forms.CheckBox();
            this.pictureBox_RunEthlargement = new System.Windows.Forms.PictureBox();
            this.checkBox_RunEthlargement = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.pictureBox_SwitchProfitabilityThreshold = new System.Windows.Forms.PictureBox();
            this.textBox_SwitchProfitabilityThreshold = new System.Windows.Forms.TextBox();
            this.label_SwitchProfitabilityThreshold = new System.Windows.Forms.Label();
            this.label_SwitchMinSeconds = new System.Windows.Forms.Label();
            this.textBox_SwitchMinSeconds = new System.Windows.Forms.TextBox();
            this.pictureBox_SwitchMaxSeconds = new System.Windows.Forms.PictureBox();
            this.pictureBox_SwitchMinSeconds = new System.Windows.Forms.PictureBox();
            this.label_SwitchMaxSeconds = new System.Windows.Forms.Label();
            this.textBox_SwitchMaxSeconds = new System.Windows.Forms.TextBox();
            this.groupBox_Miners = new System.Windows.Forms.GroupBox();
            this.checkBox_MinimizeMiningWindows = new System.Windows.Forms.CheckBox();
            this.pictureBox_MinimizeMiningWindows = new System.Windows.Forms.PictureBox();
            this.checkBox_HideMiningWindows = new System.Windows.Forms.CheckBox();
            this.pictureBox_HideMiningWindows = new System.Windows.Forms.PictureBox();
            this.pictureBox_MinerRestartDelayMS = new System.Windows.Forms.PictureBox();
            this.pictureBox_APIBindPortStart = new System.Windows.Forms.PictureBox();
            this.pictureBox_MinerAPIQueryInterval = new System.Windows.Forms.PictureBox();
            this.label_MinerAPIQueryInterval = new System.Windows.Forms.Label();
            this.label_MinerRestartDelayMS = new System.Windows.Forms.Label();
            this.label_APIBindPortStart = new System.Windows.Forms.Label();
            this.textBox_APIBindPortStart = new System.Windows.Forms.TextBox();
            this.textBox_MinerRestartDelayMS = new System.Windows.Forms.TextBox();
            this.textBox_MinerAPIQueryInterval = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label_IFTTTAPIKey = new System.Windows.Forms.Label();
            this.textBox_IFTTTKey = new System.Windows.Forms.TextBox();
            this.pictureBox_UseIFTTT = new System.Windows.Forms.PictureBox();
            this.checkBox_UseIFTTT = new System.Windows.Forms.CheckBox();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.groupBox_Main = new System.Windows.Forms.GroupBox();
            this.checkBox_MineRegardlessOfProfit = new System.Windows.Forms.CheckBox();
            this.pictureBox_MineRegardlessOfProfit = new System.Windows.Forms.PictureBox();
            this.pictureBox_ElectricityCost = new System.Windows.Forms.PictureBox();
            this.textBox_ElectricityCost = new System.Windows.Forms.TextBox();
            this.label_ElectricityCost = new System.Windows.Forms.Label();
            this.pictureBox_TimeUnit = new System.Windows.Forms.PictureBox();
            this.label_TimeUnit = new System.Windows.Forms.Label();
            this.comboBox_TimeUnit = new System.Windows.Forms.ComboBox();
            this.pictureBox_MinProfit = new System.Windows.Forms.PictureBox();
            this.textBox_MinProfit = new System.Windows.Forms.TextBox();
            this.label_MinProfit = new System.Windows.Forms.Label();
            this.groupBox_Localization = new System.Windows.Forms.GroupBox();
            this.label_Language = new System.Windows.Forms.Label();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.pictureBox_displayCurrency = new System.Windows.Forms.PictureBox();
            this.pictureBox_Language = new System.Windows.Forms.PictureBox();
            this.comboBox_Language = new System.Windows.Forms.ComboBox();
            this.currencyConverterCombobox = new System.Windows.Forms.ComboBox();
            this.label_displayCurrency = new System.Windows.Forms.Label();
            this.tabControlGeneral = new System.Windows.Forms.TabControl();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.tabPageAdvanced.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox_Misc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowGPUPCIeBusIDs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_WindowAlwaysOnTop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunAtStartup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AllowMultipleInstances)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowInternetConnectionWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableWindowsErrorReporting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowDriverVersionWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AutoScaleBTCValues)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AutoStartMining)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinimizeToTray)).BeginInit();
            this.groupBoxDeviceMonitoring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableDevicePowerModeSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableDeviceStatusMonitoring)).BeginInit();
            this.groupBox_Logging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DebugConsole)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LogMaxFileSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LogToFile)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_IdleWhenNoInternetAccess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_StartMiningWhenIdle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinIdleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_IdleType)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunScriptOnCUDA_GPU_Lost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_NVIDIAP0State)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunEthlargement)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchProfitabilityThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchMaxSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchMinSeconds)).BeginInit();
            this.groupBox_Miners.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinimizeMiningWindows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_HideMiningWindows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinerRestartDelayMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_APIBindPortStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinerAPIQueryInterval)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_UseIFTTT)).BeginInit();
            this.tabPageGeneral.SuspendLayout();
            this.groupBox_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MineRegardlessOfProfit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ElectricityCost)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_TimeUnit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinProfit)).BeginInit();
            this.groupBox_Localization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_displayCurrency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Language)).BeginInit();
            this.tabControlGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.ToolTip1_Popup);
            // 
            // buttonSaveClose
            // 
            this.buttonSaveClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveClose.Location = new System.Drawing.Point(486, 482);
            this.buttonSaveClose.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonSaveClose.Name = "buttonSaveClose";
            this.buttonSaveClose.Size = new System.Drawing.Size(134, 23);
            this.buttonSaveClose.TabIndex = 44;
            this.buttonSaveClose.Text = "&Save and Close";
            this.buttonSaveClose.UseVisualStyleBackColor = true;
            this.buttonSaveClose.Click += new System.EventHandler(this.ButtonSaveClose_Click);
            // 
            // buttonDefaults
            // 
            this.buttonDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDefaults.Location = new System.Drawing.Point(408, 482);
            this.buttonDefaults.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonDefaults.Name = "buttonDefaults";
            this.buttonDefaults.Size = new System.Drawing.Size(74, 23);
            this.buttonDefaults.TabIndex = 43;
            this.buttonDefaults.Text = "&Defaults";
            this.buttonDefaults.UseVisualStyleBackColor = true;
            this.buttonDefaults.Click += new System.EventHandler(this.ButtonDefaults_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.flowLayoutPanel1);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPageAdvanced.Size = new System.Drawing.Size(599, 439);
            this.tabPageAdvanced.TabIndex = 2;
            this.tabPageAdvanced.Text = "Advanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.groupBox_Misc);
            this.flowLayoutPanel1.Controls.Add(this.groupBoxDeviceMonitoring);
            this.flowLayoutPanel1.Controls.Add(this.groupBox_Logging);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Controls.Add(this.groupBox3);
            this.flowLayoutPanel1.Controls.Add(this.groupBox5);
            this.flowLayoutPanel1.Controls.Add(this.groupBox_Miners);
            this.flowLayoutPanel1.Controls.Add(this.groupBox4);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(595, 433);
            this.flowLayoutPanel1.TabIndex = 393;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // groupBox_Misc
            // 
            this.groupBox_Misc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Misc.Controls.Add(this.pictureBox_ShowGPUPCIeBusIDs);
            this.groupBox_Misc.Controls.Add(this.checkBox_ShowGPUPCIeBusIDs);
            this.groupBox_Misc.Controls.Add(this.pictureBox_WindowAlwaysOnTop);
            this.groupBox_Misc.Controls.Add(this.checkBox_WindowAlwaysOnTop);
            this.groupBox_Misc.Controls.Add(this.pictureBox_RunAtStartup);
            this.groupBox_Misc.Controls.Add(this.checkBox_RunAtStartup);
            this.groupBox_Misc.Controls.Add(this.checkBox_AllowMultipleInstances);
            this.groupBox_Misc.Controls.Add(this.pictureBox_AllowMultipleInstances);
            this.groupBox_Misc.Controls.Add(this.pictureBox_ShowInternetConnectionWarning);
            this.groupBox_Misc.Controls.Add(this.checkBox_ShowInternetConnectionWarning);
            this.groupBox_Misc.Controls.Add(this.checkBox_AutoStartMining);
            this.groupBox_Misc.Controls.Add(this.checkBox_MinimizeToTray);
            this.groupBox_Misc.Controls.Add(this.pictureBox_DisableWindowsErrorReporting);
            this.groupBox_Misc.Controls.Add(this.pictureBox_ShowDriverVersionWarning);
            this.groupBox_Misc.Controls.Add(this.pictureBox_AutoScaleBTCValues);
            this.groupBox_Misc.Controls.Add(this.pictureBox_AutoStartMining);
            this.groupBox_Misc.Controls.Add(this.pictureBox_MinimizeToTray);
            this.groupBox_Misc.Controls.Add(this.checkBox_AutoScaleBTCValues);
            this.groupBox_Misc.Controls.Add(this.checkBox_DisableWindowsErrorReporting);
            this.groupBox_Misc.Controls.Add(this.checkBox_ShowDriverVersionWarning);
            this.groupBox_Misc.Location = new System.Drawing.Point(2, 3);
            this.groupBox_Misc.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Misc.Name = "groupBox_Misc";
            this.groupBox_Misc.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Misc.Size = new System.Drawing.Size(392, 252);
            this.groupBox_Misc.TabIndex = 395;
            this.groupBox_Misc.TabStop = false;
            this.groupBox_Misc.Text = "Misc:";
            // 
            // pictureBox_ShowGPUPCIeBusIDs
            // 
            this.pictureBox_ShowGPUPCIeBusIDs.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ShowGPUPCIeBusIDs.Image")));
            this.pictureBox_ShowGPUPCIeBusIDs.Location = new System.Drawing.Point(222, 224);
            this.pictureBox_ShowGPUPCIeBusIDs.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_ShowGPUPCIeBusIDs.Name = "pictureBox_ShowGPUPCIeBusIDs";
            this.pictureBox_ShowGPUPCIeBusIDs.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_ShowGPUPCIeBusIDs.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_ShowGPUPCIeBusIDs.TabIndex = 379;
            this.pictureBox_ShowGPUPCIeBusIDs.TabStop = false;
            // 
            // checkBox_ShowGPUPCIeBusIDs
            // 
            this.checkBox_ShowGPUPCIeBusIDs.AutoSize = true;
            this.checkBox_ShowGPUPCIeBusIDs.Location = new System.Drawing.Point(6, 224);
            this.checkBox_ShowGPUPCIeBusIDs.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_ShowGPUPCIeBusIDs.Name = "checkBox_ShowGPUPCIeBusIDs";
            this.checkBox_ShowGPUPCIeBusIDs.Size = new System.Drawing.Size(145, 17);
            this.checkBox_ShowGPUPCIeBusIDs.TabIndex = 378;
            this.checkBox_ShowGPUPCIeBusIDs.Text = "Show GPU PCIe Bus IDs";
            this.checkBox_ShowGPUPCIeBusIDs.UseVisualStyleBackColor = true;
            // 
            // pictureBox_WindowAlwaysOnTop
            // 
            this.pictureBox_WindowAlwaysOnTop.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_WindowAlwaysOnTop.Image")));
            this.pictureBox_WindowAlwaysOnTop.Location = new System.Drawing.Point(222, 201);
            this.pictureBox_WindowAlwaysOnTop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_WindowAlwaysOnTop.Name = "pictureBox_WindowAlwaysOnTop";
            this.pictureBox_WindowAlwaysOnTop.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_WindowAlwaysOnTop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_WindowAlwaysOnTop.TabIndex = 377;
            this.pictureBox_WindowAlwaysOnTop.TabStop = false;
            // 
            // checkBox_WindowAlwaysOnTop
            // 
            this.checkBox_WindowAlwaysOnTop.AutoSize = true;
            this.checkBox_WindowAlwaysOnTop.Location = new System.Drawing.Point(6, 201);
            this.checkBox_WindowAlwaysOnTop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_WindowAlwaysOnTop.Name = "checkBox_WindowAlwaysOnTop";
            this.checkBox_WindowAlwaysOnTop.Size = new System.Drawing.Size(171, 17);
            this.checkBox_WindowAlwaysOnTop.TabIndex = 376;
            this.checkBox_WindowAlwaysOnTop.Text = "Form Windows Always On Top";
            this.checkBox_WindowAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // pictureBox_RunAtStartup
            // 
            this.pictureBox_RunAtStartup.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_RunAtStartup.Image")));
            this.pictureBox_RunAtStartup.Location = new System.Drawing.Point(222, 64);
            this.pictureBox_RunAtStartup.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_RunAtStartup.Name = "pictureBox_RunAtStartup";
            this.pictureBox_RunAtStartup.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_RunAtStartup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_RunAtStartup.TabIndex = 375;
            this.pictureBox_RunAtStartup.TabStop = false;
            // 
            // checkBox_RunAtStartup
            // 
            this.checkBox_RunAtStartup.AutoSize = true;
            this.checkBox_RunAtStartup.Location = new System.Drawing.Point(6, 64);
            this.checkBox_RunAtStartup.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_RunAtStartup.Name = "checkBox_RunAtStartup";
            this.checkBox_RunAtStartup.Size = new System.Drawing.Size(118, 17);
            this.checkBox_RunAtStartup.TabIndex = 374;
            this.checkBox_RunAtStartup.Text = "Run With Windows";
            this.checkBox_RunAtStartup.UseVisualStyleBackColor = true;
            // 
            // checkBox_AllowMultipleInstances
            // 
            this.checkBox_AllowMultipleInstances.AutoSize = true;
            this.checkBox_AllowMultipleInstances.Location = new System.Drawing.Point(6, 42);
            this.checkBox_AllowMultipleInstances.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_AllowMultipleInstances.Name = "checkBox_AllowMultipleInstances";
            this.checkBox_AllowMultipleInstances.Size = new System.Drawing.Size(139, 17);
            this.checkBox_AllowMultipleInstances.TabIndex = 373;
            this.checkBox_AllowMultipleInstances.Text = "Allow Multiple Instances";
            this.checkBox_AllowMultipleInstances.UseVisualStyleBackColor = true;
            // 
            // pictureBox_AllowMultipleInstances
            // 
            this.pictureBox_AllowMultipleInstances.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_AllowMultipleInstances.Image")));
            this.pictureBox_AllowMultipleInstances.Location = new System.Drawing.Point(222, 42);
            this.pictureBox_AllowMultipleInstances.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_AllowMultipleInstances.Name = "pictureBox_AllowMultipleInstances";
            this.pictureBox_AllowMultipleInstances.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_AllowMultipleInstances.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_AllowMultipleInstances.TabIndex = 372;
            this.pictureBox_AllowMultipleInstances.TabStop = false;
            // 
            // pictureBox_ShowInternetConnectionWarning
            // 
            this.pictureBox_ShowInternetConnectionWarning.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ShowInternetConnectionWarning.Image")));
            this.pictureBox_ShowInternetConnectionWarning.Location = new System.Drawing.Point(222, 178);
            this.pictureBox_ShowInternetConnectionWarning.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_ShowInternetConnectionWarning.Name = "pictureBox_ShowInternetConnectionWarning";
            this.pictureBox_ShowInternetConnectionWarning.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_ShowInternetConnectionWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_ShowInternetConnectionWarning.TabIndex = 371;
            this.pictureBox_ShowInternetConnectionWarning.TabStop = false;
            // 
            // checkBox_ShowInternetConnectionWarning
            // 
            this.checkBox_ShowInternetConnectionWarning.AutoSize = true;
            this.checkBox_ShowInternetConnectionWarning.Location = new System.Drawing.Point(6, 178);
            this.checkBox_ShowInternetConnectionWarning.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_ShowInternetConnectionWarning.Name = "checkBox_ShowInternetConnectionWarning";
            this.checkBox_ShowInternetConnectionWarning.Size = new System.Drawing.Size(192, 17);
            this.checkBox_ShowInternetConnectionWarning.TabIndex = 370;
            this.checkBox_ShowInternetConnectionWarning.Text = "Show Internet Connection Warning";
            this.checkBox_ShowInternetConnectionWarning.UseVisualStyleBackColor = true;
            // 
            // checkBox_AutoStartMining
            // 
            this.checkBox_AutoStartMining.AutoSize = true;
            this.checkBox_AutoStartMining.Location = new System.Drawing.Point(6, 19);
            this.checkBox_AutoStartMining.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_AutoStartMining.Name = "checkBox_AutoStartMining";
            this.checkBox_AutoStartMining.Size = new System.Drawing.Size(102, 17);
            this.checkBox_AutoStartMining.TabIndex = 315;
            this.checkBox_AutoStartMining.Text = "Autostart Mining";
            this.checkBox_AutoStartMining.UseVisualStyleBackColor = true;
            // 
            // checkBox_MinimizeToTray
            // 
            this.checkBox_MinimizeToTray.AutoSize = true;
            this.checkBox_MinimizeToTray.Location = new System.Drawing.Point(6, 87);
            this.checkBox_MinimizeToTray.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_MinimizeToTray.Name = "checkBox_MinimizeToTray";
            this.checkBox_MinimizeToTray.Size = new System.Drawing.Size(106, 17);
            this.checkBox_MinimizeToTray.TabIndex = 316;
            this.checkBox_MinimizeToTray.Text = "Minimize To Tray";
            this.checkBox_MinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // pictureBox_DisableWindowsErrorReporting
            // 
            this.pictureBox_DisableWindowsErrorReporting.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_DisableWindowsErrorReporting.Image")));
            this.pictureBox_DisableWindowsErrorReporting.Location = new System.Drawing.Point(222, 155);
            this.pictureBox_DisableWindowsErrorReporting.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_DisableWindowsErrorReporting.Name = "pictureBox_DisableWindowsErrorReporting";
            this.pictureBox_DisableWindowsErrorReporting.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_DisableWindowsErrorReporting.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_DisableWindowsErrorReporting.TabIndex = 364;
            this.pictureBox_DisableWindowsErrorReporting.TabStop = false;
            // 
            // pictureBox_ShowDriverVersionWarning
            // 
            this.pictureBox_ShowDriverVersionWarning.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ShowDriverVersionWarning.Image")));
            this.pictureBox_ShowDriverVersionWarning.Location = new System.Drawing.Point(222, 134);
            this.pictureBox_ShowDriverVersionWarning.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_ShowDriverVersionWarning.Name = "pictureBox_ShowDriverVersionWarning";
            this.pictureBox_ShowDriverVersionWarning.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_ShowDriverVersionWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_ShowDriverVersionWarning.TabIndex = 364;
            this.pictureBox_ShowDriverVersionWarning.TabStop = false;
            // 
            // pictureBox_AutoScaleBTCValues
            // 
            this.pictureBox_AutoScaleBTCValues.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_AutoScaleBTCValues.Image")));
            this.pictureBox_AutoScaleBTCValues.Location = new System.Drawing.Point(222, 111);
            this.pictureBox_AutoScaleBTCValues.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_AutoScaleBTCValues.Name = "pictureBox_AutoScaleBTCValues";
            this.pictureBox_AutoScaleBTCValues.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_AutoScaleBTCValues.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_AutoScaleBTCValues.TabIndex = 364;
            this.pictureBox_AutoScaleBTCValues.TabStop = false;
            // 
            // pictureBox_AutoStartMining
            // 
            this.pictureBox_AutoStartMining.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_AutoStartMining.Image")));
            this.pictureBox_AutoStartMining.Location = new System.Drawing.Point(222, 19);
            this.pictureBox_AutoStartMining.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_AutoStartMining.Name = "pictureBox_AutoStartMining";
            this.pictureBox_AutoStartMining.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_AutoStartMining.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_AutoStartMining.TabIndex = 364;
            this.pictureBox_AutoStartMining.TabStop = false;
            // 
            // pictureBox_MinimizeToTray
            // 
            this.pictureBox_MinimizeToTray.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinimizeToTray.Image")));
            this.pictureBox_MinimizeToTray.Location = new System.Drawing.Point(222, 87);
            this.pictureBox_MinimizeToTray.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinimizeToTray.Name = "pictureBox_MinimizeToTray";
            this.pictureBox_MinimizeToTray.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinimizeToTray.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinimizeToTray.TabIndex = 364;
            this.pictureBox_MinimizeToTray.TabStop = false;
            // 
            // checkBox_AutoScaleBTCValues
            // 
            this.checkBox_AutoScaleBTCValues.AutoSize = true;
            this.checkBox_AutoScaleBTCValues.Location = new System.Drawing.Point(6, 111);
            this.checkBox_AutoScaleBTCValues.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_AutoScaleBTCValues.Name = "checkBox_AutoScaleBTCValues";
            this.checkBox_AutoScaleBTCValues.Size = new System.Drawing.Size(132, 17);
            this.checkBox_AutoScaleBTCValues.TabIndex = 321;
            this.checkBox_AutoScaleBTCValues.Text = "Autoscale BTC Values";
            this.checkBox_AutoScaleBTCValues.UseVisualStyleBackColor = true;
            // 
            // checkBox_DisableWindowsErrorReporting
            // 
            this.checkBox_DisableWindowsErrorReporting.AutoSize = true;
            this.checkBox_DisableWindowsErrorReporting.Location = new System.Drawing.Point(6, 155);
            this.checkBox_DisableWindowsErrorReporting.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_DisableWindowsErrorReporting.Name = "checkBox_DisableWindowsErrorReporting";
            this.checkBox_DisableWindowsErrorReporting.Size = new System.Drawing.Size(182, 17);
            this.checkBox_DisableWindowsErrorReporting.TabIndex = 324;
            this.checkBox_DisableWindowsErrorReporting.Text = "Disable Windows Error Reporting";
            this.checkBox_DisableWindowsErrorReporting.UseVisualStyleBackColor = true;
            // 
            // checkBox_ShowDriverVersionWarning
            // 
            this.checkBox_ShowDriverVersionWarning.AutoSize = true;
            this.checkBox_ShowDriverVersionWarning.Location = new System.Drawing.Point(6, 134);
            this.checkBox_ShowDriverVersionWarning.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_ShowDriverVersionWarning.Name = "checkBox_ShowDriverVersionWarning";
            this.checkBox_ShowDriverVersionWarning.Size = new System.Drawing.Size(165, 17);
            this.checkBox_ShowDriverVersionWarning.TabIndex = 323;
            this.checkBox_ShowDriverVersionWarning.Text = "Show Driver Version Warning";
            this.checkBox_ShowDriverVersionWarning.UseVisualStyleBackColor = true;
            // 
            // groupBoxDeviceMonitoring
            // 
            this.groupBoxDeviceMonitoring.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDeviceMonitoring.Controls.Add(this.checkBox_DisableDevicePowerModeSettings);
            this.groupBoxDeviceMonitoring.Controls.Add(this.pictureBox_DisableDevicePowerModeSettings);
            this.groupBoxDeviceMonitoring.Controls.Add(this.checkBox_DisableDeviceStatusMonitoring);
            this.groupBoxDeviceMonitoring.Controls.Add(this.pictureBox_DisableDeviceStatusMonitoring);
            this.groupBoxDeviceMonitoring.Location = new System.Drawing.Point(2, 261);
            this.groupBoxDeviceMonitoring.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBoxDeviceMonitoring.Name = "groupBoxDeviceMonitoring";
            this.groupBoxDeviceMonitoring.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBoxDeviceMonitoring.Size = new System.Drawing.Size(392, 72);
            this.groupBoxDeviceMonitoring.TabIndex = 396;
            this.groupBoxDeviceMonitoring.TabStop = false;
            this.groupBoxDeviceMonitoring.Text = "Device Monitoring:";
            // 
            // checkBox_DisableDevicePowerModeSettings
            // 
            this.checkBox_DisableDevicePowerModeSettings.AutoSize = true;
            this.checkBox_DisableDevicePowerModeSettings.Location = new System.Drawing.Point(6, 42);
            this.checkBox_DisableDevicePowerModeSettings.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_DisableDevicePowerModeSettings.Name = "checkBox_DisableDevicePowerModeSettings";
            this.checkBox_DisableDevicePowerModeSettings.Size = new System.Drawing.Size(202, 17);
            this.checkBox_DisableDevicePowerModeSettings.TabIndex = 373;
            this.checkBox_DisableDevicePowerModeSettings.Text = "Disable Device Power Mode Settings";
            this.checkBox_DisableDevicePowerModeSettings.UseVisualStyleBackColor = true;
            // 
            // pictureBox_DisableDevicePowerModeSettings
            // 
            this.pictureBox_DisableDevicePowerModeSettings.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_DisableDevicePowerModeSettings.Image")));
            this.pictureBox_DisableDevicePowerModeSettings.Location = new System.Drawing.Point(222, 42);
            this.pictureBox_DisableDevicePowerModeSettings.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_DisableDevicePowerModeSettings.Name = "pictureBox_DisableDevicePowerModeSettings";
            this.pictureBox_DisableDevicePowerModeSettings.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_DisableDevicePowerModeSettings.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_DisableDevicePowerModeSettings.TabIndex = 372;
            this.pictureBox_DisableDevicePowerModeSettings.TabStop = false;
            // 
            // checkBox_DisableDeviceStatusMonitoring
            // 
            this.checkBox_DisableDeviceStatusMonitoring.AutoSize = true;
            this.checkBox_DisableDeviceStatusMonitoring.Location = new System.Drawing.Point(6, 19);
            this.checkBox_DisableDeviceStatusMonitoring.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_DisableDeviceStatusMonitoring.Name = "checkBox_DisableDeviceStatusMonitoring";
            this.checkBox_DisableDeviceStatusMonitoring.Size = new System.Drawing.Size(183, 17);
            this.checkBox_DisableDeviceStatusMonitoring.TabIndex = 315;
            this.checkBox_DisableDeviceStatusMonitoring.Text = "Disable Device Status Monitoring";
            this.checkBox_DisableDeviceStatusMonitoring.UseVisualStyleBackColor = true;
            // 
            // pictureBox_DisableDeviceStatusMonitoring
            // 
            this.pictureBox_DisableDeviceStatusMonitoring.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_DisableDeviceStatusMonitoring.Image")));
            this.pictureBox_DisableDeviceStatusMonitoring.Location = new System.Drawing.Point(222, 19);
            this.pictureBox_DisableDeviceStatusMonitoring.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_DisableDeviceStatusMonitoring.Name = "pictureBox_DisableDeviceStatusMonitoring";
            this.pictureBox_DisableDeviceStatusMonitoring.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_DisableDeviceStatusMonitoring.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_DisableDeviceStatusMonitoring.TabIndex = 364;
            this.pictureBox_DisableDeviceStatusMonitoring.TabStop = false;
            // 
            // groupBox_Logging
            // 
            this.groupBox_Logging.Controls.Add(this.label_LogMaxFileSize);
            this.groupBox_Logging.Controls.Add(this.textBox_LogMaxFileSize);
            this.groupBox_Logging.Controls.Add(this.checkBox_LogToFile);
            this.groupBox_Logging.Controls.Add(this.pictureBox_DebugConsole);
            this.groupBox_Logging.Controls.Add(this.pictureBox_LogMaxFileSize);
            this.groupBox_Logging.Controls.Add(this.pictureBox_LogToFile);
            this.groupBox_Logging.Controls.Add(this.checkBox_DebugConsole);
            this.groupBox_Logging.Location = new System.Drawing.Point(2, 339);
            this.groupBox_Logging.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Logging.Name = "groupBox_Logging";
            this.groupBox_Logging.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Logging.Size = new System.Drawing.Size(391, 70);
            this.groupBox_Logging.TabIndex = 392;
            this.groupBox_Logging.TabStop = false;
            this.groupBox_Logging.Text = "Logging:";
            // 
            // label_LogMaxFileSize
            // 
            this.label_LogMaxFileSize.AutoSize = true;
            this.label_LogMaxFileSize.Location = new System.Drawing.Point(174, 19);
            this.label_LogMaxFileSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_LogMaxFileSize.Name = "label_LogMaxFileSize";
            this.label_LogMaxFileSize.Size = new System.Drawing.Size(127, 13);
            this.label_LogMaxFileSize.TabIndex = 357;
            this.label_LogMaxFileSize.Text = "Log Max File Size [bytes]:";
            // 
            // textBox_LogMaxFileSize
            // 
            this.textBox_LogMaxFileSize.Location = new System.Drawing.Point(174, 41);
            this.textBox_LogMaxFileSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_LogMaxFileSize.Name = "textBox_LogMaxFileSize";
            this.textBox_LogMaxFileSize.Size = new System.Drawing.Size(160, 20);
            this.textBox_LogMaxFileSize.TabIndex = 334;
            // 
            // checkBox_LogToFile
            // 
            this.checkBox_LogToFile.AutoSize = true;
            this.checkBox_LogToFile.Location = new System.Drawing.Point(6, 19);
            this.checkBox_LogToFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_LogToFile.Name = "checkBox_LogToFile";
            this.checkBox_LogToFile.Size = new System.Drawing.Size(79, 17);
            this.checkBox_LogToFile.TabIndex = 327;
            this.checkBox_LogToFile.Text = "Log To File";
            this.checkBox_LogToFile.UseVisualStyleBackColor = true;
            // 
            // pictureBox_DebugConsole
            // 
            this.pictureBox_DebugConsole.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_DebugConsole.Image")));
            this.pictureBox_DebugConsole.Location = new System.Drawing.Point(130, 42);
            this.pictureBox_DebugConsole.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_DebugConsole.Name = "pictureBox_DebugConsole";
            this.pictureBox_DebugConsole.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_DebugConsole.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_DebugConsole.TabIndex = 364;
            this.pictureBox_DebugConsole.TabStop = false;
            // 
            // pictureBox_LogMaxFileSize
            // 
            this.pictureBox_LogMaxFileSize.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_LogMaxFileSize.Image")));
            this.pictureBox_LogMaxFileSize.Location = new System.Drawing.Point(318, 19);
            this.pictureBox_LogMaxFileSize.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_LogMaxFileSize.Name = "pictureBox_LogMaxFileSize";
            this.pictureBox_LogMaxFileSize.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_LogMaxFileSize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_LogMaxFileSize.TabIndex = 364;
            this.pictureBox_LogMaxFileSize.TabStop = false;
            // 
            // pictureBox_LogToFile
            // 
            this.pictureBox_LogToFile.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_LogToFile.Image")));
            this.pictureBox_LogToFile.Location = new System.Drawing.Point(130, 19);
            this.pictureBox_LogToFile.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_LogToFile.Name = "pictureBox_LogToFile";
            this.pictureBox_LogToFile.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_LogToFile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_LogToFile.TabIndex = 364;
            this.pictureBox_LogToFile.TabStop = false;
            // 
            // checkBox_DebugConsole
            // 
            this.checkBox_DebugConsole.AutoSize = true;
            this.checkBox_DebugConsole.Location = new System.Drawing.Point(6, 42);
            this.checkBox_DebugConsole.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_DebugConsole.Name = "checkBox_DebugConsole";
            this.checkBox_DebugConsole.Size = new System.Drawing.Size(99, 17);
            this.checkBox_DebugConsole.TabIndex = 313;
            this.checkBox_DebugConsole.Text = "Debug Console";
            this.checkBox_DebugConsole.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_IdleWhenNoInternetAccess);
            this.groupBox2.Controls.Add(this.pictureBox_IdleWhenNoInternetAccess);
            this.groupBox2.Controls.Add(this.pictureBox_StartMiningWhenIdle);
            this.groupBox2.Controls.Add(this.checkBox_StartMiningWhenIdle);
            this.groupBox2.Controls.Add(this.pictureBox_MinIdleSeconds);
            this.groupBox2.Controls.Add(this.label_MinIdleSeconds);
            this.groupBox2.Controls.Add(this.textBox_MinIdleSeconds);
            this.groupBox2.Controls.Add(this.pictureBox_IdleType);
            this.groupBox2.Controls.Add(this.comboBox_IdleType);
            this.groupBox2.Controls.Add(this.label_IdleType);
            this.groupBox2.Location = new System.Drawing.Point(3, 415);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(390, 123);
            this.groupBox2.TabIndex = 390;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Idle Mining:";
            // 
            // checkBox_IdleWhenNoInternetAccess
            // 
            this.checkBox_IdleWhenNoInternetAccess.AutoSize = true;
            this.checkBox_IdleWhenNoInternetAccess.Location = new System.Drawing.Point(5, 19);
            this.checkBox_IdleWhenNoInternetAccess.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_IdleWhenNoInternetAccess.Name = "checkBox_IdleWhenNoInternetAccess";
            this.checkBox_IdleWhenNoInternetAccess.Size = new System.Drawing.Size(169, 17);
            this.checkBox_IdleWhenNoInternetAccess.TabIndex = 402;
            this.checkBox_IdleWhenNoInternetAccess.Text = "Idle When No Internet Access";
            this.checkBox_IdleWhenNoInternetAccess.UseVisualStyleBackColor = true;
            // 
            // pictureBox_IdleWhenNoInternetAccess
            // 
            this.pictureBox_IdleWhenNoInternetAccess.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_IdleWhenNoInternetAccess.Image")));
            this.pictureBox_IdleWhenNoInternetAccess.Location = new System.Drawing.Point(198, 19);
            this.pictureBox_IdleWhenNoInternetAccess.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_IdleWhenNoInternetAccess.Name = "pictureBox_IdleWhenNoInternetAccess";
            this.pictureBox_IdleWhenNoInternetAccess.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_IdleWhenNoInternetAccess.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_IdleWhenNoInternetAccess.TabIndex = 401;
            this.pictureBox_IdleWhenNoInternetAccess.TabStop = false;
            // 
            // pictureBox_StartMiningWhenIdle
            // 
            this.pictureBox_StartMiningWhenIdle.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_StartMiningWhenIdle.Image")));
            this.pictureBox_StartMiningWhenIdle.Location = new System.Drawing.Point(144, 42);
            this.pictureBox_StartMiningWhenIdle.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_StartMiningWhenIdle.Name = "pictureBox_StartMiningWhenIdle";
            this.pictureBox_StartMiningWhenIdle.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_StartMiningWhenIdle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_StartMiningWhenIdle.TabIndex = 398;
            this.pictureBox_StartMiningWhenIdle.TabStop = false;
            // 
            // checkBox_StartMiningWhenIdle
            // 
            this.checkBox_StartMiningWhenIdle.AutoSize = true;
            this.checkBox_StartMiningWhenIdle.Location = new System.Drawing.Point(6, 42);
            this.checkBox_StartMiningWhenIdle.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_StartMiningWhenIdle.Name = "checkBox_StartMiningWhenIdle";
            this.checkBox_StartMiningWhenIdle.Size = new System.Drawing.Size(134, 17);
            this.checkBox_StartMiningWhenIdle.TabIndex = 397;
            this.checkBox_StartMiningWhenIdle.Text = "Start Mining When Idle";
            this.checkBox_StartMiningWhenIdle.UseVisualStyleBackColor = true;
            // 
            // pictureBox_MinIdleSeconds
            // 
            this.pictureBox_MinIdleSeconds.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinIdleSeconds.Image")));
            this.pictureBox_MinIdleSeconds.Location = new System.Drawing.Point(344, 72);
            this.pictureBox_MinIdleSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinIdleSeconds.Name = "pictureBox_MinIdleSeconds";
            this.pictureBox_MinIdleSeconds.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinIdleSeconds.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinIdleSeconds.TabIndex = 396;
            this.pictureBox_MinIdleSeconds.TabStop = false;
            // 
            // label_MinIdleSeconds
            // 
            this.label_MinIdleSeconds.AutoSize = true;
            this.label_MinIdleSeconds.Location = new System.Drawing.Point(186, 72);
            this.label_MinIdleSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_MinIdleSeconds.Name = "label_MinIdleSeconds";
            this.label_MinIdleSeconds.Size = new System.Drawing.Size(85, 13);
            this.label_MinIdleSeconds.TabIndex = 395;
            this.label_MinIdleSeconds.Text = "Minimum Idle [s]:";
            // 
            // textBox_MinIdleSeconds
            // 
            this.textBox_MinIdleSeconds.Location = new System.Drawing.Point(186, 90);
            this.textBox_MinIdleSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_MinIdleSeconds.Name = "textBox_MinIdleSeconds";
            this.textBox_MinIdleSeconds.Size = new System.Drawing.Size(178, 20);
            this.textBox_MinIdleSeconds.TabIndex = 394;
            // 
            // pictureBox_IdleType
            // 
            this.pictureBox_IdleType.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_IdleType.Image")));
            this.pictureBox_IdleType.Location = new System.Drawing.Point(159, 70);
            this.pictureBox_IdleType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_IdleType.Name = "pictureBox_IdleType";
            this.pictureBox_IdleType.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_IdleType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_IdleType.TabIndex = 393;
            this.pictureBox_IdleType.TabStop = false;
            // 
            // comboBox_IdleType
            // 
            this.comboBox_IdleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_IdleType.FormattingEnabled = true;
            this.comboBox_IdleType.Location = new System.Drawing.Point(6, 89);
            this.comboBox_IdleType.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_IdleType.Name = "comboBox_IdleType";
            this.comboBox_IdleType.Size = new System.Drawing.Size(172, 21);
            this.comboBox_IdleType.TabIndex = 391;
            // 
            // label_IdleType
            // 
            this.label_IdleType.AutoSize = true;
            this.label_IdleType.Location = new System.Drawing.Point(6, 73);
            this.label_IdleType.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_IdleType.Name = "label_IdleType";
            this.label_IdleType.Size = new System.Drawing.Size(100, 13);
            this.label_IdleType.TabIndex = 392;
            this.label_IdleType.Text = "Idle Check Method:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.pictureBox_RunScriptOnCUDA_GPU_Lost);
            this.groupBox3.Controls.Add(this.checkBox_RunScriptOnCUDA_GPU_Lost);
            this.groupBox3.Controls.Add(this.pictureBox_NVIDIAP0State);
            this.groupBox3.Controls.Add(this.checkBox_NVIDIAP0State);
            this.groupBox3.Controls.Add(this.pictureBox_RunEthlargement);
            this.groupBox3.Controls.Add(this.checkBox_RunEthlargement);
            this.groupBox3.Location = new System.Drawing.Point(3, 544);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(390, 91);
            this.groupBox3.TabIndex = 391;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "NVIDIA/CUDA";
            // 
            // pictureBox_RunScriptOnCUDA_GPU_Lost
            // 
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_RunScriptOnCUDA_GPU_Lost.Image")));
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.Location = new System.Drawing.Point(200, 18);
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.Name = "pictureBox_RunScriptOnCUDA_GPU_Lost";
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.TabIndex = 393;
            this.pictureBox_RunScriptOnCUDA_GPU_Lost.TabStop = false;
            // 
            // checkBox_RunScriptOnCUDA_GPU_Lost
            // 
            this.checkBox_RunScriptOnCUDA_GPU_Lost.AutoSize = true;
            this.checkBox_RunScriptOnCUDA_GPU_Lost.Location = new System.Drawing.Point(5, 19);
            this.checkBox_RunScriptOnCUDA_GPU_Lost.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_RunScriptOnCUDA_GPU_Lost.Name = "checkBox_RunScriptOnCUDA_GPU_Lost";
            this.checkBox_RunScriptOnCUDA_GPU_Lost.Size = new System.Drawing.Size(191, 17);
            this.checkBox_RunScriptOnCUDA_GPU_Lost.TabIndex = 392;
            this.checkBox_RunScriptOnCUDA_GPU_Lost.Text = "Run script when CUDA GPU is lost";
            this.checkBox_RunScriptOnCUDA_GPU_Lost.UseVisualStyleBackColor = true;
            // 
            // pictureBox_NVIDIAP0State
            // 
            this.pictureBox_NVIDIAP0State.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_NVIDIAP0State.Image")));
            this.pictureBox_NVIDIAP0State.Location = new System.Drawing.Point(120, 45);
            this.pictureBox_NVIDIAP0State.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_NVIDIAP0State.Name = "pictureBox_NVIDIAP0State";
            this.pictureBox_NVIDIAP0State.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_NVIDIAP0State.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_NVIDIAP0State.TabIndex = 391;
            this.pictureBox_NVIDIAP0State.TabStop = false;
            // 
            // checkBox_NVIDIAP0State
            // 
            this.checkBox_NVIDIAP0State.AutoSize = true;
            this.checkBox_NVIDIAP0State.Location = new System.Drawing.Point(5, 45);
            this.checkBox_NVIDIAP0State.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_NVIDIAP0State.Name = "checkBox_NVIDIAP0State";
            this.checkBox_NVIDIAP0State.Size = new System.Drawing.Size(106, 17);
            this.checkBox_NVIDIAP0State.TabIndex = 390;
            this.checkBox_NVIDIAP0State.Text = "NVIDIA P0 State";
            this.checkBox_NVIDIAP0State.UseVisualStyleBackColor = true;
            // 
            // pictureBox_RunEthlargement
            // 
            this.pictureBox_RunEthlargement.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_RunEthlargement.Image")));
            this.pictureBox_RunEthlargement.Location = new System.Drawing.Point(120, 69);
            this.pictureBox_RunEthlargement.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_RunEthlargement.Name = "pictureBox_RunEthlargement";
            this.pictureBox_RunEthlargement.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_RunEthlargement.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_RunEthlargement.TabIndex = 389;
            this.pictureBox_RunEthlargement.TabStop = false;
            // 
            // checkBox_RunEthlargement
            // 
            this.checkBox_RunEthlargement.AutoSize = true;
            this.checkBox_RunEthlargement.Location = new System.Drawing.Point(5, 70);
            this.checkBox_RunEthlargement.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_RunEthlargement.Name = "checkBox_RunEthlargement";
            this.checkBox_RunEthlargement.Size = new System.Drawing.Size(111, 17);
            this.checkBox_RunEthlargement.TabIndex = 388;
            this.checkBox_RunEthlargement.Text = "Run Ethlargement";
            this.checkBox_RunEthlargement.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.pictureBox_SwitchProfitabilityThreshold);
            this.groupBox5.Controls.Add(this.textBox_SwitchProfitabilityThreshold);
            this.groupBox5.Controls.Add(this.label_SwitchProfitabilityThreshold);
            this.groupBox5.Controls.Add(this.label_SwitchMinSeconds);
            this.groupBox5.Controls.Add(this.textBox_SwitchMinSeconds);
            this.groupBox5.Controls.Add(this.pictureBox_SwitchMaxSeconds);
            this.groupBox5.Controls.Add(this.pictureBox_SwitchMinSeconds);
            this.groupBox5.Controls.Add(this.label_SwitchMaxSeconds);
            this.groupBox5.Controls.Add(this.textBox_SwitchMaxSeconds);
            this.groupBox5.Location = new System.Drawing.Point(3, 641);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(390, 120);
            this.groupBox5.TabIndex = 394;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Switching:";
            // 
            // pictureBox_SwitchProfitabilityThreshold
            // 
            this.pictureBox_SwitchProfitabilityThreshold.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_SwitchProfitabilityThreshold.Image")));
            this.pictureBox_SwitchProfitabilityThreshold.Location = new System.Drawing.Point(157, 72);
            this.pictureBox_SwitchProfitabilityThreshold.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_SwitchProfitabilityThreshold.Name = "pictureBox_SwitchProfitabilityThreshold";
            this.pictureBox_SwitchProfitabilityThreshold.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_SwitchProfitabilityThreshold.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_SwitchProfitabilityThreshold.TabIndex = 388;
            this.pictureBox_SwitchProfitabilityThreshold.TabStop = false;
            // 
            // textBox_SwitchProfitabilityThreshold
            // 
            this.textBox_SwitchProfitabilityThreshold.Location = new System.Drawing.Point(5, 90);
            this.textBox_SwitchProfitabilityThreshold.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_SwitchProfitabilityThreshold.Name = "textBox_SwitchProfitabilityThreshold";
            this.textBox_SwitchProfitabilityThreshold.Size = new System.Drawing.Size(172, 20);
            this.textBox_SwitchProfitabilityThreshold.TabIndex = 386;
            // 
            // label_SwitchProfitabilityThreshold
            // 
            this.label_SwitchProfitabilityThreshold.AutoSize = true;
            this.label_SwitchProfitabilityThreshold.Location = new System.Drawing.Point(5, 70);
            this.label_SwitchProfitabilityThreshold.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_SwitchProfitabilityThreshold.Name = "label_SwitchProfitabilityThreshold";
            this.label_SwitchProfitabilityThreshold.Size = new System.Drawing.Size(145, 13);
            this.label_SwitchProfitabilityThreshold.TabIndex = 387;
            this.label_SwitchProfitabilityThreshold.Text = "Switch Profitability Threshold:";
            // 
            // label_SwitchMinSeconds
            // 
            this.label_SwitchMinSeconds.AutoSize = true;
            this.label_SwitchMinSeconds.Location = new System.Drawing.Point(5, 25);
            this.label_SwitchMinSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_SwitchMinSeconds.Name = "label_SwitchMinSeconds";
            this.label_SwitchMinSeconds.Size = new System.Drawing.Size(100, 13);
            this.label_SwitchMinSeconds.TabIndex = 362;
            this.label_SwitchMinSeconds.Text = "Switch Minimum [s]:";
            // 
            // textBox_SwitchMinSeconds
            // 
            this.textBox_SwitchMinSeconds.Location = new System.Drawing.Point(5, 44);
            this.textBox_SwitchMinSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_SwitchMinSeconds.Name = "textBox_SwitchMinSeconds";
            this.textBox_SwitchMinSeconds.Size = new System.Drawing.Size(178, 20);
            this.textBox_SwitchMinSeconds.TabIndex = 342;
            // 
            // pictureBox_SwitchMaxSeconds
            // 
            this.pictureBox_SwitchMaxSeconds.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_SwitchMaxSeconds.Image")));
            this.pictureBox_SwitchMaxSeconds.Location = new System.Drawing.Point(347, 25);
            this.pictureBox_SwitchMaxSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_SwitchMaxSeconds.Name = "pictureBox_SwitchMaxSeconds";
            this.pictureBox_SwitchMaxSeconds.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_SwitchMaxSeconds.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_SwitchMaxSeconds.TabIndex = 385;
            this.pictureBox_SwitchMaxSeconds.TabStop = false;
            // 
            // pictureBox_SwitchMinSeconds
            // 
            this.pictureBox_SwitchMinSeconds.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_SwitchMinSeconds.Image")));
            this.pictureBox_SwitchMinSeconds.Location = new System.Drawing.Point(163, 25);
            this.pictureBox_SwitchMinSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_SwitchMinSeconds.Name = "pictureBox_SwitchMinSeconds";
            this.pictureBox_SwitchMinSeconds.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_SwitchMinSeconds.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_SwitchMinSeconds.TabIndex = 385;
            this.pictureBox_SwitchMinSeconds.TabStop = false;
            // 
            // label_SwitchMaxSeconds
            // 
            this.label_SwitchMaxSeconds.AutoSize = true;
            this.label_SwitchMaxSeconds.Location = new System.Drawing.Point(195, 25);
            this.label_SwitchMaxSeconds.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_SwitchMaxSeconds.Name = "label_SwitchMaxSeconds";
            this.label_SwitchMaxSeconds.Size = new System.Drawing.Size(103, 13);
            this.label_SwitchMaxSeconds.TabIndex = 378;
            this.label_SwitchMaxSeconds.Text = "Switch Maximum [s]:";
            // 
            // textBox_SwitchMaxSeconds
            // 
            this.textBox_SwitchMaxSeconds.Location = new System.Drawing.Point(195, 44);
            this.textBox_SwitchMaxSeconds.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_SwitchMaxSeconds.Name = "textBox_SwitchMaxSeconds";
            this.textBox_SwitchMaxSeconds.Size = new System.Drawing.Size(172, 20);
            this.textBox_SwitchMaxSeconds.TabIndex = 337;
            // 
            // groupBox_Miners
            // 
            this.groupBox_Miners.Controls.Add(this.checkBox_MinimizeMiningWindows);
            this.groupBox_Miners.Controls.Add(this.pictureBox_MinimizeMiningWindows);
            this.groupBox_Miners.Controls.Add(this.checkBox_HideMiningWindows);
            this.groupBox_Miners.Controls.Add(this.pictureBox_HideMiningWindows);
            this.groupBox_Miners.Controls.Add(this.pictureBox_MinerRestartDelayMS);
            this.groupBox_Miners.Controls.Add(this.pictureBox_APIBindPortStart);
            this.groupBox_Miners.Controls.Add(this.pictureBox_MinerAPIQueryInterval);
            this.groupBox_Miners.Controls.Add(this.label_MinerAPIQueryInterval);
            this.groupBox_Miners.Controls.Add(this.label_MinerRestartDelayMS);
            this.groupBox_Miners.Controls.Add(this.label_APIBindPortStart);
            this.groupBox_Miners.Controls.Add(this.textBox_APIBindPortStart);
            this.groupBox_Miners.Controls.Add(this.textBox_MinerRestartDelayMS);
            this.groupBox_Miners.Controls.Add(this.textBox_MinerAPIQueryInterval);
            this.groupBox_Miners.Location = new System.Drawing.Point(2, 767);
            this.groupBox_Miners.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Miners.Name = "groupBox_Miners";
            this.groupBox_Miners.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Miners.Size = new System.Drawing.Size(391, 156);
            this.groupBox_Miners.TabIndex = 389;
            this.groupBox_Miners.TabStop = false;
            this.groupBox_Miners.Text = "Miners:";
            // 
            // checkBox_MinimizeMiningWindows
            // 
            this.checkBox_MinimizeMiningWindows.AutoSize = true;
            this.checkBox_MinimizeMiningWindows.Location = new System.Drawing.Point(4, 129);
            this.checkBox_MinimizeMiningWindows.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_MinimizeMiningWindows.Name = "checkBox_MinimizeMiningWindows";
            this.checkBox_MinimizeMiningWindows.Size = new System.Drawing.Size(147, 17);
            this.checkBox_MinimizeMiningWindows.TabIndex = 388;
            this.checkBox_MinimizeMiningWindows.Text = "Minimize Mining Windows";
            this.checkBox_MinimizeMiningWindows.UseVisualStyleBackColor = true;
            // 
            // pictureBox_MinimizeMiningWindows
            // 
            this.pictureBox_MinimizeMiningWindows.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinimizeMiningWindows.Image")));
            this.pictureBox_MinimizeMiningWindows.Location = new System.Drawing.Point(164, 131);
            this.pictureBox_MinimizeMiningWindows.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinimizeMiningWindows.Name = "pictureBox_MinimizeMiningWindows";
            this.pictureBox_MinimizeMiningWindows.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinimizeMiningWindows.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinimizeMiningWindows.TabIndex = 389;
            this.pictureBox_MinimizeMiningWindows.TabStop = false;
            // 
            // checkBox_HideMiningWindows
            // 
            this.checkBox_HideMiningWindows.AutoSize = true;
            this.checkBox_HideMiningWindows.Location = new System.Drawing.Point(4, 109);
            this.checkBox_HideMiningWindows.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_HideMiningWindows.Name = "checkBox_HideMiningWindows";
            this.checkBox_HideMiningWindows.Size = new System.Drawing.Size(129, 17);
            this.checkBox_HideMiningWindows.TabIndex = 386;
            this.checkBox_HideMiningWindows.Text = "Hide Mining Windows";
            this.checkBox_HideMiningWindows.UseVisualStyleBackColor = true;
            // 
            // pictureBox_HideMiningWindows
            // 
            this.pictureBox_HideMiningWindows.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_HideMiningWindows.Image")));
            this.pictureBox_HideMiningWindows.Location = new System.Drawing.Point(164, 109);
            this.pictureBox_HideMiningWindows.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_HideMiningWindows.Name = "pictureBox_HideMiningWindows";
            this.pictureBox_HideMiningWindows.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_HideMiningWindows.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_HideMiningWindows.TabIndex = 387;
            this.pictureBox_HideMiningWindows.TabStop = false;
            // 
            // pictureBox_MinerRestartDelayMS
            // 
            this.pictureBox_MinerRestartDelayMS.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinerRestartDelayMS.Image")));
            this.pictureBox_MinerRestartDelayMS.Location = new System.Drawing.Point(156, 20);
            this.pictureBox_MinerRestartDelayMS.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinerRestartDelayMS.Name = "pictureBox_MinerRestartDelayMS";
            this.pictureBox_MinerRestartDelayMS.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinerRestartDelayMS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinerRestartDelayMS.TabIndex = 385;
            this.pictureBox_MinerRestartDelayMS.TabStop = false;
            // 
            // pictureBox_APIBindPortStart
            // 
            this.pictureBox_APIBindPortStart.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_APIBindPortStart.Image")));
            this.pictureBox_APIBindPortStart.Location = new System.Drawing.Point(341, 21);
            this.pictureBox_APIBindPortStart.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_APIBindPortStart.Name = "pictureBox_APIBindPortStart";
            this.pictureBox_APIBindPortStart.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_APIBindPortStart.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_APIBindPortStart.TabIndex = 385;
            this.pictureBox_APIBindPortStart.TabStop = false;
            // 
            // pictureBox_MinerAPIQueryInterval
            // 
            this.pictureBox_MinerAPIQueryInterval.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinerAPIQueryInterval.Image")));
            this.pictureBox_MinerAPIQueryInterval.Location = new System.Drawing.Point(162, 63);
            this.pictureBox_MinerAPIQueryInterval.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinerAPIQueryInterval.Name = "pictureBox_MinerAPIQueryInterval";
            this.pictureBox_MinerAPIQueryInterval.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinerAPIQueryInterval.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinerAPIQueryInterval.TabIndex = 385;
            this.pictureBox_MinerAPIQueryInterval.TabStop = false;
            // 
            // label_MinerAPIQueryInterval
            // 
            this.label_MinerAPIQueryInterval.AutoSize = true;
            this.label_MinerAPIQueryInterval.Location = new System.Drawing.Point(4, 61);
            this.label_MinerAPIQueryInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_MinerAPIQueryInterval.Name = "label_MinerAPIQueryInterval";
            this.label_MinerAPIQueryInterval.Size = new System.Drawing.Size(139, 13);
            this.label_MinerAPIQueryInterval.TabIndex = 376;
            this.label_MinerAPIQueryInterval.Text = "Miner API Query Interval [s]:";
            // 
            // label_MinerRestartDelayMS
            // 
            this.label_MinerRestartDelayMS.AutoSize = true;
            this.label_MinerRestartDelayMS.Location = new System.Drawing.Point(4, 20);
            this.label_MinerRestartDelayMS.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_MinerRestartDelayMS.Name = "label_MinerRestartDelayMS";
            this.label_MinerRestartDelayMS.Size = new System.Drawing.Size(125, 13);
            this.label_MinerRestartDelayMS.TabIndex = 375;
            this.label_MinerRestartDelayMS.Text = "Miner Restart Delay [ms]:";
            // 
            // label_APIBindPortStart
            // 
            this.label_APIBindPortStart.AutoSize = true;
            this.label_APIBindPortStart.Location = new System.Drawing.Point(189, 21);
            this.label_APIBindPortStart.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_APIBindPortStart.Name = "label_APIBindPortStart";
            this.label_APIBindPortStart.Size = new System.Drawing.Size(118, 13);
            this.label_APIBindPortStart.TabIndex = 357;
            this.label_APIBindPortStart.Text = "API Bind port pool start:";
            // 
            // textBox_APIBindPortStart
            // 
            this.textBox_APIBindPortStart.Location = new System.Drawing.Point(189, 38);
            this.textBox_APIBindPortStart.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_APIBindPortStart.Name = "textBox_APIBindPortStart";
            this.textBox_APIBindPortStart.Size = new System.Drawing.Size(172, 20);
            this.textBox_APIBindPortStart.TabIndex = 334;
            // 
            // textBox_MinerRestartDelayMS
            // 
            this.textBox_MinerRestartDelayMS.Location = new System.Drawing.Point(4, 38);
            this.textBox_MinerRestartDelayMS.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_MinerRestartDelayMS.Name = "textBox_MinerRestartDelayMS";
            this.textBox_MinerRestartDelayMS.Size = new System.Drawing.Size(172, 20);
            this.textBox_MinerRestartDelayMS.TabIndex = 340;
            // 
            // textBox_MinerAPIQueryInterval
            // 
            this.textBox_MinerAPIQueryInterval.Location = new System.Drawing.Point(4, 82);
            this.textBox_MinerAPIQueryInterval.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_MinerAPIQueryInterval.Name = "textBox_MinerAPIQueryInterval";
            this.textBox_MinerAPIQueryInterval.Size = new System.Drawing.Size(178, 20);
            this.textBox_MinerAPIQueryInterval.TabIndex = 341;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label_IFTTTAPIKey);
            this.groupBox4.Controls.Add(this.textBox_IFTTTKey);
            this.groupBox4.Controls.Add(this.pictureBox_UseIFTTT);
            this.groupBox4.Controls.Add(this.checkBox_UseIFTTT);
            this.groupBox4.Location = new System.Drawing.Point(3, 929);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(390, 62);
            this.groupBox4.TabIndex = 394;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "IFTTT:";
            // 
            // label_IFTTTAPIKey
            // 
            this.label_IFTTTAPIKey.AutoSize = true;
            this.label_IFTTTAPIKey.Location = new System.Drawing.Point(152, 15);
            this.label_IFTTTAPIKey.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_IFTTTAPIKey.Name = "label_IFTTTAPIKey";
            this.label_IFTTTAPIKey.Size = new System.Drawing.Size(81, 13);
            this.label_IFTTTAPIKey.TabIndex = 373;
            this.label_IFTTTAPIKey.Text = "IFTTT API Key:";
            // 
            // textBox_IFTTTKey
            // 
            this.textBox_IFTTTKey.Enabled = false;
            this.textBox_IFTTTKey.Location = new System.Drawing.Point(155, 31);
            this.textBox_IFTTTKey.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_IFTTTKey.Name = "textBox_IFTTTKey";
            this.textBox_IFTTTKey.Size = new System.Drawing.Size(210, 20);
            this.textBox_IFTTTKey.TabIndex = 372;
            // 
            // pictureBox_UseIFTTT
            // 
            this.pictureBox_UseIFTTT.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_UseIFTTT.Image")));
            this.pictureBox_UseIFTTT.Location = new System.Drawing.Point(83, 15);
            this.pictureBox_UseIFTTT.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_UseIFTTT.Name = "pictureBox_UseIFTTT";
            this.pictureBox_UseIFTTT.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_UseIFTTT.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_UseIFTTT.TabIndex = 371;
            this.pictureBox_UseIFTTT.TabStop = false;
            // 
            // checkBox_UseIFTTT
            // 
            this.checkBox_UseIFTTT.AutoSize = true;
            this.checkBox_UseIFTTT.Location = new System.Drawing.Point(5, 19);
            this.checkBox_UseIFTTT.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_UseIFTTT.Name = "checkBox_UseIFTTT";
            this.checkBox_UseIFTTT.Size = new System.Drawing.Size(78, 17);
            this.checkBox_UseIFTTT.TabIndex = 370;
            this.checkBox_UseIFTTT.Text = "Use IFTTT";
            this.checkBox_UseIFTTT.UseVisualStyleBackColor = true;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.groupBox_Main);
            this.tabPageGeneral.Controls.Add(this.groupBox_Localization);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabPageGeneral.Size = new System.Drawing.Size(599, 439);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox_Main
            // 
            this.groupBox_Main.Controls.Add(this.checkBox_MineRegardlessOfProfit);
            this.groupBox_Main.Controls.Add(this.pictureBox_MineRegardlessOfProfit);
            this.groupBox_Main.Controls.Add(this.pictureBox_ElectricityCost);
            this.groupBox_Main.Controls.Add(this.textBox_ElectricityCost);
            this.groupBox_Main.Controls.Add(this.label_ElectricityCost);
            this.groupBox_Main.Controls.Add(this.pictureBox_TimeUnit);
            this.groupBox_Main.Controls.Add(this.label_TimeUnit);
            this.groupBox_Main.Controls.Add(this.comboBox_TimeUnit);
            this.groupBox_Main.Controls.Add(this.pictureBox_MinProfit);
            this.groupBox_Main.Controls.Add(this.textBox_MinProfit);
            this.groupBox_Main.Controls.Add(this.label_MinProfit);
            this.groupBox_Main.Location = new System.Drawing.Point(6, 6);
            this.groupBox_Main.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Main.Name = "groupBox_Main";
            this.groupBox_Main.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Main.Size = new System.Drawing.Size(344, 156);
            this.groupBox_Main.TabIndex = 386;
            this.groupBox_Main.TabStop = false;
            this.groupBox_Main.Text = "Main:";
            // 
            // checkBox_MineRegardlessOfProfit
            // 
            this.checkBox_MineRegardlessOfProfit.AutoSize = true;
            this.checkBox_MineRegardlessOfProfit.Location = new System.Drawing.Point(14, 22);
            this.checkBox_MineRegardlessOfProfit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_MineRegardlessOfProfit.Name = "checkBox_MineRegardlessOfProfit";
            this.checkBox_MineRegardlessOfProfit.Size = new System.Drawing.Size(146, 17);
            this.checkBox_MineRegardlessOfProfit.TabIndex = 376;
            this.checkBox_MineRegardlessOfProfit.Text = "Mine Regardless Of Profit";
            this.checkBox_MineRegardlessOfProfit.UseVisualStyleBackColor = true;
            // 
            // pictureBox_MineRegardlessOfProfit
            // 
            this.pictureBox_MineRegardlessOfProfit.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MineRegardlessOfProfit.Image")));
            this.pictureBox_MineRegardlessOfProfit.Location = new System.Drawing.Point(201, 22);
            this.pictureBox_MineRegardlessOfProfit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MineRegardlessOfProfit.Name = "pictureBox_MineRegardlessOfProfit";
            this.pictureBox_MineRegardlessOfProfit.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MineRegardlessOfProfit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MineRegardlessOfProfit.TabIndex = 377;
            this.pictureBox_MineRegardlessOfProfit.TabStop = false;
            // 
            // pictureBox_ElectricityCost
            // 
            this.pictureBox_ElectricityCost.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_ElectricityCost.Image")));
            this.pictureBox_ElectricityCost.Location = new System.Drawing.Point(131, 101);
            this.pictureBox_ElectricityCost.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_ElectricityCost.Name = "pictureBox_ElectricityCost";
            this.pictureBox_ElectricityCost.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_ElectricityCost.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_ElectricityCost.TabIndex = 375;
            this.pictureBox_ElectricityCost.TabStop = false;
            // 
            // textBox_ElectricityCost
            // 
            this.textBox_ElectricityCost.Location = new System.Drawing.Point(11, 121);
            this.textBox_ElectricityCost.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_ElectricityCost.Name = "textBox_ElectricityCost";
            this.textBox_ElectricityCost.Size = new System.Drawing.Size(138, 20);
            this.textBox_ElectricityCost.TabIndex = 373;
            // 
            // label_ElectricityCost
            // 
            this.label_ElectricityCost.AutoSize = true;
            this.label_ElectricityCost.Location = new System.Drawing.Point(11, 101);
            this.label_ElectricityCost.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_ElectricityCost.Name = "label_ElectricityCost";
            this.label_ElectricityCost.Size = new System.Drawing.Size(117, 13);
            this.label_ElectricityCost.TabIndex = 374;
            this.label_ElectricityCost.Text = "Electricity Cost (/KWh):";
            // 
            // pictureBox_TimeUnit
            // 
            this.pictureBox_TimeUnit.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_TimeUnit.Image")));
            this.pictureBox_TimeUnit.Location = new System.Drawing.Point(303, 50);
            this.pictureBox_TimeUnit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_TimeUnit.Name = "pictureBox_TimeUnit";
            this.pictureBox_TimeUnit.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_TimeUnit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_TimeUnit.TabIndex = 372;
            this.pictureBox_TimeUnit.TabStop = false;
            // 
            // label_TimeUnit
            // 
            this.label_TimeUnit.AutoSize = true;
            this.label_TimeUnit.Location = new System.Drawing.Point(161, 50);
            this.label_TimeUnit.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_TimeUnit.Name = "label_TimeUnit";
            this.label_TimeUnit.Size = new System.Drawing.Size(55, 13);
            this.label_TimeUnit.TabIndex = 371;
            this.label_TimeUnit.Text = "Time Unit:";
            // 
            // comboBox_TimeUnit
            // 
            this.comboBox_TimeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TimeUnit.FormattingEnabled = true;
            this.comboBox_TimeUnit.Location = new System.Drawing.Point(161, 71);
            this.comboBox_TimeUnit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_TimeUnit.Name = "comboBox_TimeUnit";
            this.comboBox_TimeUnit.Size = new System.Drawing.Size(160, 21);
            this.comboBox_TimeUnit.TabIndex = 370;
            // 
            // pictureBox_MinProfit
            // 
            this.pictureBox_MinProfit.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_MinProfit.Image")));
            this.pictureBox_MinProfit.Location = new System.Drawing.Point(131, 51);
            this.pictureBox_MinProfit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_MinProfit.Name = "pictureBox_MinProfit";
            this.pictureBox_MinProfit.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_MinProfit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_MinProfit.TabIndex = 364;
            this.pictureBox_MinProfit.TabStop = false;
            // 
            // textBox_MinProfit
            // 
            this.textBox_MinProfit.Location = new System.Drawing.Point(11, 71);
            this.textBox_MinProfit.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.textBox_MinProfit.Name = "textBox_MinProfit";
            this.textBox_MinProfit.Size = new System.Drawing.Size(138, 20);
            this.textBox_MinProfit.TabIndex = 334;
            // 
            // label_MinProfit
            // 
            this.label_MinProfit.AutoSize = true;
            this.label_MinProfit.Location = new System.Drawing.Point(11, 51);
            this.label_MinProfit.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_MinProfit.Name = "label_MinProfit";
            this.label_MinProfit.Size = new System.Drawing.Size(115, 13);
            this.label_MinProfit.TabIndex = 357;
            this.label_MinProfit.Text = "Minimum Profit ($/day):";
            // 
            // groupBox_Localization
            // 
            this.groupBox_Localization.Controls.Add(this.label_Language);
            this.groupBox_Localization.Controls.Add(this.pictureBox5);
            this.groupBox_Localization.Controls.Add(this.pictureBox_displayCurrency);
            this.groupBox_Localization.Controls.Add(this.pictureBox_Language);
            this.groupBox_Localization.Controls.Add(this.comboBox_Language);
            this.groupBox_Localization.Controls.Add(this.currencyConverterCombobox);
            this.groupBox_Localization.Controls.Add(this.label_displayCurrency);
            this.groupBox_Localization.Location = new System.Drawing.Point(354, 6);
            this.groupBox_Localization.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Localization.Name = "groupBox_Localization";
            this.groupBox_Localization.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.groupBox_Localization.Size = new System.Drawing.Size(236, 128);
            this.groupBox_Localization.TabIndex = 385;
            this.groupBox_Localization.TabStop = false;
            this.groupBox_Localization.Text = "Localization:";
            // 
            // label_Language
            // 
            this.label_Language.AutoSize = true;
            this.label_Language.Location = new System.Drawing.Point(6, 16);
            this.label_Language.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_Language.Name = "label_Language";
            this.label_Language.Size = new System.Drawing.Size(58, 13);
            this.label_Language.TabIndex = 358;
            this.label_Language.Text = "Language:";
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox5.Image")));
            this.pictureBox5.Location = new System.Drawing.Point(-58, 59);
            this.pictureBox5.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(18, 18);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox5.TabIndex = 364;
            this.pictureBox5.TabStop = false;
            // 
            // pictureBox_displayCurrency
            // 
            this.pictureBox_displayCurrency.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_displayCurrency.Image")));
            this.pictureBox_displayCurrency.Location = new System.Drawing.Point(178, 71);
            this.pictureBox_displayCurrency.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_displayCurrency.Name = "pictureBox_displayCurrency";
            this.pictureBox_displayCurrency.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_displayCurrency.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_displayCurrency.TabIndex = 364;
            this.pictureBox_displayCurrency.TabStop = false;
            // 
            // pictureBox_Language
            // 
            this.pictureBox_Language.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_Language.Image")));
            this.pictureBox_Language.Location = new System.Drawing.Point(178, 16);
            this.pictureBox_Language.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pictureBox_Language.Name = "pictureBox_Language";
            this.pictureBox_Language.Size = new System.Drawing.Size(18, 18);
            this.pictureBox_Language.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox_Language.TabIndex = 364;
            this.pictureBox_Language.TabStop = false;
            // 
            // comboBox_Language
            // 
            this.comboBox_Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Language.FormattingEnabled = true;
            this.comboBox_Language.Location = new System.Drawing.Point(6, 36);
            this.comboBox_Language.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.comboBox_Language.Name = "comboBox_Language";
            this.comboBox_Language.Size = new System.Drawing.Size(190, 21);
            this.comboBox_Language.TabIndex = 328;
            // 
            // currencyConverterCombobox
            // 
            this.currencyConverterCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.currencyConverterCombobox.FormattingEnabled = true;
            this.currencyConverterCombobox.Items.AddRange(new object[] {
            "AUD",
            "BGN",
            "BRL",
            "CAD",
            "CHF",
            "CNY",
            "CZK",
            "DKK",
            "EUR",
            "GBP",
            "HKD",
            "HRK",
            "HUF",
            "IDR",
            "ILS",
            "INR",
            "JPY",
            "KRW",
            "MXN",
            "MYR",
            "NOK",
            "NZD",
            "PHP",
            "PLN",
            "RON",
            "RUB",
            "SEK",
            "SGD",
            "THB",
            "TRY",
            "USD",
            "ZAR"});
            this.currencyConverterCombobox.Location = new System.Drawing.Point(6, 91);
            this.currencyConverterCombobox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.currencyConverterCombobox.Name = "currencyConverterCombobox";
            this.currencyConverterCombobox.Size = new System.Drawing.Size(190, 21);
            this.currencyConverterCombobox.Sorted = true;
            this.currencyConverterCombobox.TabIndex = 381;
            this.currencyConverterCombobox.SelectedIndexChanged += new System.EventHandler(this.CurrencyConverterCombobox_SelectedIndexChanged);
            // 
            // label_displayCurrency
            // 
            this.label_displayCurrency.AutoSize = true;
            this.label_displayCurrency.Location = new System.Drawing.Point(6, 71);
            this.label_displayCurrency.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_displayCurrency.Name = "label_displayCurrency";
            this.label_displayCurrency.Size = new System.Drawing.Size(89, 13);
            this.label_displayCurrency.TabIndex = 382;
            this.label_displayCurrency.Text = "Display Currency:";
            // 
            // tabControlGeneral
            // 
            this.tabControlGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlGeneral.Controls.Add(this.tabPageGeneral);
            this.tabControlGeneral.Controls.Add(this.tabPageAdvanced);
            this.tabControlGeneral.Location = new System.Drawing.Point(12, 11);
            this.tabControlGeneral.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tabControlGeneral.Name = "tabControlGeneral";
            this.tabControlGeneral.SelectedIndex = 0;
            this.tabControlGeneral.Size = new System.Drawing.Size(607, 465);
            this.tabControlGeneral.TabIndex = 0;
            // 
            // Form_Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 519);
            this.Controls.Add(this.buttonDefaults);
            this.Controls.Add(this.buttonSaveClose);
            this.Controls.Add(this.tabControlGeneral);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(647, 525);
            this.Name = "Form_Settings";
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.tabPageAdvanced.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox_Misc.ResumeLayout(false);
            this.groupBox_Misc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowGPUPCIeBusIDs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_WindowAlwaysOnTop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunAtStartup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AllowMultipleInstances)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowInternetConnectionWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableWindowsErrorReporting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ShowDriverVersionWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AutoScaleBTCValues)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_AutoStartMining)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinimizeToTray)).EndInit();
            this.groupBoxDeviceMonitoring.ResumeLayout(false);
            this.groupBoxDeviceMonitoring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableDevicePowerModeSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DisableDeviceStatusMonitoring)).EndInit();
            this.groupBox_Logging.ResumeLayout(false);
            this.groupBox_Logging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_DebugConsole)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LogMaxFileSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_LogToFile)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_IdleWhenNoInternetAccess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_StartMiningWhenIdle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinIdleSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_IdleType)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunScriptOnCUDA_GPU_Lost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_NVIDIAP0State)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_RunEthlargement)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchProfitabilityThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchMaxSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_SwitchMinSeconds)).EndInit();
            this.groupBox_Miners.ResumeLayout(false);
            this.groupBox_Miners.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinimizeMiningWindows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_HideMiningWindows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinerRestartDelayMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_APIBindPortStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinerAPIQueryInterval)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_UseIFTTT)).EndInit();
            this.tabPageGeneral.ResumeLayout(false);
            this.groupBox_Main.ResumeLayout(false);
            this.groupBox_Main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MineRegardlessOfProfit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ElectricityCost)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_TimeUnit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_MinProfit)).EndInit();
            this.groupBox_Localization.ResumeLayout(false);
            this.groupBox_Localization.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_displayCurrency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Language)).EndInit();
            this.tabControlGeneral.ResumeLayout(false);
            this.ResumeLayout(false);

        }

#endregion

        private System.Windows.Forms.Button buttonSaveClose;
        private System.Windows.Forms.Button buttonDefaults;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.TabControl tabControlGeneral;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.GroupBox groupBox_Main;
        private System.Windows.Forms.CheckBox checkBox_MineRegardlessOfProfit;
        private System.Windows.Forms.PictureBox pictureBox_MineRegardlessOfProfit;
        private System.Windows.Forms.PictureBox pictureBox_ElectricityCost;
        private System.Windows.Forms.TextBox textBox_ElectricityCost;
        private System.Windows.Forms.Label label_ElectricityCost;
        private System.Windows.Forms.PictureBox pictureBox_TimeUnit;
        private System.Windows.Forms.Label label_TimeUnit;
        private System.Windows.Forms.ComboBox comboBox_TimeUnit;
        private System.Windows.Forms.PictureBox pictureBox_MinProfit;
        private System.Windows.Forms.TextBox textBox_MinProfit;
        private System.Windows.Forms.Label label_MinProfit;
        private System.Windows.Forms.GroupBox groupBox_Localization;
        private System.Windows.Forms.Label label_Language;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.PictureBox pictureBox_displayCurrency;
        private System.Windows.Forms.PictureBox pictureBox_Language;
        private System.Windows.Forms.ComboBox comboBox_Language;
        private System.Windows.Forms.ComboBox currencyConverterCombobox;
        private System.Windows.Forms.Label label_displayCurrency;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox_Misc;
        private System.Windows.Forms.PictureBox pictureBox_ShowGPUPCIeBusIDs;
        private System.Windows.Forms.CheckBox checkBox_ShowGPUPCIeBusIDs;
        private System.Windows.Forms.PictureBox pictureBox_WindowAlwaysOnTop;
        private System.Windows.Forms.CheckBox checkBox_WindowAlwaysOnTop;
        private System.Windows.Forms.PictureBox pictureBox_RunAtStartup;
        private System.Windows.Forms.CheckBox checkBox_RunAtStartup;
        private System.Windows.Forms.CheckBox checkBox_AllowMultipleInstances;
        private System.Windows.Forms.PictureBox pictureBox_AllowMultipleInstances;
        private System.Windows.Forms.PictureBox pictureBox_ShowInternetConnectionWarning;
        private System.Windows.Forms.CheckBox checkBox_ShowInternetConnectionWarning;
        private System.Windows.Forms.CheckBox checkBox_AutoStartMining;
        private System.Windows.Forms.CheckBox checkBox_MinimizeToTray;
        private System.Windows.Forms.PictureBox pictureBox_DisableWindowsErrorReporting;
        private System.Windows.Forms.PictureBox pictureBox_ShowDriverVersionWarning;
        private System.Windows.Forms.PictureBox pictureBox_AutoScaleBTCValues;
        private System.Windows.Forms.PictureBox pictureBox_AutoStartMining;
        private System.Windows.Forms.PictureBox pictureBox_MinimizeToTray;
        private System.Windows.Forms.CheckBox checkBox_AutoScaleBTCValues;
        private System.Windows.Forms.CheckBox checkBox_DisableWindowsErrorReporting;
        private System.Windows.Forms.CheckBox checkBox_ShowDriverVersionWarning;
        private System.Windows.Forms.GroupBox groupBoxDeviceMonitoring;
        private System.Windows.Forms.CheckBox checkBox_DisableDevicePowerModeSettings;
        private System.Windows.Forms.PictureBox pictureBox_DisableDevicePowerModeSettings;
        private System.Windows.Forms.CheckBox checkBox_DisableDeviceStatusMonitoring;
        private System.Windows.Forms.PictureBox pictureBox_DisableDeviceStatusMonitoring;
        private System.Windows.Forms.GroupBox groupBox_Logging;
        private System.Windows.Forms.Label label_LogMaxFileSize;
        private System.Windows.Forms.TextBox textBox_LogMaxFileSize;
        private System.Windows.Forms.CheckBox checkBox_LogToFile;
        private System.Windows.Forms.PictureBox pictureBox_DebugConsole;
        private System.Windows.Forms.PictureBox pictureBox_LogMaxFileSize;
        private System.Windows.Forms.PictureBox pictureBox_LogToFile;
        private System.Windows.Forms.CheckBox checkBox_DebugConsole;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_IdleWhenNoInternetAccess;
        private System.Windows.Forms.PictureBox pictureBox_IdleWhenNoInternetAccess;
        private System.Windows.Forms.PictureBox pictureBox_StartMiningWhenIdle;
        private System.Windows.Forms.CheckBox checkBox_StartMiningWhenIdle;
        private System.Windows.Forms.PictureBox pictureBox_MinIdleSeconds;
        private System.Windows.Forms.Label label_MinIdleSeconds;
        private System.Windows.Forms.TextBox textBox_MinIdleSeconds;
        private System.Windows.Forms.PictureBox pictureBox_IdleType;
        private System.Windows.Forms.ComboBox comboBox_IdleType;
        private System.Windows.Forms.Label label_IdleType;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.PictureBox pictureBox_RunScriptOnCUDA_GPU_Lost;
        private System.Windows.Forms.CheckBox checkBox_RunScriptOnCUDA_GPU_Lost;
        private System.Windows.Forms.PictureBox pictureBox_NVIDIAP0State;
        private System.Windows.Forms.CheckBox checkBox_NVIDIAP0State;
        private System.Windows.Forms.PictureBox pictureBox_RunEthlargement;
        private System.Windows.Forms.CheckBox checkBox_RunEthlargement;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.PictureBox pictureBox_SwitchProfitabilityThreshold;
        private System.Windows.Forms.TextBox textBox_SwitchProfitabilityThreshold;
        private System.Windows.Forms.Label label_SwitchProfitabilityThreshold;
        private System.Windows.Forms.Label label_SwitchMinSeconds;
        private System.Windows.Forms.TextBox textBox_SwitchMinSeconds;
        private System.Windows.Forms.PictureBox pictureBox_SwitchMaxSeconds;
        private System.Windows.Forms.PictureBox pictureBox_SwitchMinSeconds;
        private System.Windows.Forms.Label label_SwitchMaxSeconds;
        private System.Windows.Forms.TextBox textBox_SwitchMaxSeconds;
        private System.Windows.Forms.GroupBox groupBox_Miners;
        private System.Windows.Forms.CheckBox checkBox_MinimizeMiningWindows;
        private System.Windows.Forms.PictureBox pictureBox_MinimizeMiningWindows;
        private System.Windows.Forms.CheckBox checkBox_HideMiningWindows;
        private System.Windows.Forms.PictureBox pictureBox_HideMiningWindows;
        private System.Windows.Forms.PictureBox pictureBox_MinerRestartDelayMS;
        private System.Windows.Forms.PictureBox pictureBox_APIBindPortStart;
        private System.Windows.Forms.PictureBox pictureBox_MinerAPIQueryInterval;
        private System.Windows.Forms.Label label_MinerAPIQueryInterval;
        private System.Windows.Forms.Label label_MinerRestartDelayMS;
        private System.Windows.Forms.Label label_APIBindPortStart;
        private System.Windows.Forms.TextBox textBox_APIBindPortStart;
        private System.Windows.Forms.TextBox textBox_MinerRestartDelayMS;
        private System.Windows.Forms.TextBox textBox_MinerAPIQueryInterval;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label_IFTTTAPIKey;
        private System.Windows.Forms.TextBox textBox_IFTTTKey;
        private System.Windows.Forms.PictureBox pictureBox_UseIFTTT;
        private System.Windows.Forms.CheckBox checkBox_UseIFTTT;
    }
}
