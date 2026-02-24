// MainForm.cs - Основная форма приложения
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DecisionTreeApp
{
    public partial class MainForm : Form
    {
        private DecisionNode rootNode;
        private DecisionNode currentNode;
        private List<Tuple<string, string>> history;
        private TreeNode currentTreeNode;

        public MainForm()
        {
            InitializeComponent();
            InitializeDecisionTree();
            history = new List<Tuple<string, string>>();
            DisplayCurrentNode();
        }

        private void InitializeComponent()
        {
            this.Text = "Дерево решений";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Левая панель - TreeView
            treeView = new TreeView
            {
                Location = new Point(10, 10),
                Size = new Size(350, 530),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            treeView.AfterSelect += TreeView_AfterSelect;
            this.Controls.Add(treeView);

            // Правая панель
            int rightPanelX = 380;

            // Заголовок текущего вопроса
            lblQuestionTitle = new Label
            {
                Text = "Текущий вопрос",
                Location = new Point(rightPanelX, 10),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblQuestionTitle);

            // Панель текущего вопроса
            panelQuestion = new Panel
            {
                Location = new Point(rightPanelX, 35),
                Size = new Size(480, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblQuestion = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(440, 40),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            panelQuestion.Controls.Add(lblQuestion);

            btnYes = new Button
            {
                Text = "Да",
                Location = new Point(130, 70),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnYes.FlatAppearance.BorderSize = 0;
            btnYes.Click += BtnYes_Click;
            panelQuestion.Controls.Add(btnYes);

            btnNo = new Button
            {
                Text = "Нет",
                Location = new Point(250, 70),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNo.FlatAppearance.BorderSize = 0;
            btnNo.Click += BtnNo_Click;
            panelQuestion.Controls.Add(btnNo);

            this.Controls.Add(panelQuestion);

            // Заголовок результата
            lblResultTitle = new Label
            {
                Text = "Результат",
                Location = new Point(rightPanelX, 170),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblResultTitle);

            // Панель результата
            panelResult = new Panel
            {
                Location = new Point(rightPanelX, 195),
                Size = new Size(480, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblResult = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(440, 40),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue,
                AutoSize = false
            };
            panelResult.Controls.Add(lblResult);
            this.Controls.Add(panelResult);

            // Заголовок истории
            lblHistoryTitle = new Label
            {
                Text = "История ответов",
                Location = new Point(rightPanelX, 290),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblHistoryTitle);

            // История ответов
            txtHistory = new TextBox
            {
                Location = new Point(rightPanelX, 315),
                Size = new Size(480, 150),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
            this.Controls.Add(txtHistory);

            // Панель управления
            panelControls = new Panel
            {
                Location = new Point(rightPanelX, 480),
                Size = new Size(480, 60),
                BackColor = Color.Transparent
            };

            btnBack = new Button
            {
                Text = "← Назад",
                Location = new Point(130, 10),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;
            panelControls.Controls.Add(btnBack);

            btnReset = new Button
            {
                Text = "↺ Сброс",
                Location = new Point(250, 10),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            panelControls.Controls.Add(btnReset);

            this.Controls.Add(panelControls);
        }

        // Элементы управления
        private TreeView treeView;
        private Label lblQuestionTitle;
        private Panel panelQuestion;
        private Label lblQuestion;
        private Button btnYes;
        private Button btnNo;
        private Label lblResultTitle;
        private Panel panelResult;
        private Label lblResult;
        private Label lblHistoryTitle;
        private TextBox txtHistory;
        private Panel panelControls;
        private Button btnBack;
        private Button btnReset;

        private void InitializeDecisionTree()
        {
            // Построение дерева решений согласно заданию
            rootNode = new DecisionNode { Question = "Запускать новый продукт?" };

            // Ветка "Да" - Высокий спрос
            var highDemand = new DecisionNode { Question = "Высокий спрос?", Parent = rootNode };
            rootNode.YesNode = highDemand;

            // Высокий спрос → Да
            var highProfit = new DecisionNode { Question = "Высокая прибыль?", Parent = highDemand };
            highDemand.YesNode = highProfit;

            // Высокая прибыль → Да
            var lowCompetition = new DecisionNode { Question = "Низкая конкуренция?", Parent = highProfit };
            highProfit.YesNode = lowCompetition;

            lowCompetition.YesNode = new DecisionNode { Result = "Запускать немедленно", Parent = lowCompetition };
            lowCompetition.NoNode = new DecisionNode { Result = "Нужно УТП", Parent = lowCompetition };

            // Высокая прибыль → Нет
            var highCompetition = new DecisionNode { Question = "Высокая конкуренция?", Parent = highProfit };
            highProfit.NoNode = highCompetition;

            highCompetition.YesNode = new DecisionNode { Result = "Анализировать конкурентов", Parent = highCompetition };
            highCompetition.NoNode = new DecisionNode { Result = "Искать нишу", Parent = highCompetition };

            // Высокий спрос → Нет
            var lowProfit = new DecisionNode { Question = "Низкая прибыль?", Parent = highDemand };
            highDemand.NoNode = lowProfit;

            // Низкая прибыль → Да
            var lowCost = new DecisionNode { Question = "Низкая себестоимость?", Parent = lowProfit };
            lowProfit.YesNode = lowCost;

            lowCost.YesNode = new DecisionNode { Result = "Запускать с осторожностью", Parent = lowCost };
            lowCost.NoNode = new DecisionNode { Result = "Оптимизировать производство", Parent = lowCost };

            // Низкая прибыль → Нет
            var highCost = new DecisionNode { Question = "Высокая себестоимость?", Parent = lowProfit };
            lowProfit.NoNode = highCost;

            highCost.YesNode = new DecisionNode { Result = "Искать инвесторов", Parent = highCost };
            highCost.NoNode = new DecisionNode { Result = "Пересмотреть бизнес-план", Parent = highCost };

            // Ветка "Нет" - Низкий спрос
            var lowDemand = new DecisionNode { Question = "Низкий спрос?", Parent = rootNode };
            rootNode.NoNode = lowDemand;

            // Низкий спрос → Да
            var seasonality = new DecisionNode { Question = "Есть сезонность?", Parent = lowDemand };
            lowDemand.YesNode = seasonality;

            // Сезонность → Да
            var peakSoon = new DecisionNode { Question = "Пик спроса скоро?", Parent = seasonality };
            seasonality.YesNode = peakSoon;

            peakSoon.YesNode = new DecisionNode { Result = "Готовиться к запуску", Parent = peakSoon };
            peakSoon.NoNode = new DecisionNode { Result = "Отложить запуск", Parent = peakSoon };

            // Сезонность → Нет
            var peakNotSoon = new DecisionNode { Question = "Пик спроса не скоро?", Parent = seasonality };
            seasonality.NoNode = peakNotSoon;

            peakNotSoon.YesNode = new DecisionNode { Result = "Развивать в низкий сезон", Parent = peakNotSoon };
            peakNotSoon.NoNode = new DecisionNode { Result = "Искать другой рынок", Parent = peakNotSoon };

            // Низкий спрос → Нет
            lowDemand.NoNode = new DecisionNode { Result = "Отказаться от запуска", Parent = lowDemand };

            currentNode = rootNode;
            BuildTreeView();
        }

        private void BuildTreeView()
        {
            treeView.Nodes.Clear();
            var rootTreeNode = new TreeNode(rootNode.Question) { Tag = rootNode };
            treeView.Nodes.Add(rootTreeNode);
            AddNodesRecursive(rootTreeNode, rootNode);
            treeView.ExpandAll();
        }

        private void AddNodesRecursive(TreeNode treeNode, DecisionNode decisionNode)
        {
            if (decisionNode.YesNode != null)
            {
                var yesTreeNode = new TreeNode("Да: " + decisionNode.YesNode.DisplayText)
                {
                    Tag = decisionNode.YesNode,
                    ForeColor = decisionNode.YesNode.IsResult ? Color.Green : Color.Black
                };
                treeNode.Nodes.Add(yesTreeNode);
                AddNodesRecursive(yesTreeNode, decisionNode.YesNode);
            }

            if (decisionNode.NoNode != null)
            {
                var noTreeNode = new TreeNode("Нет: " + decisionNode.NoNode.DisplayText)
                {
                    Tag = decisionNode.NoNode,
                    ForeColor = decisionNode.NoNode.IsResult ? Color.Green : Color.Black
                };
                treeNode.Nodes.Add(noTreeNode);
                AddNodesRecursive(noTreeNode, decisionNode.NoNode);
            }
        }

        private void DisplayCurrentNode()
        {
            lblQuestion.Text = currentNode.Question;

            if (currentNode.IsResult)
            {
                lblResult.Text = currentNode.Result;
                lblQuestionTitle.Text = "РЕЗУЛЬТАТ";
                lblQuestionTitle.ForeColor = Color.Red;
                lblQuestion.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                panelQuestion.BackColor = Color.FromArgb(232, 245, 233);

                btnYes.Enabled = false;
                btnNo.Enabled = false;
                btnYes.Visible = false;
                btnNo.Visible = false;
            }
            else
            {
                lblResult.Text = "";
                lblQuestionTitle.Text = "Текущий вопрос";
                lblQuestionTitle.ForeColor = Color.Gray;
                lblQuestion.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                panelQuestion.BackColor = Color.White;

                btnYes.Enabled = true;
                btnNo.Enabled = true;
                btnYes.Visible = true;
                btnNo.Visible = true;
            }

            btnBack.Enabled = currentNode.Parent != null;
            HighlightCurrentNode();
            UpdateHistory();
        }

        private void HighlightCurrentNode()
        {
            // Сброс подсветки
            ResetTreeNodeColors(treeView.Nodes);

            // Поиск и подсветка текущего узла
            currentTreeNode = FindTreeNode(treeView.Nodes, currentNode);
            if (currentTreeNode != null)
            {
                currentTreeNode.BackColor = Color.FromArgb(33, 150, 243);
                currentTreeNode.ForeColor = Color.White;
                treeView.SelectedNode = currentTreeNode;
                currentTreeNode.EnsureVisible();
            }
        }

        private void ResetTreeNodeColors(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.White;
                if (node.Tag is DecisionNode dn && dn.IsResult)
                    node.ForeColor = Color.Green;
                else
                    node.ForeColor = Color.Black;
                ResetTreeNodeColors(node.Nodes);
            }
        }

        private TreeNode FindTreeNode(TreeNodeCollection nodes, DecisionNode target)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag == target) return node;
                var found = FindTreeNode(node.Nodes, target);
                if (found != null) return found;
            }
            return null;
        }

        private void UpdateHistory()
        {
            txtHistory.Text = "";
            foreach (var item in history)
            {
                txtHistory.Text += $"Вопрос: {item.Item1} → Ответ: {item.Item2}\r\n";
            }
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            if (currentNode.YesNode != null)
            {
                history.Add(new Tuple<string, string>(currentNode.Question, "Да"));
                currentNode = currentNode.YesNode;
                DisplayCurrentNode();
            }
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            if (currentNode.NoNode != null)
            {
                history.Add(new Tuple<string, string>(currentNode.Question, "Нет"));
                currentNode = currentNode.NoNode;
                DisplayCurrentNode();
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (currentNode.Parent != null)
            {
                // Удаляем последнюю запись из истории
                if (history.Count > 0)
                    history.RemoveAt(history.Count - 1);

                currentNode = currentNode.Parent;
                DisplayCurrentNode();
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            currentNode = rootNode;
            history.Clear();
            DisplayCurrentNode();
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Можно добавить навигацию по клику на узел дерева
        }
    }
}