using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PROG_Part3
{
    // Data models and basic fields
    public class Cybertask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reminder { get; set; } //e.g . "2023-12-31" or "in 3 days"
        public bool Completed { get; set; }
    }
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public string Answer { get; set; }
        public string Explanation { get; set; }
        public bool IsTrueFalse { get; set; } // For true/false questions
    }
    public partial class Form1 : Form
    {
        
        // MY SQL connection string
        private const string ConnStr =
        "Server=localhost;Database=cybertask_db;Uid=root;Pwd=;";

        //Memory and state
        private string username = "";
        private string favouriteTopic = "";
        private string lastTopic = "";
        private bool waitingForName = true;
        private bool isTyping = false;

        //Quiz state
        private bool inQuiz = false;
        private int quizIndex = 0;
        private int quizScore = 0;
        private bool awaitingQuizAnswer = false;
        private List<QuizQuestion> quizQuestions;

        //Task assistant state
        private bool awaitingTaskTitle = false;
        private bool awaitingReminder = false;
        private string pendingTaskTitle = "";
        private string pendingTaskDesc = "";

        //Activity log
        private List<string> activityLog = new List<string>();

        //Keyword responses
        // ─── Keyword Responses ────────────────────────────────────────────────
        private Dictionary<string, List<string>> keywordResponses =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
            {
                "Use a strong password with a mix of letters, numbers, and symbols.",
                "Never reuse the same password across multiple sites. Consider a password manager like Bitwarden.",
                "Enable two-factor authentication (2FA) alongside a strong password for maximum security.",
                "Follow your organisation's policies when creating passwords."
            },
                ["phishing"] = new List<string>
            {
                "Phishing tricks you into revealing sensitive info through fake emails. Always check the sender's address!",
                "Be cautious of emails asking for personal information. Scammers disguise themselves as trusted organisations.",
                "Never click links in unexpected emails — type the website address yourself instead."
            },
                ["browsing"] = new List<string>
            {
                "Always use secure (https) websites and avoid unknown links.",
                "Consider uBlock Origin to block malicious ads and trackers.",
                "Keep your browser updated — updates include important security patches."
            },
                ["privacy"] = new List<string>
            {
                "Limit what you share online and review your app permissions regularly.",
                "Review your social media privacy settings to control who sees your information.",
                "Be mindful of personal data you share — once it's out there, it's hard to take back."
            },
                ["malware"] = new List<string>
            {
                "Malware is harmful software that damages your device or steals data. Avoid files from unknown sources!",
                "Keep your antivirus software up to date to defend against the latest threats.",
                "Regularly scan your device and be cautious of unexpected pop-ups."
            },
                ["scam"] = new List<string>
            {
                "Online scams try to trick you into giving away money or personal information.",
                "If an offer seems too good to be true, it probably is.",
                "Scammers create urgency — slow down and verify any unexpected requests."
            },
                ["wifi"] = new List<string>
            {
                "Avoid logging into sensitive accounts like banking on public Wi-Fi!",
                "Use a VPN on public Wi-Fi to protect your browsing data.",
                "Use your mobile data instead of public Wi-Fi for sensitive transactions."
            },
                ["virus"] = new List<string>
            {
                "Computer viruses are malicious programs. Use antivirus software and avoid unknown files!",
                "Keep your OS updated — many viruses exploit outdated software.",
                "Never open email attachments from unknown senders."
            }
            };

        //NLP synonym map(maps alternative words to a standard keyword)
        private Dictionary<string, string> nlpSynonms = 
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["pass"] = "password",
                ["passwords"] = "password",
                ["credentials"] = "password",
                ["login"] = "password",
                ["phish"] = "phishing",
                ["spam"] = "phishing",
                ["fake email"] = "phishing",
                ["web"] = "browsing",
                ["internet"] = "browsing",
                ["surfing"] = "browsing",
                ["private"] = "privacy",
                ["data"] = "privacy",
                ["personal info"] = "privacy",
                ["virus"] = "virus",
                ["viruses"] = "virus",
                ["hack"] = "malware",
                ["hacker"] = "malware",
                ["ransomware"] = "malware",
                ["spyware"] = "malware",
                ["fraud"] = "scam",
                ["trick"] = "scam",
                ["network"] = "wifi",
                ["hotspot"] = "wifi",
                ["public wifi"] = "wifi"
            };
           
        //Sentiment map
        private Dictionary<string, string> sentimentPrefixes =
            new Dictionary<string, string(StringComparer.OrdinalIgnoreCase)
        {
            ["worried"]     = "It's understandable to feel that way. Here are some tips: ",
            ["scared"]      = "Don't worry — learning about this is already the right step. ",
            ["frustrated"]  = "I hear you — cybersecurity can feel overwhelming. Let's break it down: ",
            ["confused"]    = "No problem! Let me explain more clearly. ",
            ["curious"]     = "Great curiosity! Staying informed is the best defence. ",
            ["anxious"]     = "Take a deep breath — awareness is the first step to staying safe online. ",
            ["angry"]       = "Let's channel that energy into staying better protected. ",
            ["overwhelmed"] = "Let's take it one step at a time. Here's one thing you can do: "
        };

        //Follow up phrases
        private List<string> followUpPhrases = new List<string>
        {
            "tell me more", "another tip", "give me more", "explain more", "more info",
            "elaborate", "continue", "more please", "go on"
        };

        private Random rng = new Random();
    public Form1()
        {
            InitializeComponent();
            InitialiseQuizQuestions();
            EnsureDatabaseExists();
            this.Load += Form1_Load;
        }
        //Form load
        private void Form1_Load(object? sender, EventArgs e)
        {
            PlayVoiceGreeting();
            ShowAsciiLogo();
            AppendDivider();
            AppendSection("Welcome");
            AppendBot("Hello! I am your CyberSecurity Awareness Bot. 🛡");
            AppendBot("Ask me about password security, phishing, or safe browsing.");
            AppendBot("Type 'quiz' to start the mini-game, 'tasks' to manage tasks, or 'activity log' to view recent actions.");
            AppendDivider();
            AppendBot("Please enter your name to get started:");
        }
        //Send/enter buttons
        private void btnSend_Click(object sender, EventArgs e) => ProcessInput();

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ProcessInput(); }
        }
        private void ProcessInput()
        {
            if (isTyping) return;
            string input = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AppendUser(input);
            txtInput_KeyDown().Clear();
        }
    }
}