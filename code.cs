using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Note
{
    public partial class frmMain : Form
    {
        #region 변수
#if DEBUG
        public int _versionCounter = 0;
        public char _versionType = 'M';
#endif
        public bool firstSave = false;// 최초 저장 여부
        public bool textSave = false; // 텍스트 파일로 저장 여부

        public string _name = "무제";
        public string _path = null;
        public bool isTextChanged = false;

        public static string _pageVersion = "1.25.10.9";
        public int saveVersion = 0;

        // UI 관련 변수
        public bool menuOpen = false;
        public bool timeLapOpen = false;
        public bool checkButton = false;

        public int[] targetX = new int[2];
        public double[] _speed = new double[2];
        public double[] _easing = new double[2];

        public int paddingL = 5;
        public int paddingR = 5;
        public int paddingTop = 2;

        public bool dragging = false;
        public Point dragCursorPoint;
        public Point dragFormPoint;
        public int memoLayoutWidth = 0;

        // 타임랩, 메모 관련
        public List<TimeLap> timeLaps = new List<TimeLap>();
        public List<Memo> memos = new List<Memo>();
        public int tempTimeLaps = 0;
        public int tempMemos = 0;

        public int selectedMemo = 0;

        public List<ContextMenuStrip> memoStripMenu = new List<ContextMenuStrip>();
        public List<ToolStripMenuItem> memoToolMenu = new List<ToolStripMenuItem>();


        //옵션 관련
        public Options option = new Options();

        //테마 관련
        public List<Thema> thema = new List<Thema>();

        public Color ButtonUp = new Color();
        public Color ButtonDown = new Color();

        //참조 관련
        public List<Label> referenceTimeLap = new List<Label>();
        public List<memoUI> uiMemo = new List<memoUI>();
        public List<timeLapUI> uiTime = new List<timeLapUI>();


        #endregion

        #region Form 관련
        public frmMain()
        {
            InitializeComponent();
            loadLabel.BringToFront();
#if DEBUG
            TempButton.Visible = true;


            _versionCounter = app.Default.counter;
            _versionCounter++;

            app.Default.counter = _versionCounter;
            app.Default.Save();

            _pageVersion = "1." + DateTime.Now.ToString("yy.MM.") + _versionCounter + _versionType;

            pageTextBox.Text = _pageVersion;
#endif

        }
        private void PageForm_Load(object sender, EventArgs e)
        {
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width - (Screen.PrimaryScreen.Bounds.Width / 4), Screen.PrimaryScreen.Bounds.Height - (Screen.PrimaryScreen.Bounds.Height / 4));
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);

            loadPanel.Location = new Point(0, 0);
            loadPanel.Size = this.Size;
        }
        private void PageForm_Shown(object sender, EventArgs e)
        {
            option.thema = app.Default.thema;
            option.fontName = app.Default.fontName;
            option.fontSize = app.Default.fontSize;
            option.memoSize = app.Default.memoSize;
            option.autoSave = app.Default.autoSave;
            option.autoCount = app.Default.autoCount;
            option.memoForm = app.Default.memoForm;

            menuPanel.Location = new Point(menuPanel.Width * -1, 0);
            mainPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

            timeLapPanel.Location = new Point(mainPanel.Width, 35);
            timeLapPanel.Height = mainPanel.Height - 35;
            pagePanel.Size = new Size(timeLapPanel.Location.X, mainPanel.Height - 65);
            menuPanel.Height = this.ClientSize.Height;
            memoLayoutPanel.Controls.Add(addMemoLabel);
            timeLapLayoutPanel.Height = timeLapPanel.Height - timeLapLayoutPanel.Location.Y - 12;

            timeLapLabel.Location = new Point(this.ClientSize.Width - 30, 0);
            memoLabel.Location = new Point(this.ClientSize.Width - 60, 0);
            wallLabel.Height = mainPanel.Height;
            wallLabel.Visible = false;

            optionLabel.Location = new Point(10, menuPanel.Height - 45);

            menuStrip.Visible = false;

            loadThema();
            changeSkin(option.thema);

            pageTextBox.Font = new Font(option.fontName, option.fontSize);
            wallline.Visible = false;
            isTextChanged = false;
            UpdateTitle();

            option.yoBack = app.Default.yoBack;
            switch (option.yoBack)
            {
                case 0:
                    paddingL = paddingR = 6;
                    break;
                case 1:
                    paddingL = paddingR = 30;
                    break;
                case 2:
                    paddingL = paddingR = this.ClientSize.Width / 10;
                    break;
                default:
                    break;
            }

            textBoxPanel.Padding = new Padding(paddingL, paddingTop, paddingR, 0);


            ToolTip tip = new ToolTip();

            tip.SetToolTip(newLabel, "Ctrl + N");
            tip.SetToolTip(openLabel, "Ctrl + O");
            tip.SetToolTip(saveLabel, "Ctrl + S");
            tip.SetToolTip(asSaveLabel, "Ctrl + SHIFT + S");
            tip.SetToolTip(optionLabel, "F1");

            tip.SetToolTip(timeLapLabel, "타임 랩 (Ctrl + T)\nF2 : 타임랩 찍기");
            tip.SetToolTip(memoLabel, "메모 (Ctrl + M)");

            tip.SetToolTip(menuOpenLabel, "메뉴 (F1)");

            /*
#if !DEBUG
            #region 업데이트 체크
            try
            {
                HttpClient update = new HttpClient();
                string[] updateContent = update.GetStringAsync("https://raw.githubusercontent.com/nahulEditor/BlankNote/main/Version").Result.Split("\n");

                if (_pageVersion != updateContent[0])
                {
                    DialogResult result = MessageBox.Show("업데이트 파일이 발견됐습니다. 다우론드 페이지로 이동하겠습니까?", "업데이트", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(updateContent[1]) { UseShellExecute = true });
                        Application.Exit();
                    }
                }
            }
            catch
            {
            }
            #endregion
#endif
            */
            loadPanel.Visible = false;
        }

        private void PageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if DEBUG
            if (pageTextBox.Text == _pageVersion)
                e.Cancel = false;
#endif
            if (isTextChanged && pageTextBox.Text != _pageVersion)
            {
                var result = MessageBox.Show("수정된 페이지를 저장하시겠습니까?", "안내", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                    e.Cancel = false;
                else if (result == DialogResult.No)
                    e.Cancel = false;
                else
                    e.Cancel = true;
            }
            if (e.Cancel == false)
            {
                app.Default.fontName = option.fontName;
                app.Default.fontSize = option.fontSize;
                app.Default.memoSize = option.memoSize;
                app.Default.thema = option.thema;
                app.Default.yoBack = option.yoBack;
                app.Default.autoSave = option.autoSave;
                app.Default.autoCount = option.autoCount;
                app.Default.memoForm = option.memoForm;

                app.Default.Save();
            }

        }

        private void PageForm_Resize(object sender, EventArgs e)
        {

            if (menuOpen)
                mainPanel.Width = this.ClientSize.Width - (menuPanel.Location.X + menuPanel.Width);
            else
                mainPanel.Width = this.ClientSize.Width;

            timeLapLabel.Location = new Point(this.ClientSize.Width - 30, 0);
            memoLabel.Location = new Point(this.ClientSize.Width - 60, 0);

            timeLapPanel.Location = new Point((timeLapOpen ? mainPanel.Width - timeLapPanel.Width : mainPanel.Width), 35);
            timeLapPanel.Height = mainPanel.Height - 35;
            pagePanel.Size = new Size(timeLapPanel.Location.X, mainPanel.Height - 65);
            menuPanel.Height = this.ClientSize.Height;

            wallLabel.Height = mainPanel.Height;
            wallLabel.Location = new Point(timeLapPanel.Location.X, 0);

            if (memoLayoutPanel.VerticalScroll.Visible)
                memoLayoutPanel.Width = 262;
            else
                memoLayoutPanel.Width = 244;

            timeLapLayoutPanel.Height = timeLapPanel.Height - timeLapLayoutPanel.Location.Y - 12;
            optionLabel.Location = new Point(10, menuPanel.Height - 45);
        }

        #endregion

        #region 타이머 관련
        private void InColTimer_Tick(object sender, EventArgs e)
        {
            StringBuilder _sb = new StringBuilder();

            _sb.Append("Ln ");
            _sb.Append(pageTextBox.GetLineFromCharIndex(pageTextBox.SelectionStart) + 1);
            _sb.Append(", Col ");
            _sb.Append(Math.Abs(pageTextBox.GetFirstCharIndexOfCurrentLine() - pageTextBox.SelectionStart));
            _sb.Append(", All ");
            _sb.Append(pageTextBox.Text.Length);

            if (lnColLabel.Text != _sb.ToString())
                lnColLabel.Text = _sb.ToString();

            _sb.Clear();


            if (publicVar.chagedText)
            {
                if (!isTextChanged)
                {
                    isTextChanged = true;
                    UpdateTitle();
                }
            }
        }


        private void menuAnimation_Tick(object sender, EventArgs e)
        {
            double dx = (targetX[0] - menuPanel.Left) * _speed[0];
            double dy = (0 - menuPanel.Top) * _speed[0];

            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
            {
                menuPanel.Left = targetX[0];
                menuPanel.Top = 0;

                if (menuOpen)
                    mainPanel.Size = new Size(this.ClientSize.Width - menuPanel.Width, this.ClientSize.Height);
                else
                    mainPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);

                if (timeLapOpen)
                {
                    timeLapPanel.Location = new Point(mainPanel.Width - timeLapPanel.Width, 35);
                    wallLabel.Location = new Point(timeLapPanel.Location.X, 0);
                }
                else
                    timeLapPanel.Location = new Point(mainPanel.Width, 35);

                pagePanel.Width = timeLapPanel.Location.X;


                menuAnimation.Stop();
            }
            else
            {
                menuPanel.Left += (int)dx;
                menuPanel.Top += (int)dy;
                //mainPanel.Location = new Point(menuPanel.Location.X + menuPanel.Width, 0);
                mainPanel.Size = new Size(this.ClientSize.Width - (menuPanel.Location.X + menuPanel.Width), this.ClientSize.Height);

                if (timeLapOpen)
                {
                    wallLabel.Location = new Point(timeLapPanel.Location.X, 0);
                    timeLapPanel.Location = new Point(mainPanel.Width - timeLapPanel.Width, 35);
                }
                else
                    timeLapPanel.Location = new Point(mainPanel.Width, 35);

                pagePanel.Width = timeLapPanel.Location.X;

                // 이징 효과 적용
                _speed[0] -= _easing[0];
                if (_speed[0] < 0) _speed[0] = 0;
            }
        }

        private void timeLapAnimation_Tick(object sender, EventArgs e)
        {
            double dx = (targetX[1] - timeLapPanel.Left) * _speed[1];
            double dy = (35 - timeLapPanel.Top) * _speed[1];

            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
            {
                timeLapPanel.Left = targetX[1];
                timeLapPanel.Top = 35;
                pagePanel.Width = timeLapPanel.Location.X;
                wallLabel.Location = new Point(timeLapPanel.Location.X, 0);

                if (!timeLapOpen)
                    wallLabel.Visible = false;

                timeLapAnimation.Stop();
            }
            else
            {
                timeLapPanel.Left += (int)dx;
                timeLapPanel.Top += (int)dy;
                pagePanel.Width = timeLapPanel.Location.X;
                wallLabel.Location = new Point(timeLapPanel.Location.X, 0);
                // 이징 효과 적용
                _speed[1] -= _easing[1];
                if (_speed[1] < 0) _speed[1] = 0;
            }
        }

        int timerCounter = -1;
        private void autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!option.autoSave)
                autoSaveTimer.Stop();
            timerCounter++;

            if (timerCounter >= option.autoCount)
            {
                if (_path != null)
                {
                    savePage();
                }
                timerCounter = -1;
            }
        }
        #endregion

        #region 버튼 관련 코드

        private void timeLapLabel_Click(object sender, EventArgs e)
        {
            if (!timeLapAnimation.Enabled)
            {
                timeLapOpen = !timeLapOpen;
                _speed[1] = 0.5;
                _easing[1] = 0.01;

                if (timeLapOpen)
                {
                    wallLabel.Visible = true;
                    targetX[1] = mainPanel.Width - timeLapPanel.Width;
                }
                else
                    targetX[1] = mainPanel.Width;

                timeLapAnimation.Start();
            }
        }

        private void memoLabel_Click(object sender, EventArgs e)
        {

            if (!option.memoForm)
            {
                wallline.Visible = memoLayoutPanel.Visible = !memoLayoutPanel.Visible;

                if (memoLayoutPanel.Visible)
                    paddingR = 5;
                else
                    paddingR = paddingL;

                textBoxPanel.Padding = new Padding(paddingL, paddingTop, paddingR, 0);

                UpdateMemo();
            }
            else
            {
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm.Name == "frmMemo")
                    {
                        openForm.BringToFront();
                        return;
                    }
                }

                if (memoLayoutPanel.Visible)
                    wallline.Visible = memoLayoutPanel.Visible = false;

                frmMemo mForm = new frmMemo(memos, thema[option.thema], this, memoStripMenu, memoToolMenu, option);

                mForm.Show();
            }
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (!menuAnimation.Enabled)
            {
                menuOpen = !menuOpen;
                _speed[0] = 0.5;
                _easing[0] = 0.01;

                menuOpenLabel.Visible = !menuOpenLabel.Visible;

                if (menuOpen)
                {
                    nameLabel.Padding = new Padding(8, 0, 0, 0);
                    targetX[0] = 0;
                }
                else
                {
                    nameLabel.Padding = new Padding(0, 0, 0, 0);
                    targetX[0] = menuPanel.Width * -1;
                }
                menuAnimation.Start();
            }
        }

        private void addMemoLabel_Click(object sender, EventArgs e)
        {
            memoLayoutPanel.Controls.Remove(addMemoLabel);
            Memo addMemo = new Memo();
            memos.Add(addMemo);

            memoUI add = new memoUI(memos, tempMemos, this.BackColor, this.ForeColor, thema[option.thema].BorderColor, option.fontSize, option.fontName);
            ContextMenuStrip addMenu = new ContextMenuStrip()
            {
                Tag = tempMemos,
                BackColor = Color.FromArgb(254, 254, 254),
                ShowImageMargin = false
            };
            ToolStripMenuItem addTool = new ToolStripMenuItem()
            {
                Text = "⌦ 삭제",
                Font = new Font("나눔 고딕", 10)
            };

            addMenu.Items.Add(addTool);
            add.Content.ContextMenuStrip = addMenu;

            addMenu.Opening += memoStrip_Opening;
            addTool.Click += memoTool_Click;

            memoStripMenu.Add(addMenu);
            memoToolMenu.Add(addTool);
            uiMemo.Add(add);
            memoLayoutPanel.Controls.Add(uiMemo[tempMemos].Panel);
            tempMemos = tempMemos + 1;

            memoLayoutPanel.Controls.Add(addMemoLabel);

            changeMemoSize();
        }

        #endregion

        #region 핫키 코드
        private void dokangMenuStrip_Click(object sender, EventArgs e)
        {
            TimeLap addTimeLap = new TimeLap() { Date = DateTime.Now.ToString("yy년 MM월 dd일 HH시 mm분 ss초"), Content = pageTextBox.Text };

            timeLaps.Add(addTimeLap);

            timeLapUI add = new timeLapUI(timeLaps, tempTimeLaps, this.ForeColor, this.BackColor, ButtonUp);
            add.panel.DoubleClick += timeLapList_DoubleClick;
            add.text.DoubleClick += timeLapList_DoubleClick;

            uiTime.Add(add);

            timeLapLayoutPanel.Controls.Add(add.panel);
            tempTimeLaps = tempTimeLaps + 1;

        }

        private void saveMenuStrip_Click(object sender, EventArgs e)
        {
            savePage();
        }

        private void openMenuStrip_Click(object sender, EventArgs e)
        {
            loadPage();
        }

        private void asSave_Click(object sender, EventArgs e)
        {
            savePage(true);
        }

        private void optionMenuStrip_Click(object sender, EventArgs e)
        {
            using (OptionForm opForm = new OptionForm(thema[option.thema], option))
            {
                opForm.ShowDialog();

                pageTextBox.Font = new Font(option.fontName, option.fontSize);

                changeSkin(option.thema);

                switch (option.yoBack)
                {
                    case 0:
                        paddingL = paddingR = 6;
                        break;
                    case 1:
                        paddingL = paddingR = 30;
                        break;
                    case 2:
                        paddingL = paddingR = this.ClientSize.Width / 10;
                        break;
                    default:
                        break;
                }

                textBoxPanel.Padding = new Padding(paddingL, paddingTop, paddingR, 0);

                if (memoLayoutPanel.Visible)
                {
                    paddingR = 5;
                    textBoxPanel.Padding = new Padding(paddingL, paddingTop, paddingR, 0);
                }

                if (uiMemo.Count > 0 && memoLayoutPanel.Visible)
                {
                    foreach (memoUI m in uiMemo)
                    {
                        m.Content.Font = new Font(option.fontName, option.fontSize);
                    }
                }

                if (memoLayoutPanel.Visible && option.memoForm)
                {
                    memoLayoutPanel.Visible = wallline.Visible = false;
                    frmMemo mForm = new frmMemo(memos, thema[option.thema], this, memoStripMenu, memoToolMenu, option);

                    mForm.Show();
                }

                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm.Name == "frmMemo")
                    {
                        int tmpX = openForm.Location.X;
                        int tmpY = openForm.Location.Y;
                        openForm.Close();

                        if (option.memoForm)
                        {
                            frmMemo mForm = new frmMemo(memos, thema[option.thema], this, memoStripMenu, memoToolMenu, option);

                            mForm.Location = new Point(tmpX, tmpY);
                            mForm.Show();
                        }
                        else
                            memoLayoutPanel.Visible = wallline.Visible = true;

                        opForm.optionCopy = null;
                        return;
                    }
                }

                opForm.optionCopy = null;


            }
            ;

        }

        private void exitMenuStrip_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void undoMenuSTrip_Click(object sender, EventArgs e)
        {
            pageTextBox.Undo();
        }

        private void redoMenuStrip_Click(object sender, EventArgs e)
        {
            pageTextBox.Redo();
        }


        #endregion

        #region 세이브 관련

        public void savePage(bool asSave = false)
        {

            bool tCheck = false;

            if (string.IsNullOrEmpty(_path) || asSave)
            {
                SaveFileDialog _sv = new SaveFileDialog();
                _sv.Filter = "Page 문서|*.page|텍스트 문서 (UTF-8)|*.txt";
                _sv.FilterIndex = 1;

                if (_sv.ShowDialog() == DialogResult.OK)
                {
                    _path = _sv.FileName;
                    int len = Path.GetFileName(_sv.FileName).Length;
                    switch (_sv.FilterIndex)
                    {
                        case 1:

                            saveGroup _save = new saveGroup() { ver = 0, text = pageTextBox.Text, memos = memos, timeLaps = timeLaps };
                            var json = JsonSerializer.Serialize(_save);

                            using (var writer = new StreamWriter(_path))
                            {
                                writer.Write(json);
                            }

                            _name = Path.GetFileName(_sv.FileName).Substring(0, len - 5);


                            var deJson = JsonSerializer.Deserialize<saveGroup>(json);
                            deJson = null;

                            _save.memos = null;
                            _save.timeLaps = null;
                            _save = null;
                            _sv = null;
                            textSave = false;
                            break;
                        case 2:
                            if (memos.Count > 0)
                            {
                                DialogResult result = MessageBox.Show("텍스트 파일로 저장하면, 메모가 저장되지 않습니다. 그래도 저장하시겠습니까?", "경고", MessageBoxButtons.YesNo);

                                if (result == DialogResult.No)
                                {
                                    tCheck = true;
                                }
                            }

                            if (tCheck == false)
                            {
                                textSave = true;
                                string saveText = pageTextBox.Text;

                                _name = Path.GetFileName(_sv.FileName).Substring(0, len - 4);
                                File.WriteAllText(_sv.FileName, saveText, Encoding.UTF8);
                            }
                            break;
                    }

                    if (tCheck == false)
                    {
                        firstSave = true;

                        publicVar.chagedText = false;
                        isTextChanged = false;
                        UpdateTitle();
                    }

                }
            }

            if (!string.IsNullOrEmpty(_path))
            {
                if (textSave == false)
                {
                    saveGroup _save = new saveGroup() { ver = 0, text = pageTextBox.Text, memos = memos, timeLaps = timeLaps };
                    var json = JsonSerializer.Serialize(_save);

                    using (var writer = new StreamWriter(_path))
                    {
                        writer.Write(json);
                    }

                    var deJson = JsonSerializer.Deserialize<saveGroup>(json);
                    deJson = null;
                    json = null;

                    _save.memos = null;
                    _save.timeLaps = null;
                    _save = null;
                }
                else
                {
                    if (memos.Count > 0)
                    {
                        DialogResult result = MessageBox.Show("텍스트 파일로 저장하면, 메모가 저장되지 않습니다. 그래도 저장하시겠습니까?", "경고", MessageBoxButtons.YesNo);

                        if (result == DialogResult.No)
                        {
                            tCheck = true;
                        }
                    }

                    if (tCheck == false)
                    {
                        textSave = true;
                        string saveText = pageTextBox.Text;

                        File.WriteAllText(_path, saveText, Encoding.UTF8);
                    }
                }

                firstSave = true;

                publicVar.chagedText = false;
                isTextChanged = false;
                UpdateTitle();

            }
        }

        public void loadPage()
        {
            if (!checkSave())
            {
                OpenFileDialog _op = new OpenFileDialog();
                _op.Filter = "전체 파일|*.*";

                if (_op.ShowDialog() == DialogResult.OK)
                {
                    string filterIndex = Path.GetExtension(_op.FileName).ToLower();

                    if (filterIndex == ".page")
                    {
                        _path = _op.FileName;
                        saveGroup _load = new saveGroup();
                        int len = Path.GetFileName(_op.FileName).Length;
                        _name = Path.GetFileName(_op.FileName).Substring(0, len - 5);
                        textSave = false;
                        string loadJson;

                        using (var reader = new StreamReader(_op.FileName))
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                            };

                            loadJson = reader.ReadToEnd();
                            var loadObjects = JsonSerializer.Deserialize<saveGroup>(loadJson, options);
                            _load = loadObjects;

                            if (_load.ver < saveVersion)
                            {
                                MessageBox.Show("이전 버전과 호환이 되지 않아, 기존 페이지를 백업했습니다.");
                                File.Copy(_op.FileName, _op.FileName + "." + DateTime.Now.ToString("MMddhhmmss") + ".bak");
                            }

                            reSet(false);
                            pageTextBox.Text = _load.text;
                            timeLaps = _load.timeLaps;
                            memos = _load.memos;
                            isTextChanged = false;

                            tempMemos = 0;
                            tempTimeLaps = 0;

                            _load = null;
                            loadObjects = null;
                        }

                        _op = null;
                        firstSave = true;

                        UpdateTitle();
                        ResetTimeLap();
                        UpdateMemo();
                    }
                    else if (filterIndex == ".txt")
                    {
                        _path = _op.FileName;
                        int len = Path.GetFileName(_op.FileName).Length;
                        _name = Path.GetFileName(_op.FileName).Substring(0, len - 4);
                        string loadText = File.ReadAllText(_op.FileName, Encoding.UTF8);

                        reSet(false);
                        pageTextBox.Text = loadText;
                        isTextChanged = false;

                        tempMemos = 0;
                        tempTimeLaps = 0;
                        textSave = true;

                        _op = null;
                        firstSave = true;

                        UpdateTitle();
                        ResetTimeLap();
                        UpdateMemo();
                    }
                    else
                    {
                        MessageBox.Show("불러올 수 없는 파일입니다.", "오류");
                    }


                }
            }
        }

        public bool checkSave()
        {
            if (isTextChanged)
            {
                var result = MessageBox.Show("페이지를 저장하시겠습니까?", "Page", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                    savePage();
                else if (result == DialogResult.Cancel)
                    return true;
            }

            return false;
        }

        public void reSet(bool _new = true)
        {
            if (uiTime.Count > 0)
            {
                for (int i = uiTime.Count - 1; i >= 0; i--)
                {
                    uiTime[i].removeEventHandler();
                    uiTime[i].text.DoubleClick -= timeLapList_DoubleClick;
                    uiTime[i].panel.DoubleClick -= timeLapList_DoubleClick;
                    timeLapLayoutPanel.Controls.Remove(uiTime[i].panel);

                    uiTime.Remove(uiTime[i]);
                }

                timeLapLayoutPanel.Controls.Clear();
            }

            if (memoStripMenu.Count > 0)
            {
                for (int i = memoStripMenu.Count - 1; i >= 0; i--)
                {
                    memoToolMenu[i].Click -= memoTool_Click;
                    memoStripMenu[i].Opening -= memoStrip_Opening;
                    memoStripMenu[i].Items.Remove(memoToolMenu[i]);

                    memoToolMenu.Remove(memoToolMenu[i]);
                    memoStripMenu.Remove(memoStripMenu[i]);
                }
            }

            if (uiMemo.Count > 0)
            {
                for (int i = uiMemo.Count - 1; i >= 0; i--)
                {
                    uiMemo[i].removeHandler();
                    memoLayoutPanel.Controls.Remove(uiMemo[i].Panel);
                    uiMemo.Remove(uiMemo[i]);
                }
            }

            memoLayoutPanel.Controls.Clear();
            memoLayoutPanel.Controls.Add(addMemoLabel);

            if (memos.Count > 0)
            {
                for (int i = memos.Count - 1; i >= 0; i--)
                    memos.Remove(memos[i]);
            }

            if (timeLaps.Count > 0)
            {
                for (int i = timeLaps.Count - 1; i >= 0; i--)
                    timeLaps.Remove(timeLaps[i]);

            }
            tempMemos = 0;
            tempTimeLaps = 0;
            isTextChanged = false;

            uiTime.Clear();
            memoStripMenu.Clear();
            memoToolMenu.Clear();
            uiMemo.Clear();
            memos.Clear();
            timeLaps.Clear();

            memos = new List<Memo>();
            timeLaps = new List<TimeLap>();
            uiMemo = new List<memoUI>();
            uiTime = new List<timeLapUI>();
            memoStripMenu = new List<ContextMenuStrip>();
            memoToolMenu = new List<ToolStripMenuItem>();

            if (_new)
            {
                pageTextBox.Clear();
                _name = "무제";
                _path = null;
            }


        }

        #endregion

        #region UI Event Handler
        // Label에서 마우스 버튼을 뗐을 때의 이벤트 핸들러
        private void UIButtonUp(object sender, MouseEventArgs e)
        {
            if (sender is Label)
            {
                Label label = sender as Label;
                label.BackColor = ButtonUp;

                if (!checkButton)
                    label.BackColor = Color.Transparent;

            }
        }
        // Label에서 마우스 버튼을 눌렀을 때의 이벤트 핸들러
        private void UIButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is Label)
            {
                Label label = sender as Label;
                label.BackColor = ButtonDown;
            }
        }
        // Label에 마우스를 올렸을 때의 이벤트 핸들러
        private void UIButtonEnter(object sender, EventArgs e)
        {
            checkButton = true;
            if (sender is Label)
            {
                Label label = sender as Label;
                label.BackColor = ButtonUp;

                if (!checkButton)
                    label.BackColor = Color.Transparent;
            }
        }
        // Label에서 마우스를 뗐을 때의 이벤트 핸들러
        private void UIButtonLeave(object sender, EventArgs e)
        {
            checkButton = false;
            if (sender is Label)
            {
                Label label = sender as Label;
                label.BackColor = Color.Transparent;
            }
        }
        #endregion

        private void pageTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!isTextChanged)
            {
                isTextChanged = true;
                UpdateTitle();
            }
        }

        public void UpdateTitle()
        {
            StringBuilder _tex = new StringBuilder();

            _tex.Append(_name);
            _tex.Append((isTextChanged ? "*" : string.Empty));
            nameLabel.Text = _tex.ToString();

            if (option.focuse)
                this.Text = "빈 노트 :: Focuse Mode";
            else
                this.Text = "빈 노트";
#if DEBUG
            this.Text = "빈 노트 :: DebugMode";
#endif
        }

        #region 메모 관련

        public void UpdateMemo()
        {
            if (memoStripMenu.Count > 0)
            {
                for (int i = memoStripMenu.Count - 1; i >= 0; i--)
                {
                    memoToolMenu[i].Click -= memoTool_Click;
                    memoStripMenu[i].Opening -= memoStrip_Opening;
                    memoStripMenu[i].Items.Remove(memoToolMenu[i]);

                    memoToolMenu.Remove(memoToolMenu[i]);
                    memoStripMenu.Remove(memoStripMenu[i]);
                }
                memoStripMenu.Clear();
                memoToolMenu.Clear();

                memoStripMenu = new List<ContextMenuStrip>();
                memoToolMenu = new List<ToolStripMenuItem>();
            }

            if (uiMemo.Count > 0)
            {
                for (int i = uiMemo.Count - 1; i >= 0; i--)
                {
                    uiMemo[i].removeHandler();
                    memoLayoutPanel.Controls.Remove(uiMemo[i].Panel);
                    uiMemo.Remove(uiMemo[i]);
                }
                uiMemo.Clear();

                uiMemo = new List<memoUI>();
            }

            memoLayoutPanel.Controls.Clear();

            if (memos.Count > 0)
            {
                for (int i = 0; i < memos.Count; i++)
                {
                    memoUI add = new memoUI(memos, i, this.BackColor, this.ForeColor, thema[option.thema].BorderColor, option.fontSize, option.fontName);
                    ContextMenuStrip addMenu = new ContextMenuStrip()
                    {
                        Tag = i,
                        BackColor = Color.FromArgb(254, 254, 254),
                        ShowImageMargin = false
                    };
                    ToolStripMenuItem addTool = new ToolStripMenuItem()
                    {
                        Text = "⌦ 삭제",
                        Font = new Font("나눔 고딕", 10)
                    };

                    addMenu.Items.Add(addTool);
                    add.Content.ContextMenuStrip = addMenu;

                    addMenu.Opening += memoStrip_Opening;
                    addTool.Click += memoTool_Click;

                    memoStripMenu.Add(addMenu);
                    memoToolMenu.Add(addTool);
                    uiMemo.Add(add);
                    memoLayoutPanel.Controls.Add(uiMemo[i].Panel);
                }

            }
            tempMemos = memos.Count;

            memoLayoutPanel.Controls.Add(addMemoLabel);

            changeMemoSize();

        }
        private void memoStrip_Opening(object sender, CancelEventArgs e)
        {
            Control source = ((ContextMenuStrip)sender).SourceControl;
            selectedMemo = (int)source.Tag;
        }

        private void memoTool_Click(object sender, EventArgs e)
        {
            memos.RemoveAt(selectedMemo);
            UpdateMemo();
        }

        #endregion

        #region 타임랩 관련

        public void ResetTimeLap()
        {
            if (uiTime.Count > 0)
            {
                for (int i = uiTime.Count - 1; i >= 0; i--)
                {
                    uiTime[i].removeEventHandler();
                    uiTime[i].text.DoubleClick -= timeLapList_DoubleClick;
                    uiTime[i].panel.DoubleClick -= timeLapList_DoubleClick;
                    timeLapLayoutPanel.Controls.Remove(uiTime[i].panel);

                    uiTime.Remove(uiTime[i]);
                }

                uiTime.Clear();
                timeLapLayoutPanel.Controls.Clear();
            }

            if (timeLaps.Count > 0)
            {
                for (int i = 0; i < timeLaps.Count; i++)
                {
                    timeLapUI add = new timeLapUI(timeLaps, i, this.ForeColor, this.BackColor, ButtonUp);
                    add.panel.DoubleClick += timeLapList_DoubleClick;
                    add.text.DoubleClick += timeLapList_DoubleClick;

                    uiTime.Add(add);

                    timeLapLayoutPanel.Controls.Add(add.panel);
                }
            }
            tempTimeLaps = timeLaps.Count;
        }

        public void timeLapList_DoubleClick(object sender, EventArgs e)
        {
            Form tf = Application.OpenForms["frmTimeLap"];

            if (tf != null)
                tf.Close();

            int i = -1;

            if (sender is customLabel)
            {
                customLabel source = sender as customLabel;
                i = (int)source.Tag;
            }

            if (sender is Panel)
            {
                Panel source = sender as Panel;
                i = (int)source.Tag;
            }

            frmTimeLap timeLapForm = new frmTimeLap(timeLaps[i], thema[option.thema], this, i);

            timeLapForm.Show();
        }

        public void removeAtTimeLap(int loc)
        {
            timeLaps.RemoveAt(loc);
            ResetTimeLap();
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            pageTextBox.Clear();
            pageTextBox.AppendText(timeLaps.Count + "\n");
            if (timeLaps.Count > 0)
            {
                foreach (TimeLap t in timeLaps)
                {
                    pageTextBox.AppendText(t.Date + "\n");
                }
            }
        }

        private void newMenuStrip_Click(object sender, EventArgs e)
        {
            if (isTextChanged)
            {
                DialogResult dr = MessageBox.Show("페이지를 저장히시겠습니까?", "안내", MessageBoxButtons.YesNoCancel);

                if (dr == DialogResult.Yes)
                {
                    savePage();
                    reSet();
                    isTextChanged = false;

                    UpdateTitle();
                }
                else if (dr == DialogResult.No)
                {
                    reSet();
                    isTextChanged = false;

                    UpdateTitle();
                }
            }
            else
            {
                reSet();
                isTextChanged = false;

                UpdateTitle();
            }
        }

        #region 테마 관련
        public void loadThema()
        {
            //Light

            thema.Add(new Thema()
            {
                mainBackColor = Color.FromArgb(254, 254, 254),
                mainForeColor = Color.FromArgb(24, 24, 24),
                menuBackColor = Color.FromArgb(247, 247, 247),
                menuForeColor = Color.FromArgb(52, 52, 52),
                lineColor = Color.FromArgb(222, 222, 222),
                buttonUpColor = Color.FromArgb(232, 232, 232),
                buttonDownColor = Color.FromArgb(212, 212, 212),
                BorderColor = Color.FromArgb(182, 182, 182)
            });

            //Dark
            thema.Add(new Thema()
            {
                mainBackColor = Color.FromArgb(40, 44, 52),
                mainForeColor = Color.FromArgb(211, 211, 211),
                menuBackColor = Color.FromArgb(31, 31, 31),
                menuForeColor = Color.FromArgb(132, 132, 132),
                lineColor = Color.FromArgb(45, 45, 45),
                buttonUpColor = Color.FromArgb(69, 69, 69),
                buttonDownColor = Color.FromArgb(43, 43, 43),
                BorderColor = Color.FromArgb(65, 65, 65)
            });

            //Solarized White
            thema.Add(new Thema()
            {
                mainBackColor = Color.FromArgb(253, 246, 227),
                mainForeColor = Color.FromArgb(10, 17, 14),
                menuBackColor = Color.FromArgb(250, 230, 175),
                menuForeColor = Color.FromArgb(10, 10, 10),
                lineColor = Color.FromArgb(221, 218, 180),
                buttonUpColor = Color.FromArgb(247, 238, 217),
                buttonDownColor = Color.FromArgb(206, 158, 112),
                BorderColor = Color.FromArgb(232, 213, 179)
            });

            //Solarized Dark
            thema.Add(new Thema()
            {
                mainBackColor = Color.FromArgb(0, 57, 70),
                mainForeColor = Color.FromArgb(147, 161, 161),
                menuBackColor = Color.FromArgb(0, 108, 130),
                menuForeColor = Color.FromArgb(194, 203, 203),
                lineColor = Color.FromArgb(0, 77, 110),
                buttonUpColor = Color.FromArgb(0, 189, 230),
                buttonDownColor = Color.FromArgb(0, 78, 94),
                BorderColor = Color.FromArgb(20, 97, 130)
            });
        }

        public void changeSkin(int i)
        {
            this.BackColor = pageTextBox.BackColor = thema[i].mainBackColor;
            this.ForeColor = pageTextBox.ForeColor = thema[i].mainForeColor;
            menuPanel.BackColor = thema[i].menuBackColor;
            menuPanel.ForeColor = thema[i].menuForeColor;

            ButtonUp = thema[i].buttonUpColor;
            ButtonDown = thema[i].buttonDownColor;

            wallline.BackColor = label1.BackColor = wallLabel.BackColor = thema[i].lineColor;


            if (uiTime.Count > 0)
            {
                foreach (timeLapUI t in uiTime)
                {
                    t.color(this.ForeColor, ButtonUp, this.BackColor);
                }
            }

            if (uiMemo.Count > 0)
            {
                foreach (memoUI m in uiMemo)
                {
                    m.changeSkin(this.BackColor, this.ForeColor, thema[option.thema].BorderColor);
                }
            }
        }

        private void menuPanel_ForeColorChanged(object sender, EventArgs e)
        {
            newLabel.ForeColor = menuPanel.ForeColor;
            saveLabel.ForeColor = menuPanel.ForeColor;
            asSaveLabel.ForeColor = menuPanel.ForeColor;
            openLabel.ForeColor = menuPanel.ForeColor;
            optionLabel.ForeColor = menuPanel.ForeColor;
            label3.ForeColor = menuPanel.ForeColor;
            sideButton.ForeColor = menuPanel.ForeColor;
        }

        #endregion

        private void button1_Click_1(object sender, EventArgs e)
        {
            option.thema = 0;
            changeSkin(0);
        }

        private void 복사ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (publicVar.selectMemo)
                {
                    uiMemo[publicVar.selectMemoIndex].paste_Content(clipboardText);
                }
                else
                {
                    pageTextBox.SelectionFont = pageTextBox.Font;
                    pageTextBox.SelectionColor = pageTextBox.ForeColor;
                    pageTextBox.SelectedText = clipboardText;
                }
            }
        }

        private void 포커스모드ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            option.focuse = !option.focuse;

            if (option.focuse)
            {
                paddingTop = 30;
                TopPanel.Visible = false;
                timeLapLabel.Visible = false;
                menuPanel.Visible = false;
                memoLabel.Visible = false;
                memoLayoutPanel.Visible = false;
                timeLapPanel.Visible = false;
                lnColLabel.Visible = false;
                wallLabel.Visible = false;
            }
            else
            {
                paddingTop = 2;
                TopPanel.Visible = true;
                timeLapLabel.Visible = true;
                menuPanel.Visible = true;
                memoLabel.Visible = true;
                timeLapPanel.Visible = true;
                lnColLabel.Visible = true;
                wallLabel.Visible = true;
            }


            if (memoLayoutPanel.Visible)
                paddingR = 5;

            textBoxPanel.Padding = new Padding(paddingL, paddingTop, paddingR, 0);
            UpdateTitle();
        }

        private void line_mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void line_mouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                int newWidth = memoLayoutPanel.Width - dif.X;
                if (newWidth > 0 && newWidth < mainPanel.Width) // 폼의 크기를 고려
                {
                    memoLayoutPanel.Width = newWidth;
                }
                dragCursorPoint = Cursor.Position;
            }
        }

        private void line_mouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
                memoLayoutPanel.Width = memoLayoutPanel.Width + 1;
                changeMemoSize();
            }
        }

        public void changeMemoSize()
        {
            if (memoLayoutPanel.VerticalScroll.Visible)
            {
                if (uiMemo.Count > 0)
                {
                    foreach (memoUI m in uiMemo)
                    {
                        m.changeSize(memoLayoutPanel.Width - 24);
                    }
                }
                addMemoLabel.Width = memoLayoutPanel.Width - 24;
            }
            else
            {
                if (uiMemo.Count > 0)
                {
                    foreach (memoUI m in uiMemo)
                    {
                        m.changeSize(memoLayoutPanel.Width - 12);
                    }
                }
                addMemoLabel.Width = memoLayoutPanel.Width - 12;
            }
        }

        #region 추가 코드

        public void saveOptions()
        {
        }

        public void loadOptions()
        {
        }

        #endregion

        private void TempButton_Click(object sender, EventArgs e)
        {
#if DEBUG
            string logPath = "../VersionCountLog.txt";

            using (StreamWriter logwrite = new StreamWriter(logPath, true))
            {
                logwrite.WriteLine("[최종] Debug Verison 1." + DateTime.Now.ToString("yy.MM.dd") + _versionCounter + _versionType);
            }

            _versionCounter = 0;
            app.Default.counter = _versionCounter;
            app.Default.Save();
#endif
        }

        private void pageTextBox_Click(object sender, EventArgs e)
        {
            publicVar.selectMemo = false;
        }
    }
}
